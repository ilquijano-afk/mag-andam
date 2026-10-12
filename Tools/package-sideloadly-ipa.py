#!/usr/bin/env python3
"""Package an unsigned device .app for Sideloadly, preserving executable modes."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import plistlib
import stat
import time
import zipfile


def excluded(relative):
    return any(part in ("_CodeSignature", "__MACOSX", "embedded.mobileprovision")
               or part.startswith("._") for part in relative.parts)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("app", type=Path)
    parser.add_argument("ipa", type=Path)
    parser.add_argument("--expected-version", required=True)
    parser.add_argument("--expected-build", required=True)
    args = parser.parse_args()
    app = args.app.resolve()
    with (app / "Info.plist").open("rb") as stream:
        info = plistlib.load(stream)
    if info.get("CFBundleShortVersionString") != args.expected_version or str(info.get("CFBundleVersion")) != args.expected_build:
        raise RuntimeError("Compiled app version does not match the requested update")
    prefix = Path("Payload") / app.name
    args.ipa.parent.mkdir(parents=True, exist_ok=True)
    source_hashes = {}
    executable_names = [str(prefix / info["CFBundleExecutable"])]
    for framework in (app / "Frameworks").glob("*.framework"):
        with (framework / "Info.plist").open("rb") as stream:
            framework_info = plistlib.load(stream)
        executable_names.append(str(prefix / framework.relative_to(app) / framework_info["CFBundleExecutable"]))
    # Stored plist entries also avoid Sideloadly 0.70.1's framework signing failure.
    with zipfile.ZipFile(args.ipa, "w", compression=zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for path in sorted(app.rglob("*")):
            relative = path.relative_to(app)
            if excluded(relative) or (path.is_dir() and not path.is_symlink()):
                continue
            name = str(prefix / relative)
            metadata = path.lstat()
            entry = zipfile.ZipInfo(name, time.localtime(metadata.st_mtime)[:6])
            entry.create_system = 3
            entry.external_attr = (metadata.st_mode & 0xFFFF) << 16
            entry.compress_type = zipfile.ZIP_STORED if path.suffix.lower() == ".plist" else zipfile.ZIP_DEFLATED
            data = os.readlink(path).encode() if path.is_symlink() else path.read_bytes()
            archive.writestr(entry, data)
            source_hashes[name] = digest(data)
    with zipfile.ZipFile(args.ipa) as archive:
        if archive.testzip() is not None:
            raise RuntimeError("IPA integrity check failed")
        for entry in archive.infolist():
            if excluded(Path(entry.filename)) or digest(archive.read(entry)) != source_hashes[entry.filename]:
                raise RuntimeError("IPA payload verification failed: " + entry.filename)
            if entry.filename.lower().endswith(".plist") and entry.compress_type != zipfile.ZIP_STORED:
                raise RuntimeError("Plist must be stored without compression")
        for name in executable_names:
            mode = archive.getinfo(name).external_attr >> 16
            if not stat.S_ISREG(mode) or mode & 0o111 == 0:
                raise RuntimeError("Missing executable permissions: " + name)
    report = {
        "ipa": str(args.ipa.resolve()),
        "version": args.expected_version,
        "build": args.expected_build,
        "bundle_identifier": info["CFBundleIdentifier"],
        "sha256": digest(args.ipa.read_bytes()),
        "bytes": args.ipa.stat().st_size,
        "entries": len(source_hashes),
        "integrity_verified": True,
        "payload_matches_compiled_app": True,
        "executable_permissions_verified": executable_names,
        "plist_compression": "stored",
        "old_signatures_and_appledouble": "excluded",
        "signing": "unsigned; Sideloadly signs during installation",
    }
    args.ipa.with_suffix(".json").write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({key: report[key] for key in ("ipa", "version", "build", "bytes", "integrity_verified")}))


if __name__ == "__main__":
    main()

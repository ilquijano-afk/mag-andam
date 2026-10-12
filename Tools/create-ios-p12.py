#!/usr/bin/env python3
"""Package the issued Apple Development certificate with the matching local key."""
import argparse
import getpass
from pathlib import Path
import re
import subprocess
import tempfile


def openssl(*arguments, data=None):
    result = subprocess.run(["openssl", *map(str, arguments)], input=data,
                            capture_output=True, text=True)
    if result.returncode:
        raise SystemExit("OpenSSL could not complete this step: " + result.stderr.strip())
    return result.stdout


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("certificate", type=Path, help="Apple's downloaded .cer file")
    args = parser.parse_args()
    certificate = args.certificate.expanduser().resolve()
    signing = Path(__file__).resolve().parents[2] / "iOS-Signing"
    key = signing / "ANDAM-Apple-Development.key"
    output = signing / "ANDAM-Apple-Development.p12"
    if not certificate.is_file() or not key.is_file():
        raise SystemExit("The downloaded certificate and the original local signing key are required.")
    if output.exists():
        raise SystemExit("The .p12 already exists. Preserve it before creating another file.")
    with tempfile.TemporaryDirectory(prefix="andam-signing-") as folder:
        pem = Path(folder) / "certificate.pem"
        format_name = "PEM" if certificate.read_bytes().startswith(b"-----BEGIN") else "DER"
        pem.write_text(openssl("x509", "-inform", format_name, "-in", certificate))
        subject = openssl("x509", "-in", pem, "-noout", "-subject", "-nameopt", "RFC2253")
        if "Apple Development:" not in subject and "iPhone Developer:" not in subject:
            raise SystemExit("Use an Apple Development certificate for the requested developer IPA.")
        openssl("x509", "-in", pem, "-noout", "-checkend", "0")
        certificate_public = openssl("x509", "-in", pem, "-pubkey", "-noout")
        key_public = openssl("pkey", "-in", key, "-pubout")
        if certificate_public.strip() != key_public.strip():
            raise SystemExit("This certificate does not match the key used to make the prepared CSR.")
        password = getpass.getpass("Choose a password for the .p12: ")
        if len(password) < 8 or "\n" in password or "\r" in password:
            raise SystemExit("Use at least eight characters, without line breaks.")
        if password != getpass.getpass("Confirm the password: "):
            raise SystemExit("The passwords did not match.")
        output.touch(mode=0o600)
        openssl("pkcs12", "-export", "-inkey", key, "-in", pem,
                "-name", "ANDAM Apple Development", "-out", output,
                "-passout", "stdin", data=password + "\n")
        output.chmod(0o600)
        team = re.search(r"(?:^|,)OU=([A-Z0-9]{10})(?:,|$)", subject.strip())
        print("Created:", output)
        if team:
            print("Apple Developer Team ID:", team.group(1))
        print("Enter its password directly in the cloud signing credential form.")


if __name__ == "__main__":
    main()

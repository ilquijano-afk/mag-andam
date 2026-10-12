# Install the updated iPhone game

Use `Builds/iOS/ReadySetResilient-0.6.1-Sideloadly.ipa` — version 0.6.1, build 2. The `Sideloadly-Compatible` and `Sideloadly-Unsigned` filenames also contain this release build.

This build was installed successfully over the existing game and its launch was verified. Automated Unity checks passed; physical-device gesture and appearance checks remain manual.

1. Connect IQ17 by USB, unlock it and open Sideloadly.
2. Load the IPA above and select **IQ17**, using the same Apple account as the installed game.
3. Click **Start** and wait for **Done. 100%**. Install over the existing app to keep its scores.
4. Launch **Ready Set Resilient** on the phone. Jump over the regular ground obstacles, change lanes for the long flood, and duck beneath the hanging cable.
5. Check the complete startup image, menu buttons and leaderboard. The player is built in release mode, without Unity's development console.

Xcode signing remains unconfigured. Sideloadly signs the IPA during installation. This package preserves executable permissions, stores plist entries without compression and excludes old signature resources and AppleDouble files to avoid the earlier UnityRuntime.framework verification error.

To package a future unsigned native build, run the helper from the Unity project folder and pass the version/build that Xcode actually compiled:

```bash
python3 Tools/package-sideloadly-ipa.py \
  Builds/iOS/LocalDerivedData/Build/Products/ReleaseForRunning-iphoneos/ReadySetResilient.app \
  Builds/iOS/ReadySetResilient-0.6.1-Sideloadly.ipa \
  --expected-version 0.6.1 --expected-build 2
```

The helper verifies the package and writes a JSON report beside the IPA. Unity gameplay and layout checks are recorded in `Docs/Jump-Validation.txt`; native compilation is recorded in `Docs/Xcode-release-compile.log`.

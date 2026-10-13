# Install the updated iPhone game

Use `Builds/iOS/ReadySetResilient-0.6.3-Sideloadly.ipa` — version 0.6.3, build 4. The `Sideloadly-Compatible` and `Sideloadly-Unsigned` filenames also contain this release build. This version adds floating jump rewards and 10-second magnet, 2×, 4× and 8× bonuses after 500 meters. The street checklist and item collection popups remain removed.

Automated Unity checks and native compilation passed. Device installation and launch results are recorded locally in `Builds/iOS/Sideloadly-install-status.txt`; physical-device gesture and appearance checks remain manual.

1. Connect IQ17 by USB, unlock it and open Sideloadly.
2. Load the IPA above and select **IQ17**, using the same Apple account as the installed game.
3. Click **Start** and wait for **Done. 100%**. Install over the existing app to keep its scores.
4. Launch **Ready Set Resilient** on the phone. Jump over the regular ground obstacles, change lanes for the long flood, and duck beneath the hanging cable.
   Collect the raised supply trails while jumping. After 500 meters, collect a bonus and check its 10-second indicator. Verify that magnets pull useful supplies and multipliers increase their points without multiplying penalties.
5. Check the complete startup image, menu buttons and leaderboard. The player is built in release mode, without Unity's development console.

Xcode signing remains unconfigured. Sideloadly signs the IPA during installation. This package preserves executable permissions, stores plist entries without compression and excludes old signature resources and AppleDouble files to avoid the earlier UnityRuntime.framework verification error.

To package a future unsigned native build, run the helper from the Unity project folder and pass the version/build that Xcode actually compiled:

```bash
python3 Tools/package-sideloadly-ipa.py \
  Builds/iOS/LocalDerivedData/Build/Products/ReleaseForRunning-iphoneos/ReadySetResilient.app \
  Builds/iOS/ReadySetResilient-0.6.3-Sideloadly.ipa \
  --expected-version 0.6.3 --expected-build 4
```

The helper verifies the package and writes a JSON report beside the IPA. Unity gameplay and artwork checks are recorded in `Docs/Power-up-Validation.txt`, `Docs/Jump-Validation.txt` and `Docs/Power-up-final-preview.log`; native compilation is recorded in `Docs/Xcode-v0.6.3-compile.log`.

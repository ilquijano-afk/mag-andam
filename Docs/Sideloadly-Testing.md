# Install the updated iPhone game

Use `Builds/iOS/ReadySetResilient-0.7.0-Sideloadly.ipa` — version 0.7.0, build 6. The `Sideloadly-Compatible` and `Sideloadly-Unsigned` filenames also contain this release build. This version adds the low-poly coastal road, three-lane barangay alley, uphill flood escape and evacuation refuge, with gradual weather, a stable chase camera and a home-preparedness lesson. The movement, obstacle recovery, single raised rewards and rare field bonuses from 0.6.4 remain.

Automated Unity checks and native compilation passed. Version 0.7.0 has not yet been installed on IQ17; the last verified installation is 0.6.4, build 5. Device installation and launch results are recorded locally in `Builds/iOS/Sideloadly-install-status.txt`; physical-device gesture and appearance checks remain manual.

1. Connect IQ17 by USB, unlock it and open Sideloadly.
2. Load the IPA above and select **IQ17**, using the same Apple account as the installed game.
3. Click **Start** and wait for **Done. 100%**. Install over the existing app to keep its scores.
4. Launch **Ready Set Resilient** on the phone. Jump over the regular ground obstacles, change lanes for the long flood, and duck beneath the hanging cable.
   Collect the single raised supply with a well-timed jump. Check that Opong recovers from a duck before jumping the next obstacle. After 500 meters, collect a bonus and check its 10-second indicator; bonuses should be much farther apart. Verify that magnets pull useful supplies and multipliers increase their points without multiplying penalties.
5. Follow the sunny coast into the three-lane alley and up the dry ramp toward the refuge. Check that road edges, hazards and supplies stay clear, weather changes gradually and the camera remains steady. Check the home lesson on the score page, the complete startup image, menu buttons and leaderboard. The player is built in release mode, without Unity's development console.

Xcode signing remains unconfigured. Sideloadly signs the IPA during installation. This package preserves executable permissions, stores plist entries without compression and excludes old signature resources and AppleDouble files to avoid the earlier UnityRuntime.framework verification error.

To package a future unsigned native build, run the helper from the Unity project folder and pass the version/build that Xcode actually compiled:

```bash
python3 Tools/package-sideloadly-ipa.py \
  Builds/iOS/LocalDerivedData/Build/Products/ReleaseForRunning-iphoneos/ReadySetResilient.app \
  Builds/iOS/ReadySetResilient-0.7.0-Sideloadly.ipa \
  --expected-version 0.7.0 --expected-build 6
```

The helper verifies the package and writes a JSON report beside the IPA. Unity checks are recorded in `Docs/Run-Cadence-Validation.txt`, `Docs/Power-up-Validation.txt`, `Docs/Jump-Validation.txt`, `Docs/Environment-Validation.txt` and `Docs/Environment-full-smoke.log`; native compilation is recorded in `Docs/Xcode-v0.7.0-compile.log`. See `Docs/Environment-Art-Guide.md` for editing and asset setup.

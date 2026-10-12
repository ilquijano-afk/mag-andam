# Build the iOS developer IPA in the cloud

The current game successfully exported its native iOS project with Unity 6000.6.5f1. This export includes the latest artwork, nine useful supplies, five distractors and the centered results score. Apple signing and the remote native compilation are still pending.

This cloud workflow requires an enrolled Apple Developer Program team. For free testing with a regular Apple account, use [the local iPhone testing guide](iPhone-Free-Testing.md).

## Unity Build Automation

Cloud project: **ReadySetResilient-an**, project ID `c1e3aafc-56b3-4ba5-9ec0-dbde9b9150d2`.

Repository: `https://github.com/ilquijano-afk/mag-andam`, branch `main`. Put the Unity project at the repository root. The source snapshot contains Assets, Packages and ProjectSettings; cache/build folders and signing credentials are excluded.

The game source is uploaded and the cloud repository connection is saved. Unity uses a read-only deploy key for this repository. The iOS target is prepared in the browser, but cannot be saved until the signing files are supplied. Automatic builds and recurring schedules are disabled.

| Build setting | Value |
| --- | --- |
| Platform | iOS |
| Unity | 6000.6.5f1 |
| Project subfolder | Empty / repository root |
| Scene | Assets/Scenes/MainGame.unity |
| Development Build | Disabled (release player; no developer console) |
| Backend | IL2CPP |
| Graphics | Metal |
| Minimum iOS | 15.0 |
| Bundle identifier | org.andamlapulapu.readysetresilient |
| Builder | macOS Tahoe, Apple Silicon, Mac Standard (free tier eligible) |
| Xcode | 26.5.0, offered by the cloud builder |
| Pre-export method | AndamRunnerEditor.IOSBuildTools.Configure |
| Signing | Apple Development .p12 and matching iOS App Development .mobileprovision |

Unity's [iOS signing guide](https://docs.unity.com/en-us/build-automation/sign-build-artifacts/sign-an-ios-application) explains the development certificate, device registration, App ID and provisioning profile. Its [advanced build settings](https://docs.unity.com/en-us/build-automation/advanced-build-configuration/overview) explain the pre-export hook. The hosted Mac supplies Xcode; no local Xcode update is needed to run that hosted build.

## Next: obtain the Apple signing certificate

A verified certificate signing request is saved outside the game project at `../iOS-Signing/ANDAM-Apple-Development.certSigningRequest`.

1. Sign in to the [Apple Developer certificate page](https://developer.apple.com/account/resources/certificates/list).
2. Create an **Apple Development** certificate and upload the `.certSigningRequest` file.
3. Download the issued `.cer` file and save it in `../iOS-Signing`.
4. Run `python3 Tools/create-ios-p12.py /absolute/path/to/the/downloaded.cer` from the game project folder. Choose a password when prompted. The tool validates that Apple's certificate matches the local private key and writes the password-protected `.p12` into `../iOS-Signing`.
5. In the Developer website, register App ID `org.andamlapulapu.readysetresilient` and the test iPhone/iPad UDIDs. Create an **iOS App Development** profile for that App ID, certificate and those devices; download its `.mobileprovision`.
6. Add the `.p12`, its password and the `.mobileprovision` to the cloud build's signing credentials. Enter the password directly into the credential form. Then run the iOS development build and download its signed IPA after the build succeeds.

If the target draft has to be recreated, name it **ANDAM iOS Development**, choose the settings above, and name the new credentials set **ANDAM iOS Development**. Use **Save configuration** before starting a manual build. The cloud UI will not save an iOS target without both signing files.

The private `.key` stays in the separate `iOS-Signing` folder, with owner-only file permissions. It is needed to make the `.p12`; it is not included in either source ZIP or the Git repository. The `.p12` is a signing credential and belongs in the cloud credential form, separate from source files.

## Prepared files

- `Builds/iOS/ReadySetResilient-iOS-Cloud-Source.zip`: verified Unity source snapshot, approximately 31 MB.
- `Builds/iOS/ReadySetResilient-iOS-Xcode-Export.zip`: complete native project for a hosted Mac with Xcode, approximately 260 MB.
- `Builds/iOS/Cloud-build-settings.json`: build settings and remaining requirements.
- `Builds/iOS/Unity-export-status.txt`: successful Unity export status.
- `Docs/Editor-ios-export.log`: actual Unity build/export log.

Both ZIPs passed integrity checks. Neither is an IPA. No iPhone installation or physical-device test has been performed yet. The signed IPA will only run on devices included in its development provisioning profile; enable Developer Mode when required by iOS.

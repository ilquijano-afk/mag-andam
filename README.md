# mag-andam
Repo for the mag-andam game


## Unity game and iOS cloud build

The Unity project lives at this repository's root. Open it using Unity **6000.6.5f1** and play `Assets/Scenes/MainGame.unity`. It includes the current Opong Andam runner, coastal evacuation artwork, nine useful supplies, five distractors, menus and local leaderboard.

For Unity Build Automation, select **iOS**, **Development Build**, Unity **6000.6.5f1**, Xcode **16 or later**, and the bundle ID `org.andamlapulapu.readysetresilient`. Leave the project subfolder blank. Use pre-export method `AndamRunnerEditor.IOSBuildTools.Configure` and add Apple Development signing credentials separately.

See [the cloud build guide](Docs/iOS-Cloud-Build.md) and [the configuration](iOS-cloud-build-settings.json). Unity's native project export has succeeded; the signed IPA requires the Apple certificate and provisioning profile and a successful cloud build.

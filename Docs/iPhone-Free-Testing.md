# Test ANDAM on your own iPhone for free

Use Xcode's **Personal Team** with a regular Apple account. This installs the game directly on your own device. Apple's free provisioning profiles expire after seven days; build and install again to renew the test app. A cloud-signed IPA and TestFlight require an enrolled Apple Developer Program team.

## Prepared game project

Open `Builds/iOS/Xcode/Unity-iPhone.xcodeproj` in a compatible Xcode version. The game successfully exported with Unity 6000.6.5f1. Its bundle ID is `org.andamlapulapu.readysetresilient`; it uses IL2CPP, Metal, portrait orientation and minimum iOS 15.0.

The local native project is prepared for automatic signing. No Personal Team has been selected yet. A backup of the original native signing settings is at `Builds/iOS/Unity-iPhone-before-local-signing.pbxproj`. The earlier Xcode export ZIP preserves the original export.

This Mac has macOS 26.2. Xcode 26.6 supports it; the App Store's Xcode 27 requires macOS 26.6 or later. Apple's **Xcode 26.6 Apple silicon.xip** is the compatible download selected for this Mac.

## Install and run

1. Open the compatible Xcode and complete its first-launch setup. Review any license agreement before accepting it. Install the iOS platform support if requested.
2. In **Xcode → Settings → Accounts** (called **Apple Accounts** in some versions), add your Apple account. Enter the password and verification code directly in Xcode.
3. Connect the iPhone by USB, unlock it, and tap **Trust** if prompted.
4. Open the `.xcodeproj` above. Select the blue **Unity-iPhone** project in the left navigator, then the **Unity-iPhone** target. In **Signing & Capabilities**, keep **Automatically manage signing** enabled and select your **Personal Team**. If Xcode requests a team for **UnityFramework**, select the same team there.
5. Select the **Unity-iPhone** scheme and your actual iPhone in Xcode's destination selector. Click the triangular **Run** button, or press **⌘R**.
6. If the iPhone requests Developer Mode, enable it under **Settings → Privacy & Security → Developer Mode** and follow the phone's restart/confirmation steps. If the phone reports an untrusted developer, review and trust your own developer entry under **Settings → General → VPN & Device Management**.

Leave the phone connected while Xcode builds, installs and starts the game. The first native build can take several minutes. If the bundle identifier is already in use, choose a unique identifier under Signing & Capabilities and rebuild.

## Device check

Once installed, check the splash screen and menu, swipe through all three lanes, jump and duck, collect a supply and a distractor, collide with an obstacle, and check the centered score and local leaderboard. Device installation and gameplay verification are still pending.

Apple's [developer account guide](https://developer.apple.com/help/account/basics/about-your-developer-account) documents Personal Team testing and its limits. The [Xcode compatibility table](https://developer.apple.com/xcode/system-requirements) lists supported macOS versions. Compatible installers are available through [Apple's downloads page](https://developer.apple.com/download/all/?q=Xcode%2026.6).

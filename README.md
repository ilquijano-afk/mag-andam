# Ready, Set, Resilient!

**Resilience Starts at Home** · ANDAM Lapu-Lapu · IDDRR prototype

Opong rides his white shield along a fictional coastal road toward an evacuation center in Lapu-Lapu City, Cebu. Version 0.6 adds a stormy 3D-style illustration, rain, wind-bent palms, a choppy sea behind the seawall and a warmly lit shelter ahead. The rounded supplies, hazards and chibi character follow the supplied runner sheet. Nine useful supplies earn points; five distractors deduct 5 points without costing a life. Speed rises at every 250-meter milestone, and bumps reset it to starting speed. A full-width hanging cable requires a duck. Version 0.6.2 removes the street checklist and item collection popups; scores, lives, distance, speed, controls and obstacle warnings remain visible.

Developed by: **UP Cebu Center for Environmental Informatics**, in partnership with the **City of Lapu-Lapu Disaster Risk Reduction and Management Department**.

## Play in Unity — shortest path

1. In **Unity Hub → Projects → Add → Add project from disk**, choose this **ReadySetResilient** folder. Select **Unity 6000.6.5f1** when prompted. Do not create a new template project.
2. Wait for import and compilation. The **ANDAM** menu appears along the top of Unity.
3. Choose **ANDAM → 1. Prepare or open MainGame**. It opens `Assets/Scenes/MainGame.unity` and sets up the build settings.
4. Open the **Game** tab. In its resolution dropdown, choose **+**, select **Fixed Resolution**, and add **720 × 1280** (portrait). Use **Scale: Fit** so the whole screen is visible.
5. Press Unity's **Play** triangle. The illustrated splash appears for five seconds; tap anywhere to skip it. In the illustrated main menu, enter a rider nickname at the bottom and click **MAG-ANDAM NA**.
6. Use **Left/Right arrows** or **A/D** to change lanes. Press **Space**, **Up**, or **W** to jump; **Down**, **S**, or **Left Ctrl** to duck. The four on-screen buttons also work. **P**, **Escape**, or the **II** button pauses. On a phone, swipe left/right, up to jump, or down to duck.

The road, Opong, items, scenery, camera behavior, and interface are built when Play starts. A mostly empty Scene view before Play is expected. You do not need to create prefabs, assign scripts, or drag object references.

## See the artwork

Gameplay uses the storm evacuation illustration. The splash, main menu, credits, instructions, About, leaderboard and results now use the seven supplied PNG compositions in `Assets/AndamRunner/Resources`; see `Docs/Menu-PNG-Artwork.md` for editing and tap-area details. The results page uses `ScoreBackground.png` with a live score and run summary in its yellow panel, and invisible tap areas over Play Again, About and Credits. Main Menu and Scores links remain available below the painted buttons. See `Docs/Previews/01-Splash.png`, `02-MainMenu.png`, `06-About.png`, `07-Credits.png` and `11-Leaderboard-SampleRuns.png` for the current menus, plus `09-Gameplay.png` and `18-Gameplay-Duck.png` for the doubled starting speed. `21-Gameplay-Speed250m.png` and `22-Gameplay-SpeedReset.png` show the boosted and reset HUD. `23-Gameplay-NewItems.png` shows cash, food and a bulky toy on the route. `10-Results.png` shows the supplied score artwork. `Item-*.png`, `Distractor-*.png` and `Hazard-*.png` show all nineteen track objects. Character views, poses and `Face-*.png` show the playable 3D model. These are actual Unity renders. The sunny and storm gameplay backdrop illustrations were made with the built-in imagegen tool; the supplied menu PNGs are used intact. The storm asset is `Assets/AndamRunner/Resources/StormEvacuationCoast.png`; its exact prompt is in `Docs/StormEvacuationCoast-Prompt.txt`.

To inspect the 3D model in Unity, double-click **Assets/AndamRunner/Resources/OpongStorybook.prefab** in the Project window. Select it in Scene view and press **F** to frame it. Gameplay loads this prefab. The mesh, 1024-pixel palette atlas, material and animation controller are in **Assets/AndamRunner/Models/OpongAndam**. In the Animator window, select its controller to inspect Idle, Run Loop (shield balancing), Jump Start, Air, Land, Slide Duck, Hit and Victory. Slide Duck now drives the duck action. Facial joints support seven moods and blinking.

**ANDAM → 5. Rebuild Opong model prefab** regenerates the model, texture and animation assets from `OpongModel.cs` and `OpongClipBuilder.cs`, replacing direct edits. The version 0.4 model and generator source are preserved in **Docs/Backups/Art-v0.4**. Ordinary **Prepare** and **Render screen previews** commands preserve the saved prefab.

To change item or obstacle artwork, edit **Assets/AndamRunner/Scripts/RunnerReferenceArt.cs**: `Supply` creates useful collectibles; `Distractor` creates the five unnecessary items; `Hazard` creates obstacles. See `Docs/Score-And-New-Items.md` for the new designs and scoring. These objects are built when Play starts, so stop and restart Play to see your changes. `PrimitiveArt.cs` forwards object creation to that file. Painted menu lettering is edited in your original artwork document, then exported over its corresponding PNG. Native wording, layout and tap areas are in `MenuPages.cs`; native font assignment is in `UIManager.cs`; launch splash credits and game information are in `Resources/GameInformation.json`.

## What is playable

- Endless forward motion with three lanes, jumps, timed ducks and 250-meter speed steps.
- Nine recognizable supplies and points; distinct supply types are tracked for results and leaderboard ranking.
- Gym equipment, a bulky toy, beer, makeup and a game console as orange-marked distractors. Each deducts up to 5 points, with no life loss or speed reset.
- Three lives; every accepted hazard collision deducts a life and up to 10 points. Score never goes below zero.
- A short grace period after damage; each object can trigger only once.
- Credits splash, main menu, instructions, game information, full credits, leaderboard, HUD, pause, results, and restart screens; touch swipes and large touch buttons.
- A persistent top-ten leaderboard on this device. Finished runs save automatically, with score, distinct supply types, distance, and nickname. Ranking uses score, then supply types, then distance; older runs win an exact tie.
- Obstacle warnings and collision feedback; a home action to do with an adult after each run. Collecting items updates the score without an information popup.
- Road chunks and item/hazard pools recycle; the player stays near the origin instead of accumulating floating-point error.
- Phone safe-area support; offline play with bundled artwork; no account, ads, purchases, analytics, or runtime art downloads.

This prototype uses original Unity meshes and a generated transform rig with saved animation clips. No sound is included. Shield riding and jumping over short water/wire obstacles are fictional game play. In real life, stay away from floodwater and fallen electrical wires and use a safe route with an adult. Long flood streaks require changing lanes; the low blue cable with down-arrow signs requires ducking.

## New menu and credits

**HOW TO PLAY** uses the supplied full-page instructions image, replacing the previous tabs. **ABOUT THE GAME** uses `AboutBackground.png` for the overview, purpose and guidance. **CREDITS** uses the supplied organizations, logos and mascot attribution image. The startup splash uses `SplashscreenBackground.png`, automatically continues after five seconds and supports tap-anywhere skipping. Invisible tap areas cover the painted buttons; their extra native graphics and labels are disabled. The leaderboard draws live ranks, rider names, scores, supplies and distance over its blank table area. All information pages have a Back button; Android Back / Escape also works.

To edit fallback credit names or the game description, change `Assets/AndamRunner/Resources/GameInformation.json`. These fields control the original native splash, About and credits screens when their PNGs are absent. Lettering and logos embedded in the supplied PNGs must be edited in their original design documents. Run **ANDAM → Render supplied menu artwork previews** to review all seven PNG pages and current gameplay at three phone sizes.

The leaderboard is **local to each device**, with no server or account required. Scores survive app restarts and APK updates that retain the same package ID. Clearing app data or uninstalling usually removes them. Scores are stored in `andam-leaderboard-v1.json` under Unity's `Application.persistentDataPath`; a backup supports recovery from a damaged save. Automated checks and screen renders use isolated storage and do not add sample scores to the real leaderboard.

## Scoring and safe routes

| Supply | Points |
|---|---:|
| Emergency go-bag | 20 |
| Drinking water | 10 |
| First-aid kit | 15 |
| Flashlight | 10 |
| Whistle | 10 |
| Battery-powered radio | 15 |
| Family contact card | 25 |
| Emergency cash | 10 |
| Non-perishable food | 15 |

**Jump** over ordinary flood patches, downed-wire obstacles, debris and road barriers, or change lanes to avoid them. A jump lasts 0.92 seconds and rises 2.3 units. Start before the obstacle: the shield must remain above its artwork for the whole crossing. **Change lanes for long floods:** these occupy one lane for 32 meters, farther than a jump at maximum speed. Their neighboring rows retain a dry route and no collectibles are hidden inside the flooded lane. Striped barriers, lightning pictograms, water and rubble silhouettes identify hazards; supplies float above bright mint discs with dark rims and small stars. Orange discs identify distractors. The first go-bag is in the center lane.

**Duck** beneath the low blue hanging cable with three downward-arrow signs. It spans all three lanes, so changing lanes cannot bypass it. Duck lasts 1.1 seconds; it cannot start in the air or be prolonged by repeated input. Neighboring rows contain supplies without ground hazards, providing time to recover.

The current user-edited settings start at **24 m/s**, add **2 m/s** every **250 meters**, and set a ceiling of **46 m/s**. The existing six-boost limit currently makes the highest reachable speed **36 m/s**. Edit these values in `Assets/AndamRunner/Scripts/RunPace.cs`; its `Level` clamp controls the boost limit. Any obstacle contact resets speed to the starting value while retaining run distance. Speed increases again at the next global milestone.

Mint markers and stars identify useful supplies. Orange markers identify distractors: each deducts up to 5 points, never reduces lives or speed, and does not fill the checklist. A bulky toy is the game example; a small comfort item may belong in a family kit after essentials.

The checklist measures supplies found in this game; it is not a real emergency-readiness assessment. Repeated supplies still award points, but each type fills the checklist once.

## Build the Android APK

Unity's Android module was absent during the initial environment inspection. Android Studio is not required for this Unity workflow.

1. Close the project before adding modules. Open **Unity Hub → Installs**. Find **6000.6.5f1**, click its **Manage / gear / three-dot menu → Add modules**.
2. Check **Android Build Support**, expand it, and check **Android SDK & NDK Tools** and **OpenJDK**. Complete installation. Use Unity's supplied tools; an Android Studio SDK alone does not provide the Unity Android player module.
3. Reopen this project. Under **Unity → Settings → External Tools** on macOS (or **Edit → Preferences** on Windows), leave the Unity-installed Android JDK, SDK, NDK, and Gradle selected.
4. Choose **ANDAM → 3. Build Android APK**. Wait for platform preparation and the build. The output is **Builds/Android/ReadySetResilient.apk**.
5. For a phone connected by USB, enable its **Developer options → USB debugging**, accept its trust prompt, and use the install command below. Alternatively, copy the APK to the phone, open it, and allow installation from that file app when Android asks.

The build menu sets **portrait**, **Android 8 / API 26 minimum**, **ARM64**, **IL2CPP**, **OpenGL ES 3**, the package ID `org.andamlapulapu.readysetresilient`, and a development APK. This is for direct prototype testing. Store submission, release signing, and Play Store packaging are later tasks.

For Unity's standard UI, **File → Build Profiles → Android → Switch Platform** gives access to **Build** or **Build And Run**. Ensure `Assets/Scenes/MainGame.unity` is enabled in the scene list, and **Build App Bundle** is off for an APK.

On this Mac, from the project folder:

```bash
/Users/Belle_1/Library/Android/sdk/platform-tools/adb devices
/Users/Belle_1/Library/Android/sdk/platform-tools/adb install -r Builds/Android/ReadySetResilient.apk
/Users/Belle_1/Library/Android/sdk/platform-tools/adb shell am start -n org.andamlapulapu.readysetresilient/com.unity3d.player.UnityPlayerActivity
```

If the last command reports the activity does not exist, launch the installed game from the phone's app drawer instead; the launch activity can vary with Unity's Android application entry-point setting.

## Test on the phone before the event

1. Start a run. Swipe left/right in the open game area and up to jump. Test the touch buttons too.
2. Collect a center-lane go-bag: score becomes 20. Collect cash (+10) and food (+15). Collect an orange distractor: lose up to 5 points, with lives and speed unchanged. These pickups should not show information popups, and there should be no go-bag checklist panel in the street.
3. Jump over each regular ground obstacle, including the short flood and downed wires. Change lanes for the long flood streak. A mistimed jump or collision costs one life, up to 10 points, and resets speed. Swipe down or tap DUCK for the low blue cable across all lanes; check that one cable cannot deduct three lives.
4. Lose all three lives. Check the supplied score page shows the live score, distance and supplies inside its yellow panel. Test Play Again, About, Credits, Main Menu and Scores.
5. Tap PLAY AGAIN. Check zero score, three lives, center lane, and a new run.
6. Pause/resume, then background and reopen the app. It should pause safely. Check that text and controls clear the phone's notch and navigation area.
7. Play for five minutes. Check continuous spawning, recognizable symbols, the stormy shelter route and learning feedback. Confirm the speed indicator steps at 250-meter milestones and resets after a bump. Test without internet and with sound muted.
8. Open the leaderboard from Results and return to Results. Return to the menu and explore instructions, About, and credits. Check that the invisible tap areas match the painted buttons on your phone.
9. Close and relaunch the app. Confirm the finished score and nickname remain in the leaderboard. Repeat with a different nickname to check ranking.

Automated desktop checks cannot confirm real-device touch behavior or Android performance. See `Docs/Jump-Validation.txt` for the jump, flood and safe-area checks. The iOS export uses a release player, with Unity's development console, script debugging and profiler disabled. Menu images and their tap areas share a fitted safe-area frame; a dimmed illustration fills spare space without cropping the main composition.

## Beginner's code map

| File in Assets/AndamRunner/Scripts | Purpose |
|---|---|
| GameManager.cs | Builds the world, changes game state, applies points/lives, runs the simulation |
| RunSession.cs | Small plain C# score, lives, and supply-checklist model |
| RunPace.cs | Distance, 250-meter speed milestones, speed cap and collision reset |
| PlayerRunner.cs | Lanes, jump arc, timed duck, shield tilt, damage blink |
| SwipeInput.cs | Keyboard input and one action per touch swipe |
| TrackEntity.cs | Supply/distractor/hazard type and swept collision checks |
| SpawnManager.cs | Reuses items and generates rows with a safe lane |
| RepeatingRoad.cs | Recycles the three moving lanes over the illustration |
| CoastalIllustrationBackground.cs | Fits the painted coastal backdrop to the gameplay camera |
| CameraFollow.cs | Gentle camera follow |
| UIManager.cs | Start, HUD, pause, results, feedback, and safe-area layout |
| MenuPages.cs | Splash, main menu, instruction tabs, About, credits and leaderboard pages |
| LeaderboardStore.cs | Saves and ranks the top ten runs; loads a backup if needed |
| GameInformation.cs | Loads editable credits and description from the JSON resource |
| UISkin.cs | Rounded UI panels and menu atmosphere |
| CoastalPolish.cs | Curved palm fronds, jeepneys, lamps, bunting, flowers and benches |
| OpongAnimation.cs | Chooses animation clips, pickup/hit reactions and shield streaks |
| OpongExpressions.cs | Seven facial moods and blinking through face joints |
| PreparednessContent.cs | All item values, learning messages, and home actions |
| PrimitiveArt.cs | Basic mesh/material helpers, scenery, shield and item creation entry points |
| RunnerReferenceArt.cs | Sprite-sheet-guided 3D supplies, distractors and hazards, with no floating labels |
| OpongModel.cs | Chibi mascot geometry, costume, face and animation joints |
| StorybookArt.cs | Smooth mesh sculpting helpers and geometry grouping |
| StorybookPortrait.cs | Menu-only transparent camera view of the playable character |
| StorybookScenery.cs | Optional procedural clouds retained for alternate scenery |

`Assets/AndamRunner/Editor/PrototypeTools.cs` creates the scene, configures the project, and exposes build menus. `SmokeChecks.cs` checks gameplay, menu navigation and leaderboard storage inside Play mode. `PreviewRenderer.cs` adds **ANDAM → 4. Render screen previews** and saves actual Unity renders under `Docs/Previews`. Start with text and scores in `PreparednessContent.cs`; use the menus to rebuild or test after changes. The scene-creation menu preserves an existing MainGame scene.

## Common errors

| Symptom | Fix |
|---|---|
| No ANDAM menu | Wait for compilation. Open **Window → General → Console** and resolve red errors. Open in 6000.6.5f1. |
| Input-system exception | **Edit → Project Settings → Player → Other Settings → Configuration → Active Input Handling: Input Manager (Old)**. Restart when asked. This project uses Unity's legacy input with no Input System package. |
| Empty Scene or Game view | Open MainGame via the ANDAM menu, then press Play. The world is built at runtime. |
| UI cut off in Editor | Set the Game view to 720 × 1280 portrait and Scale: Fit. |
| Pink models or missing coastal background | Run ANDAM → Prepare. This is a **Built-in Render Pipeline** project. The bundled Opong and illustrated-background materials keep their custom shaders in builds. |
| Android option unavailable | Add all three Android modules for the same Unity version in Unity Hub. |
| SDK/NDK/JDK or Gradle error | Restore Unity-installed tools in External Tools. Let Unity download the required Android platform if prompted. First builds may need internet. |
| APK won't install | Use an ARM64 Android 8+ phone. If an older copy was signed with a different key, uninstall it first, then install this development APK. |
| Phone listed as unauthorized | Unlock it and accept the USB debugging prompt. Reconnect the cable if needed. |
| Unity says project is already open | Close the other editor or batch build using this same project; don't delete lock files while Unity is running. |

## Educational sources

The game's messages are short paraphrases for family learning. Its setting is fictional and does not represent real evacuation routes.

- [DOST–PHIVOLCS earthquake preparedness](https://www.phivolcs.dost.gov.ph/earthquake-preparedness/): earthquake preparation and protective actions.
- [PHIVOLCS Duck, Cover, and Hold drill](https://www.phivolcs.dost.gov.ph/phivolcs-employees-perform-the-duck-cover-and-hold/): Philippine earthquake-drill wording. The game says Drop, Cover, and Hold On and does not teach running outdoors during shaking.
- [CDC floodwater safety](https://www.cdc.gov/floods/safety/floodwater-after-a-disaster-or-emergency-safety.html): floodwater dangers and never touching fallen power lines.
- [American Red Cross emergency-kit guidance](https://www.redcross.org/get-help/how-to-prepare-for-emergencies/survival-kit-supplies.html): non-perishable food, extra cash and suitable children’s activities.
- [FEMA Ready Responder toolkit](https://www.ready.gov/sites/default/files/documents/files/RRToolkit.pdf): basic supply-kit items and family preparation.
- [Unity 6.6 Android environment setup](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/getting-started/sdksetup): required Android modules.

Use current instructions from your barangay, Lapu-Lapu City disaster office, PAGASA, PHIVOLCS, and other appropriate authorities during a real emergency.

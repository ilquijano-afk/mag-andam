# Gameplay and screen update — 0.6.1 (build 2)

Regular flood patches, downed wires, debris and barriers can be jumped. Hanging cables across all three lanes still require a duck. Jump clearance uses each ground obstacle's height and its full footprint, including during lane changes and slow frames.

New long flood streaks cover 32 meters in one lane. They exceed the longest possible jump and require a lane change. The spawner reserves the flooded lane across neighboring rows, keeps useful supplies on dry routes and retains the water until its trailing edge passes. Warnings say to change lanes for long floods.

Collectible artwork uses saturated blue, cyan, coral, yellow, violet, green and pink. Useful supplies have bright mint markers and stars; distractions have orange markers. Dark marker rims provide contrast against the storm street. There are no floating item labels.

The startup, menu, credits, instructions, About, leaderboard and results PNGs use one centered frame fitted to the final phone safe area. The entire foreground illustration remains visible, with dimmed illustrated borders filling spare space. Invisible painted-button targets stay attached to that frame. All seven images are checked on short and tall phones, notched safe areas and tablets.

The iOS player is built without Development Build, script debugging or profiler connection. Signing stays unconfigured in Xcode; Sideloadly signs the compatible IPA with the user's existing account. This update keeps the same base bundle identifier so installing over the existing game preserves its local scores.

For Unity checks, stop Play mode and choose **ANDAM → Check jumps, floods and screen fitting**. The full **Run gameplay smoke checks** command includes the same checks. Results are written to `Docs/Jump-Validation.txt`; real leaderboard data is not modified.

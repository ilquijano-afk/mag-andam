# Street HUD cleanup — version 0.6.2, build 3

Removed the complete go-bag checklist panel from gameplay, including its counter and supply slots. Moved the evacuation-route banner beneath the score strip to expose more of the street.

Collecting useful supplies or orange distractors updates the score and character reaction without an information popup. New runs also start without the introductory popup. Obstacle warnings and collision feedback remain, including the duck cue for hanging cables and lane-change cue for long floods. Supply totals still appear in results and leaderboard records.

Included the user's current speed settings from `RunPace.cs`: base 24 m/s, 2 m/s per 250 meters, configured ceiling 46 m/s. The existing six-boost limit currently reaches 36 m/s. No speed settings were changed by this HUD cleanup.

Unity's existing gameplay checks and rendered page checks passed with these settings. Pickup and jump checks use an isolated track so tutorial objects cannot interfere at faster speeds. The clean street preview is `Docs/Previews/Gameplay-Clean-HUD-v0.6.2.png`.

The iOS player uses release mode and the same base bundle identifier. Xcode signing stays unconfigured. Sideloadly signs `Builds/iOS/ReadySetResilient-0.6.2-Sideloadly.ipa` during installation.

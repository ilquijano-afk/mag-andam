# Jump rewards and power-ups — version 0.6.3, build 4

Regular flood patches, downed wires, debris and road barriers now have three useful supplies floating in an arc above them. A well-timed jump clears the obstacle and collects the trail. Standing or dodging misses the raised supplies. Long flood streaks still require changing lanes and hanging cables still require ducking; neither receives a jump reward trail.

Power-ups start spawning when the current run reaches 500 meters. The first bonus is 2×. Later normal rows have a 20% bonus chance; among these bonuses, the weights are:

| Bonus | Relative frequency | Effect |
|---|---:|---|
| Green 2× badge | 70% | Double useful-supply points |
| Purple 4× badge | 22% | Quadruple useful-supply points |
| Red/blue magnet | 6% | Pull nearby useful supplies toward Opong |
| Gold 8× badge | 2% | Eight times useful-supply points |

Each pickup activates its own 10-second timer. Collecting the same type refreshes that type to 10 seconds. The strongest active multiplier applies; a weaker pickup does not extend a stronger one. A weaker bonus can resume if it still has time when the stronger one expires. Magnet and multiplier effects can run together. Timers pause with the game and clear on a new run.

The magnet attracts useful supplies within 9 world units, including supplies above obstacles or in neighboring lanes. It excludes distractors, obstacles and other power-up tokens. Multipliers apply only to useful-supply points: distractions still subtract up to 5 points without life loss, and obstacle penalties stay unchanged. The active effect and remaining time appear beneath the evacuation-route banner. Item information popups and the street checklist remain removed.

Power-ups use rounded Unity meshes and bright cyan markers. The 2×, 4× and 8× symbols are part of the badge geometry; ordinary supplies and obstacles retain no floating item labels.

## Editing

- `Assets/AndamRunner/Scripts/RunPowerUps.cs`: unlock distance, duration, magnet radius and rarity weights.
- `Assets/AndamRunner/Scripts/SpawnManager.cs`: bonus row chance, raised reward positions and magnet pull speed.
- `Assets/AndamRunner/Scripts/RunnerReferenceArt.cs`: `PowerUp` creates the magnet and multiplier artwork.
- `Assets/AndamRunner/Scripts/UIManager.cs`: `RefreshPowerUps` controls the active bonus indicator.
- `Assets/AndamRunner/Scripts/RunPace.cs`: existing user speed settings, preserved at 24 m/s plus 2 m/s per 250 meters. The configured ceiling is 46 m/s; the six-boost limit currently reaches 36 m/s.

## Validation and previews

Unity smoke checks, 10,661 jump/flood/duck/layout assertions, and 36,005 power-up assertions passed. Checks cover all lanes, base and highest reachable speeds, raised pickup contacts, 500-meter gating, real multiplied collections, timer expiry, pause/restart, magnet exclusions, bounded spawning and rarity. The weighted selector was sampled 100,000 times; natural power-up spawning was exercised over 30,000 meters. All four new designs passed mesh checks and were rendered at three phone sizes.

See `Docs/Power-up-Validation.txt`, `Docs/Jump-Validation.txt`, `Docs/Previews/26-Gameplay-JumpRewards.png`, `Docs/Previews/27-Gameplay-PowerUps.png` and `Docs/Previews/PowerUp-*.png`. These previews deliberately place all four tokens together for review; ordinary gameplay uses the rarity weights above.

The iOS release export and Xcode arm64 compilation passed. `Builds/iOS/ReadySetResilient-0.6.3-Sideloadly.ipa` is version 0.6.3, build 4. Xcode signing remains unconfigured; Sideloadly signs during installation. The package helper verified ZIP integrity, executable permissions, stored plist entries and agreement with the compiled app. Device installation status is recorded separately in `Builds/iOS/Sideloadly-install-status.txt`; phone touch and appearance checks remain manual.

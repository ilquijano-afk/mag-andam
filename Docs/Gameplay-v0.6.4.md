# One jump reward and safer obstacle timing — version 0.6.4, build 5

Each regular flood patch, downed-wire obstacle, debris pile and road barrier now has exactly one useful supply centered above it at height 2.2. A well-timed jump collects it. Long flood streaks require changing lanes, and hanging cables require ducking; neither offers a raised reward.

Useful supplies still spawn in frequent rows, but obstacles now use separate recovery reservations instead of relying on one clear row around a hanging cable. Each obstacle group leaves enough distance for the longest action (1.1 seconds), preparation for the next jump (0.46 seconds), and 0.75 seconds of reaction time. Reservations include obstacle depth and the full trailing edge of a long flood. The highest reachable speed sets the distance, so later speed increases cannot shorten this window. A collision does not remove the reservation. Two ground hazards may share a row in different lanes, while the useful supply keeps a dry route.

With the current user settings, the top speed is 36 m/s and the minimum clear distance between separate obstacle groups is about 83 meters. The actual row alignment can leave more space. Introductory hazards were reduced to one short flood with a single jump reward; normal spawning supplies the other obstacle types. Hanging cables and long floods continue to appear in the paced obstacle sequence.

Power-ups still unlock after 500 meters, with 2× first and subsequent weights of 70% 2×, 22% 4×, 6% magnet and 2% 8×. Instead of a 20% chance in every row, a successful bonus schedules the next one 400–550 meters later. It appears on the next eligible supply row, adding at most roughly one row (13.5 meters). There is at least 400 meters between field bonuses and only one token visible at once. Effects still last 10 seconds; scoring, multiplier precedence and magnet behavior remain as described in the version 0.6.3 notes.

The user's speeds are unchanged: 24 m/s initially, plus 2 m/s per 250 meters, a configured ceiling of 46 m/s and a six-boost limit that currently reaches 36 m/s.

## Editing

- `Assets/AndamRunner/Scripts/SpawnManager.cs`: `ReactionTime`, `ObstacleRecoveryTime`, `MinimumPowerUpGap`, `MaximumPowerUpGap`, and the single reward's height in `SpawnHazard`.
- `Assets/AndamRunner/Scripts/RunPace.cs`: speed values and `MaximumLevel`. `TopSpeed` computes the highest reachable speed for obstacle spacing.
- `Assets/AndamRunner/Scripts/PlayerRunner.cs`: jump and duck durations. Obstacle spacing automatically accounts for these durations.
- `Assets/AndamRunner/Scripts/RunPowerUps.cs`: bonus duration, unlock distance, magnet range and relative rarity.

## Validation

Unity smoke and rendered artwork checks passed. The timing suite passed 29,222 assertions, including 288 actual duck→jump, jump→duck and jump→jump sequences using the shortest naturally generated gaps. These cover all three lanes, all four jumpable obstacles, base/top speed and 15/30/60/120 fps. A late valid duck still finishes before the next jump, with reaction time to spare; every sequence clears both hazards without damage.

The power-up suite passed 89,478 assertions, including a 120,000-meter spawning run that checks at least 400 meters between bonuses and no crowded bonus tokens. The jump/flood/duck/layout suite passed 4,408 assertions. The weighted selector remains checked over 100,000 draws. Tests use isolated leaderboard storage and preserve real scores.

Reports: `Docs/Run-Cadence-Validation.txt`, `Docs/Power-up-Validation.txt` and `Docs/Jump-Validation.txt`. Current gameplay renders: `Docs/Previews/26-Gameplay-JumpRewards.png` and `Docs/Previews/27-Gameplay-PowerUps.png`, with short/tall phone variants. Device installation status is recorded in `Builds/iOS/Sideloadly-install-status.txt`; phone gestures and visual checks remain manual.

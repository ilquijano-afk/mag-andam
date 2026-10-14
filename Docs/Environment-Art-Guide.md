# ANDAM coastal environment and asset pack

The playable slice starts on a sunny Philippine coastal road, enters a compact barangay alley, then climbs a dry ramp above a flood sequence toward an evacuation center. Opong keeps his existing chibi rig, expressive face, red-brown outfit, shield board and movement clips. The alley retains three lanes, as requested. A separate single-lane alley model is provided for future use.

The scene uses a 960-meter loop: coast at 0–240 m, alley at 240–480 m, uphill escape at 480–600 m, dry refuge at 600–720 m, gentle descent at 720–840 m and coastal return at 840–960 m. These are fictional game districts, not real evacuation directions. Storm light fades in across 144 meters and later fades out; the scene stays bright enough to identify supplies and hazards.

## Composition

| Layer | Content | Rule |
|---|---|---|
| Foreground | Charcoal or concrete road, cream lane lines, Opong, hazards, bright supplies, safe-route arrows | Highest readability and contrast; three lane centers remain at X = −2.6, 0, +2.6 |
| Midground | Small concrete houses, a sari-sari storefront, selected palms, streetlights, railings and the refuge | Three or four prominent prop roots per 24-meter segment; retain empty sidewalk space |
| Background | Muted residential silhouettes, simplified towers, ocean, a distant bangka and soft sky | Low contrast, limited detail and slow parallax |

The central corridor is clear of city props. Scenery has no active physics colliders. Full-width hanging wires remain an intentional gameplay obstacle; environmental utility wires stay beside the road. The optional overpass has enough clearance above Opong's jumping silhouette.

## Palette and materials

| Shared material | Colors and purpose |
|---|---|
| `MAT_Environment_VertexPalette` | Asphalt #4A5967; concrete #CBCFC5; cream #EDDEBE; muted teal #5C9999; leaf #6B9469; coral #C68974; terracotta roof #946A58 |
| `MAT_Gameplay_VertexPalette` | Bright cyan/blue, mint, gold, orange distractor markers and clear hazard yellow/charcoal; preserves the existing item designs |
| `MAT_Background_SoftPalette` | Softer, less saturated cream/blue-green skyline; restrained directional shade and fog |
| `MAT_Water_QuietTeal` | Opaque muted teal with slow, small color variation; no reflection camera or heavy transparency |
| `MAT_Rain_SoftStreaks` | One mesh containing 18 pale rain streaks, behind the immediate gameplay objects |
| `MAT_Opong_ContactShadow` | A soft 20-triangle contact shadow; no real-time shadow map |
| Existing Opong material | Existing 1024-pixel palette atlas and one skinned body renderer; the rig and source artwork are preserved |

Environment and gameplay models use vertex colors instead of individual high-resolution textures. The exported models share saved meshes and materials. The supplied menu, credits, splash, instructions, About, leaderboard and score PNGs remain available. The results page adds a native home-preparedness lesson over the old paragraph area, with the live score and painted buttons retained.

## Asset list and complexity

`Docs/Environment-Asset-Catalog.csv` is the per-asset inventory. Each row gives the exact prefab name, intended purpose, layer, suggested dimensions, lane count, prominent prop count, measured triangle count, material and collision setup. Dimensions are in meters, except the three UI entries, which use pixels at the 720×1280 reference layout. The hero is deliberately more detailed than the city props.

The library contains modular roads with sidewalks and drainage, a promenade, three-lane and single-lane alleys, ramps, an elevated walkway, refuge, alternate-route, flooded, typhoon, earthquake-afterward and underpass variants. The prop library includes palms, a tropical tree, a utility pole, houses, a sari-sari store, carinderia, residential block, resort, tower, seawall, fence, bangka, footbridge and evacuation center. Wide stairs and an elevated sidewalk are optional pedestrian models; the shield-riding route uses the gentle ramp.

Optional new obstacle silhouettes include a barricade, parked vehicle, fallen branch, broken pavement, fallen sign, limited debris, puddle, bounded floodwater and blocked passage. These are reusable model assets. They require an appropriate `TrackEntity` collision rule before being added as new gameplay types. The playable slice retains the six already-tested hazard types, nine useful supplies, five distractors and four power-ups.

## Folder structure

```text
Assets/AndamRunner/Environment/
  Materials/                         Shared materials
  Meshes/                            Saved low-poly meshes
Assets/AndamRunner/Resources/Environment/
  EnvironmentAssetPack.asset         Runtime library and material references
  Segments/PF_Segment_*.prefab        24-meter modular road pieces
  Props/PF_Prop_*.prefab              City props and optional obstacle models
  Gameplay/PF_Supply_*.prefab         Useful supplies
  Gameplay/PF_Distractor_*.prefab     Unnecessary items
  Gameplay/PF_Hazard_*.prefab         Existing gameplay hazards
  Gameplay/PF_PowerUp_*.prefab        Magnet and score badges
  Player/PF_Player_OpongShieldboard.prefab
  UI/PF_UI_*.prefab                  HUD strip, route banner and home lesson card
```

## Unity setup

1. Open the existing project in Unity 6000.6.5f1. Choose **ANDAM → 1. Prepare or open MainGame**, then press Play. The runtime builder creates the road and interface.
2. Inspect an environment prefab in Prefab Mode. Its `EnvironmentAssetInfo` component records its role and dimensions. Model pivots sit at ground level; road pivots sit at the segment entry. Y is up, Z is forward and X is horizontal.
3. Each road exposes `Entry`, `Exit`, and lane-anchor transforms. Flat three-lane modules are 24 meters long and 8.4 meters wide. Standalone ramp/descent exits record a ±1.2-meter rise. Match the next segment's entry to the previous exit when assembling a route.
4. Keep the `Collision design only - disabled` proxy disabled in this runner. Its player and track contacts use the existing swept collision system, independently of scenery meshes. A physics-driven project may enable and adapt these simple boxes separately.
5. The single-lane alley is an optional spur. It has one center anchor and is excluded from this three-lane route. Integrating it into gameplay would require a lane-merge and hazard-spawning controller change.
6. UI prefabs belong beneath a Canvas. Keep the phone safe-area layout and 720×1280 reference resolution. Native gameplay wording and lesson text are in `UIManager.cs` and `CoastalRouteProfile.cs`; lettering embedded in the supplied menu PNGs is edited in the original artwork documents.

**ANDAM → Environment → 1. Build graybox asset pack** generates simple neutral volumes for layout review. **2. Rebuild polished asset pack** regenerates the colored models. **3. Validate and render the vertical slice** saves phone previews under `Docs/Previews`. Rebuild commands replace the generated pack: duplicate a prefab into a separate user-art folder before making edits you want to preserve through regeneration. Ordinary Prepare preserves an existing pack.

## Mobile and gameplay behavior

The chase camera keeps a fixed position and angle during gameplay. Its framing adapts only to the screen's aspect ratio, preserving all three lanes on tall phones. Road and prop movement supplies the sense of speed; distant buildings move more slowly. Lighting uses one soft directional light, opaque scenery, restrained fog and no bloom or real-time shadow map.

Seven road slots prewarm their district views. Road vertices are fitted when a slot changes segment, then the same buffers are reused. Recurring track objects retain their pools and now use exported shared meshes. A gentle visual height profile leaves the runner at a stable local road-level origin; it does not alter scoring or swept contacts.

The version 0.6.4 fairness rules remain: one raised supply per jumpable obstacle, recovery space between obstacle groups, and at least 400 meters between bonuses. Long floods require changing lanes and hanging wires require ducking. The storm/flood surroundings keep water outside the protected elevated path. End-of-run lessons remind children to prepare with an adult, follow official instructions, stay away from fallen wires and never enter floodwater.

## Review workflow

The graybox checks precede detailed-model review. They cover compatible lane anchors, scenery clearance, gentle elevation and weather changes, bounded pooled road views and a stable camera. Phone renders cover 720×1080, 720×1280 and 720×1600 layouts. The polished pack is then checked with the existing movement, obstacle timing, power-up, menu and leaderboard suites.

Reports and screenshots are saved locally under `Docs`. Geometry and desktop layout checks do not substitute for physical Android GPU profiling or phone touch testing.

## Version 0.7.0 delivery

The polished library contains 76 prefabs. Graybox validation passed 70,474 assertions; polished environment validation passed 82,129. The existing jump, power-up and cadence suites passed, along with Opong's rig, shaders and menu raycasts. The iOS release player compiled successfully as version 0.7.0, build 6; installation on IQ17 is pending.

The reusable Unity package is `Builds/Artwork/ANDAM-Environment-and-Runner-0.7.0.unitypackage`. It includes the environment library, Opong, gameplay artwork, scripts, materials and MainGame dependencies. Import into a copy or new Unity 6000.6.5f1 project; import may replace matching assets in an existing project. The updated Sideloadly package is `Builds/iOS/ReadySetResilient-0.7.0-Sideloadly.ipa`.

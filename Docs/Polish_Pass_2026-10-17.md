# Polish pass, 17 October 2026 (evening)

This pass followed Awaiz Ahmed's playtest notes: the death screen hung, the Passenger should kill on contact and move faster, the countries looked cheap, the intro should be a walkable place with a wow moment, the ghost should chase you while you run for stamps, and countries should open out to their landmarks. Another session built the landmark vistas, the glasses sequence ("the Between"), the dimension shift and the stamp portals; see Handoff.md and QA_Report.md for that work.

## Changes

### Death, speed and the chase

- **Death card:** the failure card used to hang behind a fully opaque red fade. The fade now lifts to 25 %, and the country title card is hidden when the failure card shows.
- **Sudden death:** contact with the Passenger is fatal. Within reach it lunges for 0.45 s (0.9 s on Gentle scare intensity) and the failure card appears. There is no slow drain.
- **Speed:** every walk speed is ×1.9. A stalking Passenger breaks into a run (`PassengerState.Chase`, `Pose.Run`) once the player is within 9 m and it has been stalking for 2.5 s. It runs at 3.7 m/s on desktop and 2.8 m/s in VR (85 % on Gentle), and gives up at 26 m. Light still repels it.
- **Player movement:** desktop walk 2.6 m/s, hold Shift to run at 4.55 m/s. VR smooth movement is 2.0 m/s; push the stick fully forward to run at 3.3 m/s. The Controls page and README list both.
- **Egypt mummy:** it used to rise on top of a player standing by a wall, because its spawn point was clamped to the court. It now picks a spot with room, preferring behind the player.

### Stamps and the open world

- **Stamp portals:** when a seal is verified, the country's stamp appears (`StoryDirector.OfferStamp`). Three and a half seconds later the Passenger comes running, either breaking into a run or returning from the side away from the stamp. Collecting the stamp plays the dimension shift into the next country in story order. Passport tears still work as before.
- **Where stamps appear:** at the landmark when the cell has opened its route (`Landmarks.TryStampSpot`). Otherwise at the farthest of the arrival point and the seams, away from the Passenger.
- **Excluded:** the demo, the UAE tutorial (its stamp is the Between run) and Berlin (the finale).
- **Routes to the landmarks** (each cell calls `MarkReachable` once its route exists):

| Country | Route |
|---|---|
| Egypt | Across the sand and walkable dunes to the pyramids |
| Paris | Down to a lower quay, up a stair, over a new stone bridge, then the east bank to the tower |
| New York | A lamp-lit pier to the statue island |
| Japan | The end gate opens onto a stepped garden lane through the precinct gate |
| Riyadh | A gatehouse behind the honest arch opens onto the city ground |
| India | Garden onto the marble walk, with steps or paths round the tank |
| London | Stairs up onto Westminster Bridge, across, then down to the far embankment |

  Japan and Riyadh re-check their route with capsule casts on entry; if it's blocked, the stamp stays local.

### Countries

- **Detail pass:** every country got one, Paris and Egypt by the lead session and the rest by helper agents, one file set per country (`Cells/Detail_*.cs`). The main changes:
  - Paris shop windows with stock behind glass.
  - A Morris column, a Wallace fountain, the riverside booksellers' boxes and a facade closing the north end.
  - Egypt's feather palms with dates, the dressed pylon, obelisks and sand drifts.
  - Japan's machiya with lattices and paper-screen silhouettes.
  - Riyadh's Najdi architecture and majlis.
  - Agra's char-bagh with real cypresses.
  - Rio's frozen carnival.
  - New York's arrivals hall behind dark glass.
  - London's Georgian terrace and embankment.
  - Berlin's festival pavilion with home-front decoy doors.
  - The apartment as a lived-in home, with a wrong-feeling copy in the Margin.
- **Dark materials:** the project renders in linear colour, so texture × tint on already-dark textures (wood_dark is about 0.1 linear) came out near black. Tints are now pale and the texture carries the colour; metals use plain bright colours.
- **Agra's reflection:** the puzzle's reflection was never visible, because a solid rim covered it. It now shows.
- **Text sizes:** several signs were centimetres tall and have been resized.

### Intro

- **The walk:** the Marrakech memory is a walkable souk at dusk instead of still shots (`Cells/Detail_Souk.cs`). As the player walks, it paints itself in: stalls, arches and lanterns rise out of the ground (`World/MemoryReveal.cs`, world motion only).
- **The shop:** the keeper greets you. You look at the frames and pay by putting your festival token in the brass tray; he nods and holds out the case. Then the shop sinks back into the ground before you wake at home.
- **Rules inside the memory:** no passport, tears or threat. It can be skipped at any time.
- **The shopkeeper** is a new skinned, animated character (`Cells/Shopkeeper.cs`).

### Bugs fixed

- **Recall** now brings back the essential item held most recently. It used to bring the one registered last, which could pull an item from another country.
- **Morocco** rebuilds its counter puzzle when a new story or a load hasn't verified it, because the cell is cached for the session.
- **Saving on quit** happens only while a story is in progress. Playing the judges' demo and quitting from the menu used to overwrite the story save with the demo's run.

## Evidence

All runs used a desktop build at 1600×900 on an RTX 5080 laptop GPU, built from the repo in a separate project copy. Reports and screenshots are under `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive\`.

| Run | Result |
|---|---|
| `final1_212654\autotest_all.md` | 172 / 172 passed, 0 errors logged |
| `final2_*\autotest_persist1.md` + `persist2.md` | 15 / 15 and 16 / 16 (settings and Continue survive a restart; the demo plus quitting from the menu leaves the story save intact) |
| `mid3_205015` | demo 30 / 30, countries 55 / 55 |
| Other session: `walk4`, `walk5` | models 75 / 75, shift 21 / 21 |

## Not verified

- **No human playtest of this build.** The checks above are automated: they drive the game through its own calls, not a keyboard, mouse or controllers.
- **No Quest 3 headset run.** VR speeds, the run gesture, comfort during the dimension shift and VR performance are unmeasured. Frame times in the reports are desktop only.
- **Audio** was audited in code only: five buses, and voice ducks music and ambience. Levels weren't measured.
- **New shopkeeper:** see the fix status in the final summary; the first build showed his head and hands rendering dark.
- **The routes** were checked with capsule casts and helper previews, plus the other session's raycast walks for Egypt and Japan. Paris, New York and London have not been walked end to end in the engine.
- The trailer video and the judges' recording are not made yet.

## Later that night: first-person body, audio and world shifts

- **First-person arms (desktop).** `Player/FirstPersonArms.cs` and `Player/HandMesh.cs` add a jacketed forearm and a detailed hand on each side: nails, knuckles, a procedural skin normal map, a wristwatch and cuff buttons, with faint emission so they stay readable in the dark.
  - Walking, each hand swings up into the bottom corner on its stride; running, the fists pump into view. Spring-damper sway follows look, starts, stops and landings.
  - The left hand holds the open passport by its outer edge, clear of every printed page and the stamp box. The right hand reaches on Interact, holds carried items, and claws at a tear while widening.
- **VR hands** (`Player/VRHands.cs`): the same hand, locked to the tracked controllers. Fingers follow grip and trigger, and a sleeve lags on a spring. The tracked hand and the camera are never moved.
  - Needs a headset check: `VRHands.GripToModel` (68°) re-aligns the controller models, the hands, the VR hold point and the passport mount together. Set it to identity to revert.
- **Movement feel (desktop).** Acceleration and braking; head bob of 2.5 cm walking and 5 cm running, with a 0.6° roll only when running; a landing dip. Bob, roll and dip are off with Reduced effects.
  - Running strides are 1.18 m.
  - Fixed with it: momentum's tiny first steps fell under the controller's 1 mm minimum move at high frame rates, so the player could not move on fast displays. `minMoveDistance` is now 0, and the speed clamp applies only on side hits. Teleports place the player 2 cm above the floor so scripted walks never depenetrate at floor seams.
- **Audio** (`Audio/PlayerAudio.cs`, `Audio/ThreatAudio.cs`, `Audio/Acoustics.cs`, new generators in `Tools/audio/`). 127 new synthesised clips and a loudness pass:
  - running breaths locked to footsteps, which turn ragged with exertion, then recovery panting; held breath near the Passenger;
  - a heartbeat driven by its distance;
  - a chase stinger plus `mus_chase` while it runs at you;
  - footsteps for six surfaces with running layers;
  - per-place reverb;
  - shift sounds.

  All audio is now generated by the project's own scripts (the last CC0 footsteps were replaced). The menu music no longer carries into the story.
- **World shifts.** Stepping through a doorway tear now uses the dimension shift (1.8 s build-up), not a quarter-second fade. The shift itself, made more dramatic by the other session, shows the destination's light through the cracks.
- **Evidence** (`QA-archive\hands1_002226`, `hands2_002822`):
  - demo 32 / 32, including a real sprint at 4.5 m/s and a stop;
  - story 36 / 36, including walking the souk lane end to end through the real colliders;
  - lifecycle 58 / 58;
  - 0 errors logged.

  The screenshots show the passport fully visible with the hand on its edge, and the arms in view while running and walking.

## 18 October, early: a real mirror, Saeed's body, and the VR hands

Awaiz's VR playtest found no reflection in the bathroom mirror and crooked hands. He then shared a turnaround of how Saeed should look: a young Emirati man in a white kandura, a brown leather satchel strap over his right shoulder, brown suede shoes, dark curly hair and a short beard.

- **A real mirror** (`Rendering/PlanarMirror.cs`, `Shaders/TS_PlanarMirror.shader`). The bathroom glass at home used to be a shiny material that only showed the reflection probe.
  - While the glass is roughly in view, a reflection camera renders the room mirrored across the glass just before the player camera draws. VR uses one camera per eye, from the XR display's own eye matrices, so the reflection has real stereo depth. The glass samples that image at its own screen position.
  - The near plane is made oblique so it lies on the glass, and x is mirrored in the projection so culling stays normal. Screen-space AO is paused while a reflection draws, because it assumes a standard depth mapping. URP's fog is per pixel from the view matrix, so it stays correct.
  - The reflection is linear HDR, so the player camera grades the mirror and the room together.
  - It stops drawing when you turn away, walk out, or a wall is in the way.
  - The Margin's copy of home runs cold and slightly dim.
- **What a mirror shows.** New layers: `MirrorOnly` (13) is the player's full body, drawn only by mirrors; `MirrorGlass` (14) is the glass, never drawn into a reflection or a probe.
  - The first-person arms (`PlayerBody`) and the 3D UI never appear in a reflection.
  - The passport and carried items do appear, and the body's hands meet them.
- **Saeed's body for mirrors** (`Player/MirrorAvatar.cs`).
  - Stands at the player's feet, with the body turning after the head on a spring (never more than 40 degrees behind) and the head looking where the camera looks.
  - Walks and runs with the same gait as the first-person arms. Desktop hands go where the first-person hands are; VR arms reach the tracked controllers by two-bone IK.
  - Uses `Resources/Models/Saeed.fbx` when it is present. The Blender session is building it from the turnaround, with bones found by name. Until then it uses a procedural stand-in: the kandura and furakha, the strap to a pouch at the left hip, suede shoes, hair and beard.
  - In the Margin the reflection runs 0.35 s late. Once per visit, after you turn away and back, it is still staring into the glass, then it snaps back.
- **First-person sleeves** are the white kandura now, with a folded cuff and the watch under it, on desktop and in VR. The skin is a warmer light brown.
- **VR hands:** `VRHands.GripToModel` lacked a 180 degree roll about the handle. Unity's OpenXR grip pose points +Y forward and down out of the hand, so the hands and controller models pointed backwards. Fixed in code; still needs a headset check.
- **Desktop shortcuts ran an old build.** The Claude desktop app runs our shells, and every build, inside its MSIX package. Writes under `%LOCALAPPDATA%` land in the package's private `LocalCache`, while Explorer sees only the real AppData folder. So "The Twelfth Stamp - Desktop" opened a stale copy.
  - The game is now installed to `C:\Users\awaiz\Games\TheTwelfthStamp`, outside AppData, and both shortcuts point there.
- **Evidence** (`QA-archive\mirror1_011720`, `mirror2_012152`):
  - story 46 / 46, which includes ten new mirror checks: in the glass's screen area, 16.7 % of what the mirror shows changes when the body is left out;
  - demo 32 / 32;
  - 0 errors logged.

  Screenshots `20b_bathroom_mirror` and `20c_bathroom_mirror_angle` show the reflection of the room and of the stand-in body.
- **Not verified:** VR rendering of the mirror and the VR hand fix need the headset. Both are untested in VR.

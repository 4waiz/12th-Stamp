# The Twelfth Stamp: QA report

Build 0.9.0, Windows x64 (Unity 6000.6.2f1, URP, Mono), tested on the development PC: Intel Core Ultra 9 275HX, NVIDIA GeForce RTX 5080 Laptop GPU, Windows 11. Last updated 17 Oct 2026.

Every claim below names its evidence. "Automated" means the in-build AutoTest drove the real game systems (the same calls the input layer makes) in a desktop window. It does not judge feel, comfort, audio quality or VR rendering.

## Evidence by category

| Category | Status | Evidence |
|---|---|---|
| Compiles and builds | Yes: 0 errors, 4 warnings (an obsolete API in the editor setup script, one unused field) | `Game\Logs\build_*.log` |
| Unit tests (EditMode) | 19 / 19 passed | Unity Test Runner, batch mode: `Logs\editmode_results.xml` |
| Desktop, automated full playthrough | See "Latest automated run" below | `Docs/QA/run8/autotest_all.md` + screenshots. Earlier runs: `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive` |
| Desktop, played by a person | **Not done** | n/a |
| XR device simulator | **Not used** | n/a |
| Quest 3 headset, played by a person | **Not done. Needs Awaiz** (checklist below) | n/a |
| Audio listened to by a person | **Not done** | Loudness and pronunciation were measured: `Audio/AudioManifest.md` |

## Latest automated run

`TheTwelfthStamp.exe --mode=desktop --autotest=all` (one process, real build, about 3.5 minutes). The final run's report and screenshots are in `Docs/QA/run8` (only eight key screenshots are committed to git). Earlier runs are archived outside OneDrive in `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive`.

| Run | Result | Notes |
|---|---|---|
| walk7, `--autotest=models` | **75 passed, 0 failed**, 0 errors or exceptions logged | Japan's outer precinct gate is now a real passage, 3 m wide with 3.4 m headroom, on the pagoda's sight line where the street's new lane arrives: white plaster framed by vermilion posts and beams, on stone plinths. Japan's own walk check (capsule casts along the route) now finds it clear, so Japan opens itself and its stamp spot is offered; Riyadh's route is open too. Precinct walls and the gate sit off the Floor layer, so VR cannot teleport onto them. New shot `m20b_japan_gate`. `...\QA-archive\walk6`, `...\walk7` |
| walk5, `--autotest=models` | **75 passed, 0 failed**, 0 errors or exceptions logged | The repo as of 20:50, with the polish session's opened routes (Paris and Agra call `Landmarks.MarkReachable` themselves) and the dunes it added in Egypt. The stamp-spot check now accepts countries that open themselves: the spot is offered where a route exists, stays off elsewhere, and MarkReachable turns it off and on in all seven. Egypt is sampled every 25 cm: 0 gaps, largest rise 18 cm (a dune near the court; the player steps 30 cm). New shot `m25b_agra_tank` shows the tank's steps and the paths round it. Walk4, the same repo with a 1 m sampling version of the Egypt check, also ran `shift` (21 / 21) and `all` (168 / 171). The three `all` failures are the polish session's Paris sudden-death checks: in that run the Passenger never reached the player at all. `...\QA-archive\walk4`, `...\walk5` |
| walk3, `--autotest=models` | **68 passed, 0 failed**, 0 errors or exceptions logged | Adds the first opened country. Egypt has solid ground every metre along the 117 m from the court's sand floor to the stamp spot by the pyramids: 0 gaps, the largest step is the 6 cm edge of the court floor, and it is sand underfoot. The vista ground now carries footstep surfaces (sand in Egypt, stone elsewhere). The same build passed `--autotest=shift` 21 / 21. Its `all` run scored 161 / 165, and none of the four failures is in this work. One is the souk check above. The other three are the polish session's sudden-death checks in Paris: the log shows the Passenger does catch the player, but after the test's 4 s window. Run alone, `--autotest=lifecycle` failed the same way. Those checks passed in walk1 with the same Paris and Passenger code, so the timing is fragile; reported to the polish session. `...\QA-archive\walk3` |
| walk2, `--autotest=models` | **66 passed, 0 failed**, 0 errors or exceptions logged | The models gallery plus two new checks for each of the seven landmarks: the ground under its stamp spot is solid, and the spot is offered only once the cell opens its play area (`Landmarks.TryStampSpot` / `MarkReachable`). Private copy `%LOCALAPPDATA%\TheTwelfthStamp\ShiftCheck` (repo as of 19:50, plus these files). `...\QA-archive\walk2\models` |
| walk1, `--autotest=all` | **164 passed, 1 failed**, 0 errors or exceptions logged | The full game with the new glasses moment and the walkable landmark ground. "Glasses put on" passed after 2.4 s and "the Margin copy in place" after 9.7 s (the Between runs itself in test mode). The one failure is the polish session's new souk check ("the memory starts on solid ground at the far end of the souk"), added while its helper is still building that floor. `...\walk1\all` |
| shift2, `--autotest=shift` and `--autotest=story` | shift **21 passed, 0 failed**; story **34 passed, 0 failed**; 0 errors | The art-review pass of the glasses moment (see "The glasses moment and the Between"), with its 11 screenshots reviewed, and the story flow through it. In walk1 the shift pass scored 20 / 21: its one failure was the polish session's intro-cover check, which passed in walk1's `all` on the same build (a timing flake in that check). `...\shift2` |
| vista4, `--autotest=all` | **162 passed, 0 failed**, 0 errors or exceptions logged | The landmarks in their new surroundings (see "Landmarks and their surroundings"), with the rest of the game as it stood in the repo at 19:31 (it now has 162 checks, including the polish session's lifecycle flow). Built in a private copy of the project, `%LOCALAPPDATA%\TheTwelfthStamp\ModelsCheck`. Window 1600 x 900: frame time median 1.0 ms, p95 1.6 ms, worst 147 ms. Desktop window only, **not a VR measurement**. `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive\vista4\all` |
| vista4, `--autotest=models` | **52 passed, 0 failed**, 0 errors or exceptions logged | The models gallery, now with a check that each of the seven landmarks stands in its vista. Same build. `...\vista4\models` |
| vista1 to vista3 | Models 52 / 52 each time. `all`: 160 / 162, then 150 / 162 and 159 / 162 | Earlier rounds of the same pass. Every failure was in the polish session's lifecycle checks (sudden death, retry, pause, recall, stalking) while it was still writing them. vista2 also stalled 75 minutes on one wait. vista4 passes them all. |
| checkpoint2_1639, `--autotest=all` | **108 passed, 0 failed**, 0 errors or exceptions logged | After the story rework (it now has 108 checks) and with all the Blender models in. `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive\checkpoint2_1639\all` |
| checkpoint2_1639, `--autotest=models` | **44 passed, 0 failed**, 0 errors or exceptions logged | Art-review gallery of the Blender models (see "Blender models in the game"). Same build. `...\checkpoint2_1639\models` |
| run8 (final) | **109 passed, 0 failed**, 0 errors or exceptions logged | New HUD and menus, modelled glasses, case and Passenger. Ultra preset, window 1600 x 900. Frame time median 1.1 ms, p95 1.7 ms, worst 45 ms. Desktop window only, **not a VR measurement**. |
| run5 | 108 passed, 0 failed | Before the UI redesign. Window 2560 x 1494: median 1.7 ms, p95 2.5 ms. |
| Final build, Egypt only | 7 passed, 0 failed | After the last mummy-wrap tweak. |
| Release ZIP, judges' demo | 18 passed, 0 failed | Unzipped to a fresh folder and run from there. |

What the run covers:

- **Menu:** the menu cell loads and the main menu opens.
- **Judges' demo (Paris):** the Passenger guards the desk and blocks the trace. Open the passport, select Egypt, target the desk seam, open a slit (Egyptian sunlight flows through), widen to a window, sunlight drives the Passenger off the desk, a hand reaches through, seal the tear, verify France, the counterfeit HOME stamp is revealed, demo results appear, and the story save is untouched.
- **Egypt:** reflector puzzle, trace revealed, seal verified, sunlight unlocked.
- **Story:** shop prologue (shopkeeper, payment, glasses into the case), travel home, unpack, voicemail, open the case, wear the glasses, the apartment turns into the Margin copy, first tear, genuine note revealed, UAE verified, tutorial tear sealed, first threat, doorway escape to Egypt.
- **Countries:**
  - USA: breaker, double-arrival screen, noisemaker lure, gate clears.
  - Japan: lantern on the correct hook.
  - Saudi Arabia: optical panel aligned, honest arch.
  - India: lamp on the correct pedestal.
  - Brazil: three stamps on the drum beat.
  - UK: tear opened under the chime.
  - Morocco: counter rebuilt, calibration insert, optician's notes.
  - Each country ends with its seal verified.
- **Finale:** Berlin plinth accepts eleven seals, the mimic appears, noisemaker, sunlight exposes the forged doors, VOID stamp, the resource tear is sealed, the true return opens on the blank wall, near miss, virtual glasses removal, real home, glasses broken, bathroom, epilogue, results.
- **Art review screenshots** (not pass/fail): close-ups of the Passenger in Egypt (the mummy rising behind you), New York and Berlin.

**VR start-up without a headset** (run by hand): `--mode=vr` with no headset connected. The OpenXR loader did not start, and the game logged its plain-language fallback ("The OpenXR runtime did not start. Open Meta Horizon Link…, then choose Retry"). The Retry / Desktop Mode / Quit menu comes from the same code path. Its on-screen appearance was not captured.

Not covered by the automated run: failing and retrying from a checkpoint, pausing, settings changes, saving and continuing across a restart, and the Passenger draining the player in Marrakech (switched off for that puzzle check).

## Blender models in the game

Every FBX in `Game/Assets/Resources/Models` (built by the scripts in `Tools/blender`, described in `Art/Models/MODELS.md`) is now used, except `Passport_Open.fbx`. Each one replaces only a procedural mesh. Gameplay colliders, sockets, interactables and anchors stay on the procedural objects, and the procedural mesh comes back if a model file is missing (`Models.Spawn` in `Game/Assets/Scripts/World/Models.cs`).

| Where | Models |
|---|---|
| Hand props | glasses (open, folded), glasses case, closed passport, vintage camera, phone, festival token |
| VR | left and right controllers; the onboarding callouts and highlights attach to the model's buttons |
| Apartment (both copies) | sofa, dining table, four chairs, kitchen run, fridge, bathroom vanity and mirror, front door, open suitcase |
| Countries | Paris festival desk and its three street lamps, Egypt reflector, New York noisemaker (also the one carried to Berlin), Japan hand lantern, Riyadh optical panel, Agra garden lamp, Berlin VOID stamp |
| Landmarks | lattice tower, statue, pagoda, arch tower, domed pavilion, clock tower, column gate. Each has its own light and haze and stands in surroundings built for its cell (see "Landmarks and their surroundings"). |
| Passenger | body parts (since run8) |

Evidence: `--autotest=models` (52 checks in vista4) confirms each expected model in its cell, all six controls on both controllers with their callouts, the reflector's mirror turning with the puzzle, world-scale tiling on the kitchen, and each landmark standing in its vista. Its screenshots were reviewed one by one. Landmarks are shot twice from their own ground: once as the game shows them, and once with their haze switched off. `--autotest=all` passed 108/108 with 0 errors on the build that first wired the models in (checkpoint2_1639), and 162/162 in vista4. The opening shot (`20_home_bag`), the Paris demo (`02_paris_arrival`) and the real apartment seen through the home tear (`53_home_tear`) all show the models.

Where a model did not fit a fixed gameplay position, the visual was adapted, not the gameplay:

- **Suitcase:** the story rests the camera, passport and case at 0.2 m, on the bag's collider (also the "Go through your bag" target). The model's open shell is 13 cm deep, so it is packed with folded clothes up to that height.
- **Paris desk:** the model is a 0.76 m desk, but the checkpoint's colliders and the ledger trace sit at 1.01 m. The desk body is stretched to that height. The ledger, ink pad, stamp and lamp go where the procedural ones were, and the lamp light is unchanged.
- **Dining table:** raised 1.5 cm to the procedural top, where the phone and the table spots rest.
- **Egypt reflector:** scaled 4 % so its mirror centre meets the puzzle pivot. Its yoke and mirror turn with the pivot; the base stays still.
- **Riyadh optical panel:** the model turns about its vertical axis, but the puzzle spins the pattern about the disc's axis. The lens stays still and the calibration pattern turns just in front of the glass.
- **Fridge notes:** the French-door model has its handles on the centre seam, which hid the genuine UAE note (found in the gallery screenshot). With the model, the four notes sit on the left door, the genuine one directly under the UAE trace, which has not moved.
- **Not used:** the open passport in hand stays procedural, because its seal, portrait and text overlays are flat quads that would not line up with the model's angled, curled pages. The bathroom door stays procedural, because its leaf and collider are 1.9 m wide and it closes in the ending, while `Door.fbx` is a 0.9 m door; that model is the front door instead.
- **Through a tear:** a model that mixes glass with opaque parts now hides only its glass when its cell is seen through a tear. Before, any glass hid the whole object.
- **Texture scale:** an import rule (`TwelfthStampModelRules` in `ProjectSetup.cs`) records each mesh's UV density, so big meshes keep about one texture repeat per metre (the kitchen tiles at 8x instead of stretching one tile over 4 m).

### Landmarks and their surroundings

Before this pass, the landmarks read as flat, fog-coloured cut-outs hanging in the haze. The backdrop shader's fog hid all of anything 60 to 230 m away, and nothing stood under them. Each landmark now stands in a vista built for its cell's view (`Game/Assets/Scripts/World/Vista.cs`, used by `Landmarks.cs`):

- **Light and haze.** Each landmark material is a copy of the cell's backdrop material with its own light, and a gentler haze that grows with distance (1 - e^(-rate * d)). At the landmark's distance the haze replaces 22 % (the Berlin gate) to 42 % (the Riyadh tower) of its colour. Night landmarks are floodlit from below:
  - the lattice tower in gold;
  - the statue's copper in green, with a glowing torch;
  - the pagoda's walls warm against dark tiled roofs;
  - Westminster's stone in gold, its four clock faces lit, with hands at one minute to midnight.

  Day and dusk landmarks take the cell's sun.
- **Around each landmark** (cell coordinates, outside the playable area):
  - **Paris:** the Seine runs on north between stone quays, whose top meets the cell's footbridge at x 54. Along it: Haussmann blocks with lit windows and zinc roofs, plane trees, quay lamps reflected in the river, the Pont d'Iéna, and the Champ de Mars with a pool of light under the tower.
  - **New York:** Liberty Island in the harbour, with a seawall, lamps, lawns, trees and two low buildings, plus Manhattan's lit waterfront under the skyline. The statue now faces the player (the old spawn turned it 180 degrees, away from the player).
  - **Japan:** a temple precinct laid out on the pagoda's sight line: a lantern-lined approach, red maples, cedars, white walls, a one-storey outer gate and the main hall with lit shoji. It follows the pagoda, which the polish session moved to x -10 so its upper tiers show above the street's end gate. A two-storey gate on that line hid the pagoda completely, so the gate is one storey.
  - **Riyadh:** a city grid at dusk: avenues and streets with sodium lamps and median palms, low blocks with lit windows, glass mid-rise towers round the landmark, and haze at street level.
  - **Agra:** the cell's garden carries on to the tomb. The channel and marble walks run through a raised tank, with cypress rows like the cell's own, broadleaf trees, red sandstone walls with domed corner towers, and red-and-white side halls. A great gate stands behind the player, and dawn mist lies at the platform.
  - **London:** the Thames runs on past the cell's water. On the far embankment are lamps, plane trees and the floodlit Gothic palace: its river front, pavilions and pinnacles, plus the central and Victoria towers. The palace stops short of the sight line, and a low wing joins the clock tower so its faces stay clear. Lit buildings stand beyond, and Westminster Bridge has green arches and two rows of lamps.
  - **Berlin:** the blocks along Pariser Platz and the two houses flanking the gate. The road runs on through the Tiergarten to a victory column, seen through the central passage, with city blocks fading in the haze beyond.
  - **Egypt** (procedural pyramids): desert past the cell's sand floor, three queens' pyramids, a field of mastaba tombs, low dunes and heat haze. The third pyramid moved inside the 320 m sky dome; it had stood 323 m out.
- **Skylines** (`Landmarks.Skyline`, seen from New York, Riyadh, Rio, the menu and the apartment window) now have facades of windows, with lit windows at night, setbacks, spires and red beacons, under the same haze. Towers that would land on a landmark's site are left out. Berlin's skyline call was removed by the polish session, because the vista builds that city.
- **Lights and mist.** Lamps are soft additive halos with reflections on the water and pools of light on the ground. Mist banks sit at the landmarks' feet. Window textures are drawn by code at start-up with fixed seeds.
- **Cost.** Everything is merged into one mesh per material, with no colliders and no shadows. Per cell that is 21 (New York) to 118 (Riyadh) scenery meshes. vista4's frame time, a 1.0 ms median and 1.6 ms p95 in a desktop window, matches run8's (1.1 ms and 1.7 ms).

Reviewed in vista4:
- the seven landmark shots, with and without haze;
- the arrival views of every country with a landmark (`02`, `10`, `30`, `32`, `34`, `36`, `39`, `50`);
- the menu, Rio and apartment skylines (`01`, `38`, `m04`).

Limits:
- The surroundings are scenery only: no colliders, so the player cannot walk to a landmark.
- Seen through a tear, they take the cell's ordinary heavy pocket fog.
- None of this has been seen in a headset.

Not verified: how the controller models sit in the hand in a headset. They follow MODELS.md: origin at the grip, no extra rotation under the grip pose.

## The glasses moment and the Between

Putting the glasses on used to fade straight into the corrupted apartment. Now (`Story/HomecomingChapter.cs`, `Story/DimensionShift.cs`, `Story/BetweenRun.cs`, `World/StampPortal.cs`):

1. **The glasses go on.** They rise from the open case to the eyes as an object and turn to face the way the player looks. They settle with a click, the rims and bridge framing the view for a moment on desktop; in VR they stop a hand's width from the face.
2. **Subtraction, then the shift.** The fridge stops, the voicemail starts repeating, and the room comes apart. Shards of the room lift and circle, dark tears split the walls, the light sinks into a slow red pulse and whispers swell. On desktop a short red streak tunnel opens at the peak.
3. **The Between.** A stone causeway with glowing edges hangs over a red abyss. Pieces of the apartment drift past, and the landmarks of the countries ahead (the lattice tower, the statue, the pagoda, the arch tower, the domed pavilion, the clock tower, the column gate) hang huge and tilted against a red horizon. Running footsteps sound behind, and the runner's shadow slides past before the runner itself (a smoked-glass body) sprints out of a red glow. The player runs, grabs three pieces of the home stamp, and the seal at the far end opens.
4. **Caught** means thrown back to where you landed, keeping the pieces collected. After two catches the runner slows. This runner is not the Passenger: there is no drain and no death here.
5. **The seal** shifts the player into the Margin apartment, standing where they put the glasses on. The UAE tutorial runs as before; its first threat stays a slow stalk (`AllowChase = false`).

Comfort rules from the brief, as built:
- The camera and rig are never moved, shaken or turned. The player sees the runner by turning, or by its shadow, and the only forced move is the fade-through when caught.
- The red pulses at 0.7 Hz and there are no flashes.
- The streak tunnel lasts 1.5 s, is world-fixed, keeps the room visible through it, and is off in VR, with Reduced effects and on Gentle.
- Gentle halves the build-up, softens the grade and keeps the runner below walking pace, so it never catches anyone who walks.
- Once a full shift has been seen, the Skip input jumps straight through.
- If the sequence is abandoned (quit to the menu), a guard puts the lights, grade and fade back and removes everything it built.

For the polish session's stamp flow, `DimensionShift.Run(swap, buildUp)` is the travel transition. `StampPortal.Spawn(parent, worldPos, country, onCollected, portal)` places a collectible stamp (portals can wait, dimmed, until `Ready`). `Landmarks.TryStampSpot(cell, out pos)` gives a spot near each landmark on ground that now has colliders, but only after the cell calls `Landmarks.MarkReachable(cell)`.

Evidence: `--autotest=shift` covers the glasses rising and worn, the shift building and at its peak, the Between from the start and from the side, the runner behind and closing in, a stamp piece, the open seal and the Margin arrival. It checks that the swap happens at the peak, input unlocks, the runner gains on a standing player, catching throws the player back, the seal stays shut until all three pieces are held, and the player ends in the Margin where they stood. `--autotest=story` runs it end to end.

Not verified: any of it in a headset (VR stops the glasses short of the face and has no tunnel, but nobody has seen it), Gentle and Reduced-effects modes on screen, and the Skip path.

## Fixed this session (found by the automated run or by reviewing its screenshots)

| Problem | Fix |
|---|---|
| Berlin: sunlight could never expose the forged doors (the 10 m beam was 10.4 m from the target). Every later finale step failed. | The light seam was moved to face the middle door 6.9 m away. Exposure is now sampled in front of the door at three heights. |
| Berlin: the three "HOME" decoy doors were built facing into the wall, so their glow and label were hidden and mirrored. | Rotated to face the room. |
| After finishing the story or quitting during the prologue, tears and threats stayed disabled for the next run and the judges' demo. | Chapter gates reset at every campaign or demo start and on return to the menu. |
| A finished story was still offered as "Continue". | A completed save no longer counts as continuable. |
| Glossy glass under point lights bloomed into screen-sized white blobs (the Passenger in New York, London and Berlin). | Bloom threshold 1.0 and clamp 8. |
| Standing inside a light shaft (the Marrakech counter lamp, sunlight pouring past you) filled the view with bright panels. Rain streaks smeared across the lens. | Light shafts and particles fade within about 2 m of the camera. |
| HUD: control prompts were drawn across the middle of the screen over the passport, the aim dot sat on the text, the destination label collided with the prompts, and the "verified" tick and tutorial checkboxes were missing from the font (shown as boxes). | HUD redesigned: key-cap prompts beside the reticle, one passport/tear card on the right with a tear gauge and contamination meter, subtitles and toasts sized to their text, a vitality bar that hides when full, and only glyphs the fonts contain. |
| The passport was unreadable in dark scenes. | The pages glow faintly. The passport moved lower left to keep the centre clear. |
| Menus: on desktop the paper card was off to one side and skewed, its title and bottom buttons were clipped or drawn behind the paper, and the disabled "Continue" was unreadable. | Desktop menus are centred and square to the camera. The draw order is fixed, the paper has proper margins, buttons are ink on paper with a hover wash, and toggles show their value on the right. |
| Disguise pieces hung on bones (Egypt's wraps, the Paris umbrella, hair, lens rig, petals) were never removed, so the Passenger wore every previous country's disguise at once. | Pieces are tracked per disguise and removed on every change. Verified in the Berlin close-up. |
| Egypt's mummy read as bronze rings on glass. | Overlapping stained grave linen, a bound head with only the photo oval showing, and long trailing strips. |

## Known issues and gaps

- **VR is unverified.** The VR rig, controller labels, teleport, snap turn, stereo tear rendering and pause on headset removal are compiled but have not been run in a headset.
- At start-up, Unity's OpenXR plugin asks the active runtime (Meta Horizon Link) which GPU drives the headset. With no headset connected, it writes an `XR_ERROR_FORM_FACTOR_UNAVAILABLE` diagnostic report to `Player.log`, even in desktop mode. Desktop play is unaffected.
- New York, London and the corrupted apartment are very dark on a laptop panel. The Brightness setting compensates.
- Some rooms (the shop, the apartment, the Berlin pavilion) are plain box geometry, and the shopkeeper is a simple mannequin.
- 3D models from Blender are used for the hand props, the controllers, the apartment furniture, the country props and the landmarks (see "Blender models in the game"), each only when its file is in `Game/Assets/Resources/Models`. Rooms, streets, buildings, the landmark surroundings, the passport in hand, the bathroom door and the disguises are procedural. The landmark surroundings now have colliders (ground, walls, quays, bridges and the island), so they can be walked once a country's play area is opened to them. That bounds-and-paths work belongs to the polish session, and `Landmarks.TryStampSpot` stays off until then.
- The glasses moment (the dimension shift and the Between) has not been seen in a headset.
- The controller models have not been seen in a headset.
- The hand that reaches through a tear is still the older procedural hand, not the modelled one.

## Headset checklist (for Awaiz, about 15 minutes)

Run "The Twelfth Stamp - Quest 3" with Link connected and Meta Horizon Link active.

1. Main menu appears in the headset (not only on the PC window). Point and click with either controller.
2. Controls page: each label matches the button you press. Raise each controller to your face: the controller model should sit the right way round in your hand, and its labels should point at the right buttons.
3. Judges' demo start to finish: passport on X, Egypt on B, tear by gripping the seam and pulling (or holding the right trigger), seal on A.
4. Both eyes show the same view through the tear (close one eye at a time).
5. Teleport (left stick) and snap turn (right stick) are comfortable. Try smooth movement in Settings.
6. Take the headset off during play: the game pauses.
7. Pause with the left Menu button. Quit from the pause menu.

Report anything that fails, with what you saw.

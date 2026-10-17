# The Twelfth Stamp: QA report

Build 0.9.0, Windows x64 (Unity 6000.6.2f1, URP, Mono), tested on the development PC: Intel Core Ultra 9 275HX, NVIDIA GeForce RTX 5080 Laptop GPU, Windows 11. Last updated 17 Oct 2026.

Every claim below names its evidence. "Automated" means the in-build AutoTest drove the real game systems (the same calls the input layer makes) in a desktop window. It does not judge feel, comfort, audio quality or VR rendering.

## Evidence by category

| Category | Status | Evidence |
|---|---|---|
| Compiles and builds | Yes: 0 errors, 4 warnings (an obsolete API in the editor setup script, one unused field) | `%LOCALAPPDATA%\TheTwelfthStamp\Logs\build_*.log` |
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
- 3D models from Blender are used for the glasses, the glasses case and the Passenger's body, and only when the files are in `Game/Assets/Resources/Models`. Everything else is procedural.
- The hand that reaches through a tear is still the older procedural hand, not the modelled one.

## Headset checklist (for Awaiz, about 15 minutes)

Run "The Twelfth Stamp - Quest 3" with Link connected and Meta Horizon Link active.

1. Main menu appears in the headset (not only on the PC window). Point and click with either controller.
2. Controls page: each label matches the button you press.
3. Judges' demo start to finish: passport on X, Egypt on B, tear by gripping the seam and pulling (or holding the right trigger), seal on A.
4. Both eyes show the same view through the tear (close one eye at a time).
5. Teleport (left stick) and snap turn (right stick) are comfortable. Try smooth movement in Settings.
6. Take the headset off during play: the game pauses.
7. Pause with the left Menu button. Quit from the pause menu.

Report anything that fails, with what you saw.

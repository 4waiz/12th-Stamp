# The Twelfth Stamp: status and handoff

Last updated 17 Oct 2026. Evidence for every "verified" line is in `Docs/QA/QA_Report.md`.

## Where things are

| What | Where |
|---|---|
| Unity project | `Game/` (Unity 6000.6.2f1, URP). The scene is empty: the game builds its world from code when it starts. |
| Game code | `Game/Assets/Scripts` (Core, Cells, Story, Passport, Passenger, Player, Interaction, Rendering, UI, Audio, Input) |
| Shaders | `Game/Assets/Shaders` (tear stencil, pocket-world lighting, light shafts, particles, sky) |
| Editor and build scripts | `Game/Assets/Editor` (`ProjectSetup.cs`, `BuildScript.cs`) |
| Generated textures, audio, narration | `Game/Assets/Resources` (generators in `Tools/textures`, `Tools/audio`) |
| Windows build | `%LOCALAPPDATA%\TheTwelfthStamp\Builds\Windows\TheTwelfthStamp.exe` (outside the repo and outside OneDrive) |
| Shortcut installer and player README | `Tools/install` (copied next to the .exe by every build) |
| Release ZIP (not uploaded) | `%LOCALAPPDATA%\TheTwelfthStamp\Release\TheTwelfthStamp_v0.9.0_Windows.zip`, 110 MB, SHA-256 `21A26786AC350B5D580343F3B2DF5A3DCB102A4C0995AA58F19A3DA6CF6ECEDE`. Rebuild it with `Tools/install/Package-Release.ps1`. |
| Desktop shortcuts | "The Twelfth Stamp - Quest 3" and "The Twelfth Stamp - Desktop" on your Desktop, pointing at the build above |
| Automated test reports and screenshots | `Docs/QA/QA_Report.md`, final run in `Docs/QA/run8/`, earlier runs in `%LOCALAPPDATA%\TheTwelfthStamp\QA-archive` |
| References, credits, pitch | `Art/References/REFERENCES.md`, `Docs/Credits_and_AI_Disclosure.md`, `Docs/Pitch_and_Demo.md` |

## Build, test, install

All commands run from any folder. Close the Unity editor first if it has the project open.

```bash
"C:\Program Files\Unity\Hub\Editor\6000.6.2f1\Editor\Unity.exe" -batchmode -nographics -quit -projectPath "C:\Dev\12th-Stamp\Game" -executeMethod TwelfthStamp.EditorTools.BuildScript.SetupAndBuild -logFile build.log
```

```bash
"%LOCALAPPDATA%\TheTwelfthStamp\Builds\Windows\TheTwelfthStamp.exe" --mode=desktop --autotest=all --report-dir="C:\Dev\12th-Stamp\Docs\QA\latest" -screen-fullscreen 0 -screen-width 1600 -screen-height 900
```

`--autotest` accepts `demo`, `egypt`, `story`, `countries`, `finale` or `all`. The test drives the real game systems, writes `autotest_<name>.md` plus screenshots, and exits with code 0 only if every check passed. It never writes to your save.

```bash
"%LOCALAPPDATA%\TheTwelfthStamp\Builds\Windows\Install Shortcuts.cmd"
```

The installer creates "The Twelfth Stamp - Quest 3" and "The Twelfth Stamp - Desktop" on your Desktop, pointing at the game in that folder.

## Story flow (all eleven countries are playable)

Marrakech shop prologue → Abu Dhabi homecoming → the apartment turns into the Margin copy (UAE tutorial: first tear, first seal, first threat) → Egypt, France, USA, Japan, Saudi Arabia, India, Brazil, UK, Morocco (any order after the tutorial; Egypt unlocks sunlight, New York the noisemaker, Marrakech the calibration insert) → Berlin finale (validate eleven seals, expose the forged doors with sunlight, stamp VOID, open the true return on the blank wall) → near miss and the virtual glasses removal → real home, break the glasses, bathroom, epilogue → results → credits → menu.

## Needs you (cannot be done or verified by me)

1. **Headset check on the Quest 3** (about 15 minutes). Run "The Twelfth Stamp - Quest 3" with Link connected and Meta Horizon Link as the active OpenXR runtime. Check each item and tell me what failed:
   - Both eyes render the same world, including the view through an open tear (stereo).
   - Controller labels on the in-game Controls page match the buttons you press.
   - Grab (grip), use (trigger), passport (X), destination (B), seal (hold A), recall (Y), pause (Menu).
   - Teleport and snap turn are comfortable. Smooth movement works when chosen.
   - Taking the headset off pauses the game.
   - The judges' demo runs from start to finish.
2. **Deadline and rubric.** The jam FAQ says the deadline is announced during the weekend. Tell me the exact time and timezone, and whether the on-site rubric in the brief is final.
3. **Listening pass.** Nobody has listened to the narration, music or effects on headphones or the Quest speakers yet.
4. **Commit and submission.** The work is not committed yet, and nothing has been pushed or uploaded. Submission to itch.io is yours to approve.

## Known gaps

- VR has been compiled and its start-up fallback path exists, but no person has played it in the headset.
- New York and London are very dark on a laptop screen. Raise Brightness in Settings, or ask me to lift those scenes.
- The glasses, the glasses case and the Passenger's body use Blender models (`Game/Assets/Resources/Models`, documented in `Art/Models/MODELS.md`). If a model file is missing, the game falls back to its procedural version. Everything else is procedural. The passport FBX files are not used yet.
- No Android module is installed, so there is no standalone Quest APK. The game runs on Quest 3 as PC VR over Link.

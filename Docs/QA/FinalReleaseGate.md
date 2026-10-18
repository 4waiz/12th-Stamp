# The Twelfth Stamp: final release gate

Audited 18 Oct 2026, from 06:40. The build under test is commit **eb4662c**, built at 06:33 from a clean tree (still clean
after the build). It is installed in `C:\Users\awaiz\Games\TheTwelfthStamp` and packaged as
`C:\Dev\12th-Stamp-builds\Release\TheTwelfthStamp_v0.9.0_Windows.zip` (130.2 MB, SHA-256
`B0812C26FBF875B02499FB3EA2FE2887FC10276EBD369C2AAE2789D23C65AFC4`). Unless noted, report paths are under
`C:\Dev\12th-Stamp-builds\QA-archive`.

"Automated" means the in-build AutoTest, which drives the real game systems in a desktop window. It does not judge feel,
comfort, audio quality or VR rendering.

**Verdict: not release-ready yet.** Gates 8, 9 and 10 need a person (real-input play, the Quest 3 headset, a listening pass).
Gate 3 is partly unverified, and gate 13's in-game text fix ships with the next build.

| # | Gate | Status | Evidence |
|---|---|---|---|
| 1 | Project compiles; the build comes from current source and is traceable | **PASS** | Build of eb4662c at 06:33 with 0 errors and 10 warnings: `C:\Dev\12th-Stamp-builds\Logs\build_final_1018_0633.log`. `git status` was clean before and after. The ZIP's 204 files are identical to that build (gate 12). |
| 2 | Desktop and Quest 3 shortcuts: target, start-in, mode, icon, launch | **PASS (desktop) / UNVERIFIED (Quest launch)** | Both .lnk target `C:\Users\awaiz\Games\TheTwelfthStamp\TheTwelfthStamp.exe` with `--mode=desktop` / `--mode=vr`, start in that folder, and use its `TheTwelfthStamp.ico`. The Desktop shortcut was opened through Explorer: the process started from explorer.exe with `--mode=desktop`, the window "The Twelfth Stamp" stayed responsive and closed normally, with no headset needed. The Quest shortcut needs the headset (gate 9). |
| 3 | Menu, Story, Continue, Judges' Demo, Controls, Settings, Pause, Credits, Quit, Results | **PARTIAL** | Automated PASS (`final_1018_0633_all`): main menu, Controls and Back, the Judges' Demo button, pause, Main menu, Retry from checkpoint, demo results and campaign results. Two-launch save test PASS (`gate_1018_0649_persist1` 15/15, `persist2` 16/16): settings survive a restart, and Continue is offered and resumes at the saved Paris checkpoint. **Unverified:** that Settings visibly and audibly change the game, the Credits page, the Quit button, and that control labels match real key presses. These are to be checked with real input in the capture session. |
| 4 | Story: the Marrakech purchase, home, the glasses on, the corrupted reality | **PASS (automated)** | `final_1018_0633_story` 48/48. Design note: by Awaiz's direction the story opens at home in Abu Dhabi, and the purchase is a playable flashback. You walk the souk, the shop paints itself in, you pay with the festival token, and the memory folds away. Back home he puts the glasses on, and the room turns into the Margin. |
| 5 | All 11 countries are playable, not just title cards | **PASS (automated)** | `final_1018_0633_all` (the countries flow), `final_1018_0633_explore` 97/97 (the expanded areas in all 11), `final_1018_0633_guide` 95/95 (walkthrough, briefing, step banner, marker and map in all 11). |
| 6 | Passport, slit/window/door tears, destinations, borrowed light and sound, sealing, verification, the Passenger | **PASS (automated)** | The demo, Egypt and countries flows in `final_1018_0633_all` (191/191). The Passenger's threat, repel, pause-freeze and retry checks are included. No human verification yet (gate 8). |
| 7 | Berlin finale and the return home | **PASS (automated)** | `final_1018_0633_all`: daylight exposes the forged HOME doors, VOID cancels the forged stamp, the near miss is survived, the glasses come off, then the bathroom, the epilogue, campaign complete and results. Saeed's face is lit in the bathroom mirror (luminance 0.243). |
| 8 | Real keyboard/mouse play, including mistakes, restart, save/continue, tutorial skip, fail/retry | **UNVERIFIED** | Only Awaiz's informal play so far (he got stuck in Egypt, which led to the step-by-step guide). Planned: a real-input capture session on the installed build. |
| 9 | Quest 3: stereo, controllers, world-space UI, locomotion, interactions, audio, pause | **UNVERIFIED** | PC side ready: Meta Horizon Link is installed and its service (OVRServer) is running, Meta's `oculus_openxr_64.json` is the active OpenXR runtime (read, not changed), and `OculusMirror.exe` is present. In code, the HUD switches to a world-space canvas in VR (`UI/Hud.cs`), menus are 3D panels (`UI/Ui3D.cs`), and the passport is held in the hand. **Needs Awaiz in the headset.** |
| 10 | Sound and dialogue listened to | **UNVERIFIED** | Measured only (`Promo/Production/QA/game_audio_levels.tsv`): none of the 63 voice, music and ambience files clip; voice sits at -21.5 to -17.5 LUFS, music at -24 LUFS and ambience at -27.9 to -30.4 LUFS. **Needs a human listening pass** on headphones and in the headset. |
| 11 | Cover, logo and icon at startup, in menus, on shortcuts, the executable and packaging | **PASS (desktop)** | Automated: "intro shows the approved cover" and "welcome screen shows the key art and the logo beside the menu card". The icon extracted from the executable is the key art (`Promo/Production/QA/exe_icon_extracted.png`). Both shortcuts use the bundled multi-size `.ico`, and the ZIP carries the same exe and `.ico`. Branding in VR is part of gate 9. |
| 12 | The packaged build runs from a short new path; the archive matches the tested build | **PASS** | Extracted to `C:\TS_zipcheck`: all 204 files are byte-identical to `Builds\Windows`. That copy ran `--autotest=demo` 33/33 with 0 errors (`zipcheck_1018_0638_demo`). |
| 13 | Title, credits, licences, AI disclosure, team, no false claims | **FIXED (docs) / in-game text in the next build** | `Docs/Credits_and_AI_Disclosure.md` was wrong in three places, now fixed. It now lists the team as the game credits it, discloses the ChatGPT-made key art, logo and icons that ship in the game, and no longer says there is no insignia (the key art shows a stylised emblem). The in-game credits line is corrected in `UI/CreditsText.cs` and ships with the next build. No sponsor, award or standalone-Quest claims were found. Whether a stylised national emblem on the key art is acceptable under the jam rules is Awaiz's call. |

## Known issues (not blockers)

These four UI overlaps are being fixed in a separate session, and will be rebuilt and retested before the final package:

- the arrival briefing covers long country titles;
- the step banner covers the open passport;
- the VR trigger callout sits behind the controller model;
- long place names wrap on the full map.

## Needs Awaiz

1. **Quest 3 session (about 15 minutes).** Link connected, headset on. The capture plan and steps are given in chat.
2. **Listening pass (about 10 minutes)** on headphones: the menu, the story opening, one country and the ending.
3. Optional: **two minutes of desktop play of the Judges' Demo**, so the uncut gameplay clip is human play rather than scripted input.

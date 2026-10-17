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

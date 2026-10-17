using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// --autotest=lifecycle: the cases the original run left out, driven through the same calls the input and menus
    /// make (menu buttons are pressed as 3D buttons; a "hold" button is held). Sudden death and retry, pause during a
    /// threat, early glasses removal, menu to demo to story, Morocco with the threat ON, wrong targets, a missing
    /// resource, a blocked verification, recalling a dropped essential item and revisiting a country without
    /// duplicates. --autotest=persist1 then --autotest=persist2 (same --save-dir) prove settings and Continue survive
    /// a restart. None of this writes the player's own save.
    /// </summary>
    public partial class AutoTest
    {
        IEnumerator Press(string label)
        {
            var b = Find<Button3D>("Button_" + label);
            Check(b != null && b.Interactive, "menu button \"" + label + "\" is shown and enabled");
            if (b != null) b.Press(b.transform.position, HandSide.Desktop);
            yield return null;
        }

        IEnumerator HoldButton(string label, float seconds)
        {
            var b = Find<Button3D>("Button_" + label);
            Check(b != null && b.Interactive, "hold button \"" + label + "\" is shown");
            if (b == null) yield break;
            b.Press(b.transform.position, HandSide.Desktop);
            yield return new WaitForSecondsRealtime(seconds);
        }

        IEnumerator LifecycleFlow()
        {
            Note("--- Lifecycle: menu -> demo -> menu -> story ---");
            if (Game.Cells.Active == null || Game.Cells.Active.Id != CellId.Menu || !Branding.I.WelcomeShowing)
            {
                Game.Story.ReturnToMenu();
                yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu && Branding.I.WelcomeShowing, 12f, "start from the welcome screen");
            }
            yield return Press("Judges' Demo (about 60 seconds)");
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Paris && !Game.Story.Busy, 20f, "the demo button starts the judges' demo in Paris");
            yield return new WaitForSeconds(1.0f);
            MenuSystem.I.TogglePause();
            yield return null;
            Check(Game.Paused && MenuSystem.I.CurrentPage == "Paused", "pause opens the pause menu during the demo");
            yield return Press("Main menu");
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu && Branding.I.WelcomeShowing && !Game.Paused, 10f, "Main menu returns to the welcome screen, unpaused");
            yield return Press(SaveSystem.HasValidCampaign() ? "New Story" : "Story Mode");
            if (MenuSystem.I.CurrentPage == "Start a new story?") yield return Press("Start new story");
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.ApartmentNormal, 20f, "the story starts at home in Abu Dhabi");
            Game.Story.ReturnToMenu();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu, 10f, "back to the menu from the story");

            Note("--- Sudden death and retry (Paris demo) ---");
            Game.Story.StartDemo();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Paris && !Game.Story.Busy, 20f, "demo re-enters Paris");
            yield return new WaitForSeconds(1.2f);
            var pc = Game.Passenger;
            Check(pc.Visible && pc.Threatening, "the Passenger is guarding and threatening");
            // It comes for the player on the open street (towards the arrival point, clear of the desk): it runs them down,
            // and on contact it lunges and has them at once (no slow drain).
            var openSide = Game.Cells.Active.Arrival.position - pc.transform.position; openSide.y = 0f;
            openSide = openSide.sqrMagnitude > 0.01f ? openSide.normalized : -pc.transform.forward;
            var stand = pc.transform.position + openSide * 3.2f;
            Game.Rig.PlaceAt(new Vector3(stand.x, pc.transform.position.y, stand.z), Quaternion.LookRotation(-openSide).eulerAngles.y);
            yield return null;
            pc.StartChase();
            float Gap() { var d = pc.transform.position - Game.Rig.Feet; d.y = 0f; return d.magnitude; }
            yield return WaitUntil(() => Gap() < pc.DrainRange, 5f, "the Passenger runs the player down");
            float caughtAt = Time.realtimeSinceStartup;
            yield return WaitUntil(() => MenuSystem.I.IsOpen && MenuSystem.I.CurrentPage == "The Margin takes hold", 4f, "reaching the player kills at once: the failure card appears");
            Check(Time.realtimeSinceStartup - caughtAt < 2.5f, "death is sudden (under 2.5 s from contact to the failure card)");
            yield return new WaitForSecondsRealtime(0.6f);
            Check(Fader.I.Alpha < 0.5f, "the failure card is not hidden behind the death fade (fade " + Fader.I.Alpha.ToString("0.00") + ")");
            yield return Shot("L1_failure_card");
            yield return Press("Retry from checkpoint");
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Paris && !Game.Story.Busy && !MenuSystem.I.IsOpen, 10f, "Retry restarts at the embankment");
            yield return new WaitForSeconds(1.0f);
            Check(Mathf.Approximately(Game.Campaign.vitality, 1f) && Game.Passenger.Visible && Game.Passenger.State == PassengerState.Guard && !Game.Campaign.IsVerified(CountryId.France),
                "after Retry: full vitality, the Passenger back at the desk, France not verified");

            Note("--- Negative cases (Paris demo) ---");
            var paris = (ParisCell)Game.Cells.Active;
            Check(!paris.Trace.CanUse(Game.Rig.MainHand) || paris.Trace.GetPrompt(null) != paris.Trace.Prompt, "the trace refuses verification while it guards the desk");
            paris.Trace.Use(Game.Rig.MainHand);
            yield return new WaitForSeconds(0.4f);
            Check(!Game.Campaign.IsVerified(CountryId.France), "a blocked verification does not succeed");
            // Wrong destination: a tear to Japan at the desk seam brings no sunlight, so the guard stays.
            Game.Passport.Open(HandSide.Desktop);
            Game.Passport.Select(CountryId.Japan, true);
            var seam = paris.DeskSeam;
            var aim = seam.Center - (paris.TearVantage.position + Vector3.up * DesktopRig.EyeHeight); // from the eye, not the feet
            Game.Rig.PlaceAt(paris.TearVantage.position, Mathf.Atan2(aim.x, aim.z) * Mathf.Rad2Deg);
            if (Game.Rig is DesktopRig r1) r1.SetPitch(-Mathf.Atan2(aim.y, new Vector2(aim.x, aim.z).magnitude) * Mathf.Rad2Deg);
            yield return null; yield return null;
            yield return Hold(dt => Game.Tears.HoldWiden(dt, HandSide.Desktop), 1.1f);
            Check(Game.Tears.Active != null && Game.Tears.Active.Destination.Id == CountryId.Japan && Game.Tears.Active.Beam == null, "a tear to the wrong country opens with no sunlight");
            yield return new WaitForSeconds(1.2f);
            Check(Game.Passenger.State == PassengerState.Guard, "without sunlight the Passenger is not driven off the desk");
            yield return Hold(dt => Game.Tears.HoldSeal(dt, HandSide.Desktop), 0.9f);
            yield return WaitUntil(() => Game.Tears.Active == null, 2f, "the wrong tear seals");
            Game.Passport.Close();

            Note("--- Pause during a threat ---");
            pc = Game.Passenger;
            var near = pc.transform.position + pc.transform.forward * 4.5f;
            Game.Rig.PlaceAt(new Vector3(near.x, pc.transform.position.y, near.z), pc.transform.eulerAngles.y + 180f);
            pc.SetState(PassengerState.Stalk);
            yield return new WaitForSeconds(0.8f);
            MenuSystem.I.TogglePause();
            yield return null;
            var frozenAt = pc.transform.position;
            yield return new WaitForSecondsRealtime(1.5f);
            Check(Game.Paused && Time.timeScale == 0f && (pc.transform.position - frozenAt).magnitude < 0.01f && !Game.Story.Failing, "paused mid-threat: the Passenger is frozen and nothing kills the player");
            yield return Press("Resume");
            yield return new WaitForSeconds(1.0f);
            Check(!Game.Paused && (pc.transform.position - frozenAt).magnitude > 0.2f, "Resume: the threat moves again");
            Game.Story.ReturnToMenu();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu, 10f, "back to the menu");

            Note("--- Early glasses removal (story, Egypt) ---");
            yield return Enter(CellId.Egypt);
            var returnsBefore = Game.Campaign.forcedReturns;
            bool egyptBefore = Game.Campaign.IsVerified(CountryId.Egypt);
            MenuSystem.I.TogglePause();
            yield return null;
            yield return HoldButton("Remove the glasses (hold)", 2.3f);
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.ApartmentCorrupt && !Game.Story.Busy, 12f, "holding Remove the glasses returns the player to the apartment");
            Check(Game.Campaign.forcedReturns == returnsBefore + 1 && Game.Campaign.IsVerified(CountryId.Egypt) == egyptBefore && !Game.Paused,
                "the removal is counted, seals are kept and the game is unpaused");

            Note("--- Recall a dropped essential item (New York) ---");
            yield return Enter(CellId.NewYork);
            var ny = (NewYorkCell)Game.Cells.Active;
            var hand = Game.Rig.MainHand;
            hand.Grab(ny.Noisemaker);
            yield return null;
            hand.Drop();
            Game.Rig.PlaceAt(ny.Arrival.position, ny.Arrival.eulerAngles.y);
            yield return new WaitForSeconds(0.3f);
            Game.Story.RecallEssential(hand);
            yield return null;
            Check(hand.Held == ny.Noisemaker, "Recall brings the dropped noisemaker back to the hand");
            hand.Drop();

            Note("--- Revisit without duplicates (Paris -> Egypt -> Paris) ---");
            yield return Enter(CellId.Paris);
            yield return new WaitForSeconds(0.8f);
            yield return Enter(CellId.Egypt);
            yield return new WaitForSeconds(0.5f);
            yield return Enter(CellId.Paris);
            yield return new WaitForSeconds(1.0f);
            int passengers = FindObjectsByType<PassengerController>(FindObjectsSortMode.None).Length;
            int hands = FindObjectsByType<PassengerHand>(FindObjectsSortMode.None).Count(h => h.Active);
            Check(passengers == 1 && hands == 0 && Game.Passenger.Visible && Game.Passenger.State == PassengerState.Guard, "revisiting Paris: one Passenger, guarding the desk, no stray hands");

            Note("--- Morocco with the threat ON (the counter lamp keeps it back) ---");
            yield return Enter(CellId.ShopCorrupt);
            var shop = (ShopCell)Game.Cells.Active;
            Game.Rig.PlaceAt(shop.CounterTop.position + new Vector3(0, -1.04f, -1.1f), 0f);
            yield return new WaitForSeconds(0.6f);
            var mp = shop.transform.Find("MoroccoProps");
            Check(Game.Passenger.Visible && Game.Passenger.Threatening, "Morocco: the observer is present and threatening");
            var first = mp != null ? mp.GetComponentsInChildren<Grabbable>(true).FirstOrDefault(g => g.ItemId == "postcard") : null;
            if (first != null) { hand.Grab(first); yield return null; hand.PlaceInto(shop.CounterMarks[0].GetComponent<Socket>()); }
            Check(Game.Passenger.State == PassengerState.Stalk || Game.Passenger.State == PassengerState.Chase || Game.Passenger.State == PassengerState.Repelled || Game.Passenger.State == PassengerState.Retreat, "taking an item sets it stalking");
            float survive = 0f;
            while (survive < 14f && !Game.Story.Failing) { survive += Time.deltaTime; yield return null; }
            Check(!Game.Story.Failing, "standing in the counter lamp's light, the player is not caught (14 s with the threat on)");
            Note("Morocco light check: ambient " + RenderSettings.ambientSkyColor + ", lamp " + (shop.Lamp != null ? shop.Lamp.intensity.ToString("0.00") : "none") +
                 ", fade " + Fader.I.Alpha.ToString("0.00") + ", brightness " + Game.Settings.brightness.ToString("0.00"));
            yield return Shot("L2_morocco_threat_on");

            Note("--- Verify, run for the stamp, shift into the next country (Japan -> Saudi Arabia) ---");
            yield return Enter(CellId.Japan, c => { c.Verify(CountryId.Egypt); c.Verify(CountryId.France); c.Verify(CountryId.USA); });
            var japan = (JapanCell)Game.Cells.Active;
            Game.Story.VerifyCountry(CountryId.Japan, japan.Trace);
            yield return WaitUntil(() => Game.Story.WaitingStamp != null, 8f, "a verified seal puts the country's stamp out to collect");
            var stamp = Game.Story.WaitingStamp;
            if (stamp != null)
            {
                var toStamp = stamp.transform.position - Game.Rig.Feet; toStamp.y = 0f;
                Check(toStamp.magnitude > 3f, "the stamp waits away from the player, so they have to run for it (" + toStamp.magnitude.ToString("0.0") + " m)");
                var look = stamp.transform.position - Game.Rig.Feet; look.y = 0f;
                if (look.sqrMagnitude > 0.01f) Game.Rig.PlaceAt(Game.Rig.Feet, Quaternion.LookRotation(look).eulerAngles.y);
                yield return new WaitForSeconds(0.5f);
                yield return Shot("L3_stamp_waiting");
            }
            yield return WaitUntil(() => Game.Passenger.Visible && Game.Passenger.Chasing, 7f, "the Passenger comes running once the stamp is out");
            yield return new WaitForSeconds(0.6f);
            yield return Shot("L4_chase");
            if (stamp != null) stamp.Collect();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Riyadh && !Game.Story.Busy, 20f, "collecting the stamp shifts the player into the next country (Saudi Arabia)");
            Check(Game.Campaign.IsVisited(CountryId.SaudiArabia) && Game.Cells.Active.Encounter != null && Game.Cells.Active.Encounter.Entered && Game.Story.WaitingStamp == null,
                "after the shift the next country starts properly and no stamp is left waiting");
            Game.Story.ReturnToMenu();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu, 10f, "lifecycle run ends at the menu");
        }

        // ---------------------------------------------------------------- persistence across a restart
        IEnumerator Persist1Flow()
        {
            Note("--- Persist 1: change settings and save a mid-story campaign into the sandbox ---");
            Check(!string.IsNullOrEmpty(AppArgs.SaveDir) && !SaveSystem.WritesSuspended, "running with a save sandbox (" + AppArgs.SaveDir + ")");
            var s = Game.Settings;
            s.brightness = 0.37f; s.subtitleSize = 2; s.scareIntensity = 0; s.master = 0.61f;
            Game.ApplySettings();
            var c = CampaignState.NewGame();
            foreach (var f in new[] { StoryFlags.PrologueDone, StoryFlags.ShopPurchased, StoryFlags.HomeArrived, StoryFlags.Unpacked, StoryFlags.GlassesWorn, StoryFlags.SeamRevealed, StoryFlags.TutorialDone }) c.SetFlag(f);
            c.Verify(CountryId.UAE);
            c.Verify(CountryId.Egypt);
            c.AddResource(ResourceId.Sunlight);
            c.CheckpointCell = CellId.Paris;
            Check(SaveSystem.SaveCampaign(c), "campaign written");
            Check(File.Exists(SaveSystem.PathOf(SaveSystem.SettingsFile)) && File.Exists(SaveSystem.PathOf(SaveSystem.CampaignFile)), "settings and campaign files exist in the sandbox");
            // Then play the judges' demo, go back to the menu and quit from there (the process ends after this flow): neither
            // the demo's throwaway run nor quitting from the menu may overwrite the story save. Persist 2 checks it.
            Game.Story.StartDemo();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Paris && !Game.Story.Busy, 20f, "the demo plays in the same session");
            yield return new WaitForSeconds(1f);
            Game.Story.ReturnToMenu();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu, 10f, "back to the menu, then quit");
        }

        IEnumerator Persist2Flow()
        {
            Note("--- Persist 2: a fresh process reads them back; Continue resumes in Paris ---");
            var s = Game.Settings;
            Check(Mathf.Abs(s.brightness - 0.37f) < 0.001f && s.subtitleSize == 2 && s.scareIntensity == 0 && Mathf.Abs(s.master - 0.61f) < 0.001f, "settings survived the restart");
            Check(SaveSystem.HasValidCampaign(), "the saved story is offered as Continue");
            yield return Press("Continue");
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Paris && !Game.Story.Busy, 20f, "Continue resumes at the saved checkpoint (Paris)");
            Check(Game.Campaign.IsVerified(CountryId.Egypt) && Game.Campaign.HasResource(ResourceId.Sunlight), "verified seals and resources came back with the save");
            Game.Story.ReturnToMenu();
            yield return WaitUntil(() => Game.Cells.Active != null && Game.Cells.Active.Id == CellId.Menu, 10f, "back to the menu");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Campaign flow: menu → shop prologue → homecoming → corrupted apartment → countries → Berlin finale → real home
    /// → bathroom/epilogue → results → credits → menu. Also the judges' demo. Owns vitality, checkpoints, failure
    /// and retry, early glasses removal, recall and travel between cells.
    /// </summary>
    public class StoryDirector : MonoBehaviour
    {
        public static StoryDirector I { get; private set; }

        public bool TearsAllowed = true;
        public bool ThreatAllowed = true;
        public bool ShowVitality = true;
        public float ContaminationScale = 1f;
        public bool InCampaign { get; private set; }
        public bool Busy { get; private set; }

        readonly List<Grabbable> m_Essentials = new List<Grabbable>();
        float m_DrainFeedback;
        bool m_Failing;

        public static StoryDirector Create(Transform parent)
        {
            var go = new GameObject("Story");
            go.transform.SetParent(parent, false);
            I = go.AddComponent<StoryDirector>();
            return I;
        }

        void Update()
        {
            if (InCampaign && !Game.Paused && Game.Campaign != null) Game.Campaign.playTimeSeconds += Time.deltaTime;
            // Gentle recovery when nothing is draining (breathing room).
            if (InCampaign && !Game.Paused && m_DrainFeedback <= 0.01f && Game.Campaign != null && Game.Campaign.vitality < 1f && Game.Campaign.vitality > 0f)
                Game.Campaign.vitality = Mathf.Min(1f, Game.Campaign.vitality + Time.deltaTime * 0.012f);
            PostFx.I?.SetDrain(Game.Campaign != null ? Mathf.Max(1f - Game.Campaign.vitality, m_DrainFeedback * 0.5f) : 0f);
        }

        // ------------------------------------------------------------------ entry points
        public void StartNewGame()
        {
            Game.IsDemo = false;
            SaveSystem.WritesSuspended = Game.IsAutoTest;
            Game.Campaign = CampaignState.NewGame();
            Game.SaveNow();
            StartCoroutine(RunCampaign());
        }

        public void ContinueGame()
        {
            var c = SaveSystem.LoadCampaign();
            if (c == null) { Hud.I?.ShowToast("No valid save found.", 2.5f); return; }
            Game.IsDemo = false;
            Game.Campaign = c;
            StartCoroutine(RunCampaign());
        }

        /// <summary>Developer/test start in a given cell (no save writes).</summary>
        public void StartAt(CellId cell, System.Action<CampaignState> configure = null)
        {
            Game.IsDemo = false;
            SaveSystem.WritesSuspended = true;
            var c = CampaignState.NewGame();
            foreach (var f in new[] { StoryFlags.PrologueDone, StoryFlags.ShopPurchased, StoryFlags.HomeArrived, StoryFlags.Unpacked, StoryFlags.GlassesWorn, StoryFlags.SeamRevealed, StoryFlags.TutorialDone }) c.SetFlag(f);
            c.Verify(CountryId.UAE);
            c.CheckpointCell = cell;
            configure?.Invoke(c);
            Game.Campaign = c;
            StartCoroutine(RunCampaign());
        }

        public void StartDemo()
        {
            Game.IsDemo = true;
            SaveSystem.WritesSuspended = true; // the demo never touches the campaign save
            var c = CampaignState.NewGame();
            foreach (var f in new[] { StoryFlags.PrologueDone, StoryFlags.ShopPurchased, StoryFlags.HomeArrived, StoryFlags.Unpacked, StoryFlags.GlassesWorn, StoryFlags.SeamRevealed, StoryFlags.TutorialDone, StoryFlags.FirstTearDone }) c.SetFlag(f);
            c.Verify(CountryId.UAE);
            c.Verify(CountryId.Egypt); // gives Egyptian sunlight
            c.CheckpointCell = CellId.Paris;
            Game.Campaign = c;
            StartCoroutine(DemoRoutine());
        }

        /// <summary>Chapters that switch these off (prologue, homecoming, ending) must never leak into the next run.</summary>
        void ResetChapterGates() { TearsAllowed = true; ThreatAllowed = true; ShowVitality = true; }

        IEnumerator RunCampaign()
        {
            InCampaign = true;
            ResetChapterGates();
            MenuSystem.I?.HideAll();
            var c = Game.Campaign;
            if (!c.HasFlag(StoryFlags.PrologueDone) && CellManager.Factory.ContainsKey(CellId.ShopNormal))
            {
                yield return Prologue.Run();
                if (!InCampaign) yield break;
            }
            c.SetFlag(StoryFlags.PrologueDone);
            if (!c.HasFlag(StoryFlags.GlassesWorn) && CellManager.Factory.ContainsKey(CellId.ApartmentNormal))
            {
                yield return Homecoming.Run();
                if (!InCampaign) yield break;
            }
            c.SetFlag(StoryFlags.GlassesWorn);
            var cell = c.CheckpointCell;
            if (cell == CellId.ShopNormal || cell == CellId.ApartmentNormal || cell == CellId.Menu || !CellManager.Factory.ContainsKey(cell))
                cell = CellManager.Factory.ContainsKey(CellId.ApartmentCorrupt) ? CellId.ApartmentCorrupt : CellId.Egypt;
            if (!c.HasFlag(StoryFlags.SeamRevealed) && cell != CellId.ApartmentCorrupt)
            {
                c.SetFlag(StoryFlags.SeamRevealed);
                c.SetFlag(StoryFlags.TutorialDone);
            }
            // The homecoming hands over to the Margin copy in place; don't re-enter (no fade, no teleport).
            if (Game.Cells.Active != null && Game.Cells.Active.Id == cell && Game.Cells.Active.Encounter != null && Game.Cells.Active.Encounter.Entered)
                yield break;
            yield return EnterCell(cell, false);
        }

        public void ReturnToMenu()
        {
            StopAllCoroutines();
            InCampaign = false;
            Busy = false;
            m_Failing = false;
            Game.InputLocks = 0;
            Game.Paused = false;
            Game.Passenger?.Despawn();
            Game.Tears?.ForceReset();
            Narration.I?.StopAll();
            Game.Passport?.Close();
            Game.SaveNow();
            Game.IsDemo = false;
            ResetChapterGates();
            SaveSystem.WritesSuspended = Game.IsAutoTest;
            StartCoroutine(MenuSystem.I.ShowMainMenuRoutine());
        }

        // ------------------------------------------------------------------ travel
        public void TravelThroughTear(CountryDef dest)
        {
            if (Busy) return;
            if (dest == CountryDatabase.Home) { StartCoroutine(EndingChapter.Run()); return; }
            StartCoroutine(EnterCell(dest.Cell, true));
        }

        public IEnumerator EnterCell(CellId id, bool viaTear, string titleOverride = null)
        {
            Busy = true;
            Game.InputLocks++;
            if (viaTear) AudioDirector.I?.Play("tear_open_door", null, 0.6f);
            yield return Fader.I.FadeOut(viaTear ? 0.25f : 0.45f);
            Game.Passenger?.Despawn();
            Narration.I?.ClearTutorial();
            foreach (var h in Game.Rig.Hands) if (h.Held != null && !h.Held.Essential) h.ForceRelease(true);
            var cell = Game.Cells.Activate(id);
            if (cell == null) { Game.InputLocks--; Busy = false; yield return Fader.I.FadeIn(0.3f); yield break; }
            Game.Rig.PlaceAt(cell.Arrival.position, cell.Arrival.eulerAngles.y);
            var country = CountryDatabase.ByCell(id);
            bool first = country != null && !Game.Campaign.IsVisited(country.Id);
            if (country != null) Game.Campaign.MarkVisited(country.Id);
            Game.SaveCheckpoint(id);
            Game.Passport?.Refresh();
            UpdateObjective();
            yield return null;
            cell.RefreshProbe();
            yield return Fader.I.FadeIn(0.55f);
            if (country != null && first)
                Hud.I?.ShowTitleCard(country.DisplayName.ToUpperInvariant(), titleOverride ?? country.EncounterTitle, 4f);
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
            Busy = false;
            if (cell.Encounter != null)
            {
                cell.Encounter.Entered = true;
                cell.Encounter.OnEnter(first);
            }
        }

        public void UpdateObjective()
        {
            var c = Game.Campaign;
            var cell = Game.Cells?.Active;
            var here = cell != null ? CountryDatabase.ByCell(cell.Id) : null;
            string text;
            if (here != null && !c.IsVerified(here.Id)) text = cell.Encounter != null ? cell.Encounter.Objective : here.Objective;
            else
            {
                var next = c.NextRecommended();
                text = next == null ? "Find the real way home." : "Open your passport (" + Game.Input.Label(Intent.Passport) + "), choose " + next.DisplayName + ", and tear a way there.";
            }
            Hud.I?.SetObjective(text, false);
        }

        // ------------------------------------------------------------------ verification
        public void VerifyCountry(CountryId id, TraceMark trace)
        {
            StartCoroutine(VerifyRoutine(id));
        }

        IEnumerator VerifyRoutine(CountryId id)
        {
            var c = Game.Campaign;
            bool newly = c.Verify(id);
            RestoreVitality(0.3f);
            Narration.I?.ClearTutorial();
            yield return Game.Passport.PlayVerify(id);
            var def = CountryDatabase.Get(id);
            if (newly)
            {
                Hud.I?.ShowToast("Seal verified: " + def.DisplayName + "   (" + c.VerifiedCount + " / 11)", 3.5f);
                Game.Cells.Active?.Encounter?.SendMessage("OnVerified", SendMessageOptions.DontRequireReceiver);
            }
            Game.SaveNow();
            if (!c.HasFlag(StoryFlags.CounterfeitRevealed) && c.ShouldRevealCounterfeit)
                yield return CounterfeitReveal();
            UpdateObjective();
            if (Game.IsDemo && id == CountryId.France) yield return DemoEnd();
        }

        IEnumerator CounterfeitReveal()
        {
            AudioDirector.I?.SetMusic("mus_reveal", 0.5f, 0.9f, false);
            yield return Game.Passport.PlayCounterfeitReveal();
            Narration.I?.Say("disc_01", null, false, null, true);
            yield return new WaitForSeconds(3.5f);
            Game.SaveNow();
        }

        // ------------------------------------------------------------------ vitality / failure
        public void DrainVitality(float amount)
        {
            if (!InCampaign && !Game.IsDemo) return;
            var c = Game.Campaign;
            if (c == null || m_Failing || amount <= 0f) return;
            c.vitality = Mathf.Max(0f, c.vitality - amount);
            if (c.vitality <= 0f) StartCoroutine(FailRoutine());
        }

        public void RestoreVitality(float amount)
        {
            if (Game.Campaign == null) return;
            Game.Campaign.vitality = Mathf.Min(1f, Game.Campaign.vitality + amount);
        }

        public void SetDrainFeedback(float f) => m_DrainFeedback = Mathf.Clamp01(f);

        IEnumerator FailRoutine()
        {
            m_Failing = true;
            var c = Game.Campaign;
            c.failures++;
            Game.InputLocks++;
            if (Game.Passenger != null) Game.Passenger.Threatening = false;
            Narration.I?.Say("fail_01", null, false, null, true);
            Fader.I.SetColor(new Color(0.08f, 0.02f, 0.02f));
            yield return Fader.I.FadeOut(1.2f);
            Game.Tears?.ForceReset();
            Game.Passenger?.Despawn();
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
            m_Failing = false;
            MenuSystem.I?.ShowFailure();
        }

        /// <summary>Retry from the last checkpoint: verified seals and resources stay, the encounter resets.</summary>
        public void RetryCheckpoint()
        {
            var c = Game.Campaign;
            c.vitality = 1f;
            Fader.I.SetColor(Color.black);
            MenuSystem.I?.HideAll();
            Game.Cells.Active?.Encounter?.ResetLocal();
            StartCoroutine(EnterCell(c.CheckpointCell, false));
        }

        // ------------------------------------------------------------------ glasses (early removal)
        public bool CanRemoveGlassesEarly => InCampaign && Game.Campaign != null && Game.Campaign.HasFlag(StoryFlags.GlassesWorn) && !Game.Campaign.HasFlag(StoryFlags.Escaped) && !Busy;

        public void RequestEarlyGlassesRemoval()
        {
            if (!CanRemoveGlassesEarly) return;
            StartCoroutine(RejectionRoutine());
        }

        IEnumerator RejectionRoutine()
        {
            var c = Game.Campaign;
            c.forcedReturns++;
            Game.InputLocks++;
            Narration.I?.Say("tut_return", null, true, null, true, true);
            AudioDirector.I?.Play("reflection_sting", null, 0.8f);
            PostFx.I?.Flash(0.8f);
            yield return Fader.I.FadeOut(0.8f);
            Game.Tears?.ForceReset();
            Game.Passenger?.Despawn();
            Game.Cells.Active?.Encounter?.ResetLocal();
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
            var target = CellManager.Factory.ContainsKey(CellId.ApartmentCorrupt) ? CellId.ApartmentCorrupt : c.CheckpointCell;
            yield return EnterCell(target, false);
            Narration.I?.Say("reject_01", null, false, null, false);
        }

        // ------------------------------------------------------------------ essentials / recall
        public void RegisterEssential(Grabbable g)
        {
            if (g != null && !m_Essentials.Contains(g)) m_Essentials.Add(g);
        }

        public void RecallEssential(Interactor hand)
        {
            Grabbable best = null;
            for (int i = m_Essentials.Count - 1; i >= 0; i--)
            {
                var g = m_Essentials[i];
                if (g == null) { m_Essentials.RemoveAt(i); continue; }
                if (g.Holder != null) continue;
                if (!g.gameObject.activeInHierarchy && g.transform.root != Game.Root) continue;
                best = g;
                break;
            }
            if (best == null) { Hud.I?.ShowToast("Nothing to recall.", 1.5f); return; }
            best.gameObject.SetActive(true);
            hand.Grab(best);
            AudioDirector.I?.Play("pp_seam_shimmer", hand.HoldPoint.position, 0.5f);
            Hud.I?.ShowToast("Recalled: " + best.ItemId.Replace('_', ' '), 1.5f);
        }

        // ------------------------------------------------------------------ passport hooks
        public void OnPassportOpened()
        {
            Tutorial.I?.Notify(TutorialStep.OpenPassport);
        }

        public void OnDestinationSelected(CountryId id)
        {
            Tutorial.I?.Notify(TutorialStep.ChooseDestination);
        }

        // ------------------------------------------------------------------ judges' demo
        IEnumerator DemoRoutine()
        {
            InCampaign = true;
            ResetChapterGates();
            MenuSystem.I?.HideAll();
            DemoTimer.Begin();
            yield return EnterCell(CellId.Paris, false, "Judges' demo");
            Hud.I?.SetObjective("Open your passport (" + Game.Input.Label(Intent.Passport) + "), choose Egypt, and open a tear at the seam facing the desk.", false);
            Hud.I?.ShowToast("Open your passport: " + Game.Input.Label(Intent.Passport), 4f);
        }

        IEnumerator DemoEnd()
        {
            yield return new WaitForSeconds(2.5f);
            float seconds = DemoTimer.Elapsed;
            Hud.I?.ShowTitleCard("THE TWELFTH STAMP", "You travelled alone. Something came back with you.", 5f);
            yield return new WaitForSeconds(5f);
            Game.Passenger?.Despawn();
            MenuSystem.I?.ShowDemoResults(seconds);
        }
    }

    /// <summary>Simple real-time measurement of the demo run (no faked numbers).</summary>
    public static class DemoTimer
    {
        static float s_Start;
        public static void Begin() => s_Start = Time.realtimeSinceStartup;
        public static float Elapsed => Time.realtimeSinceStartup - s_Start;
    }
}

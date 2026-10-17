using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// The story opens at home in Abu Dhabi. Mum's voicemail plays; going through the bag turns up the glasses case,
    /// which brings back the Marrakech corner shop (a flashback, see PrologueChapter); back in the present the only
    /// thing left to do is put the glasses on, and the room quietly stops being the room: the fridge hum stops, a
    /// recorded word repeats, the reflection becomes wrong. Not a jump scare; the Margin arrives by subtraction.
    /// </summary>
    public static class HomecomingChapter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() => Homecoming.Routine = Run;

        static bool s_Worn;

        public static IEnumerator Run()
        {
            var story = StoryDirector.I;
            var c = Game.Campaign;
            story.TearsAllowed = false;
            story.ThreatAllowed = false;
            story.ShowVitality = false;
            Game.Passport?.Close();
            Game.Passport.Available = false; // the passport matters once the Margin begins
            s_Worn = false;

            yield return story.EnterCell(CellId.ApartmentNormal, false, "Home");
            var apt = (ApartmentCell)Game.Cells.Active;
            // Start beside the open bag, facing it.
            var bagFront = apt.Suitcase.position + new Vector3(0.05f, 0f, -1.3f);
            Game.Rig.PlaceAt(new Vector3(bagFront.x, apt.transform.position.y, bagFront.z), Quaternion.LookRotation(apt.Suitcase.position - bagFront).eulerAngles.y);
            if (Game.Rig is DesktopRig desk) desk.SetPitch(45f);
            Hud.I?.ShowTitleCard("ABU DHABI", "Home", 3.5f);
            Narration.I?.Say("home_01", null, false, null, false, true);
            c.SetFlag(StoryFlags.HomeArrived);

            // What is still in the bag: the camera, the passport and the glasses case from Marrakech.
            var props = new GameObject("HomecomingProps").transform;
            props.SetParent(apt.transform, false);
            var cam = PropLibrary.VintageCamera(props).transform;
            cam.position = apt.Suitcase.position + new Vector3(-0.18f, 0.22f, 0.02f);
            var pass = PropLibrary.PassportClosed(props).transform;
            pass.position = apt.Suitcase.position + new Vector3(0.05f, 0.21f, -0.05f);
            var caseGo = PropLibrary.GlassesCase(props, false);
            caseGo.transform.position = apt.Suitcase.position + new Vector3(0.22f, 0.21f, 0.05f);
            var hall = new GameObject("HallLight").AddComponent<Light>();
            hall.transform.SetParent(props, false);
            hall.transform.position = apt.Suitcase.position + new Vector3(0f, 1.7f, -0.35f);
            hall.type = LightType.Point; hall.range = 3f; hall.intensity = 1.4f; hall.color = new Color(1f, 0.82f, 0.62f);
            hall.shadows = LightShadows.Soft;
            apt.Recache();

            // Mum's voicemail plays by itself (the Margin will replay it wrong later).
            yield return new WaitForSeconds(2.5f);
            AudioDirector.I?.Play("phone_buzz", apt.PhoneRoot.position, 0.9f);
            Hud.I?.ShowCaption("your phone buzzes on the table", 2.5f);
            apt.PhoneScreen.SetColor("_Color", new Color(0.55f, 0.75f, 0.85f, 1f));
            apt.PhoneScreen.SetFloat("_Intensity", 1.4f);
            yield return new WaitForSeconds(1.4f);
            AudioDirector.I?.Play("voicemail_beep", apt.PhoneRoot.position, 0.8f);
            bool heard = false;
            Narration.I?.Say("mom_01", apt.PhoneRoot, false, () => heard = true, true);
            for (float t = 0f; !heard && t < 9f; t += Time.deltaTime) yield return null;

            // The one thing to do: go through the bag.
            var bagUse = apt.Suitcase.gameObject.AddComponent<Interactable>();
            bagUse.Prompt = "Go through your bag";
            bool opened = false;
            bagUse.OnUse = _ => opened = true;
            Hud.I?.SetObjective("Go through your bag.", false);
            while (!opened) yield return null;
            UnityEngine.Object.Destroy(bagUse);
            Hud.I?.SetObjective("", false);

            // Camera and passport come out and are set aside; then the glasses case, into the light.
            AudioDirector.I?.Play("grab", apt.Suitcase.position + Vector3.up * 0.3f, 0.8f);
            yield return Lift(cam, apt.Suitcase.position + new Vector3(-0.62f, 0.02f, -0.15f), Quaternion.Euler(0f, 35f, 0f), 0.6f);
            yield return Lift(pass, apt.Suitcase.position + new Vector3(-0.5f, 0.01f, -0.42f), Quaternion.Euler(0f, -20f, 0f), 0.6f);
            var caseSpot = apt.Suitcase.position + new Vector3(0.05f, 0.24f, -0.22f);
            yield return Lift(caseGo.transform, caseSpot, Quaternion.Euler(0f, 180f, 0f), 0.8f);
            c.SetFlag(StoryFlags.Unpacked);
            Game.SaveNow();
            yield return new WaitForSeconds(0.8f);

            // ...and he remembers where it came from.
            if (!c.HasFlag(StoryFlags.PrologueDone)) yield return PrologueChapter.Flashback();
            // Present: he holds the case up in front of him and opens it. The only thing left is to put them on.
            var head = Game.Rig.Head;
            var fwd = head.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            var held = head.position + fwd * 0.55f + Vector3.down * 0.2f;
            yield return Lift(caseGo.transform, held, Quaternion.LookRotation(fwd, Vector3.up), 0.7f);
            if (Game.Rig is DesktopRig desk2) desk2.SetPitch(Mathf.Atan2(0.2f, 0.55f) * Mathf.Rad2Deg);
            var lamp = new GameObject("CaseLight").AddComponent<Light>();
            lamp.transform.SetParent(props, false);
            lamp.transform.position = head.position + fwd * 0.25f + Vector3.up * 0.15f;
            lamp.type = LightType.Point; lamp.range = 1.1f; lamp.intensity = 0.14f; lamp.color = new Color(1f, 0.9f, 0.78f);
            lamp.shadows = LightShadows.None;
            yield return new WaitForSeconds(0.4f);
            AudioDirector.I?.Play("case_open", caseGo.transform.position, 0.8f);
            PropLibrary.SetCaseOpen(caseGo, true);
            var glassesGo = PropLibrary.Glasses(props, true);
            glassesGo.transform.position = caseGo.transform.position + Vector3.up * 0.03f;
            glassesGo.transform.rotation = caseGo.transform.rotation * Quaternion.Euler(-90, 0, 0);
            var col = glassesGo.AddComponent<BoxCollider>();
            col.size = new Vector3(0.2f, 0.1f, 0.12f); // the whole open case is the target
            var wear = glassesGo.AddComponent<Interactable>();
            wear.Prompt = "Put on the glasses";
            wear.OnUse = _ => s_Worn = true;
            apt.Recache();
            AudioDirector.I?.Play("glasses_unfold", glassesGo.transform.position, 0.7f);
            Narration.I?.Say("home_02", null, false, null, false, true);
            Hud.I?.SetObjective("Put on the glasses.", false);
            while (!s_Worn) yield return null;
            // He sets the empty case back on the bag as the room starts to change.
            UnityEngine.Object.Destroy(lamp.gameObject);
            caseGo.transform.position = caseSpot;
            caseGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            PropLibrary.SetCaseOpen(caseGo, false);
            yield return WearAndCorrupt(apt, glassesGo);
        }

        /// <summary>Moves a prop in a short arc (unpacking happens on its own; the player only watches).</summary>
        static IEnumerator Lift(Transform t, Vector3 to, Quaternion rot, float seconds)
        {
            var from = t.position; var r0 = t.rotation;
            for (float k = 0f; k < 1f; )
            {
                k = Mathf.Min(1f, k + Time.deltaTime / seconds);
                float e = Mathf.SmoothStep(0f, 1f, k);
                t.position = Vector3.Lerp(from, to, e) + Vector3.up * Mathf.Sin(e * Mathf.PI) * 0.12f;
                t.rotation = Quaternion.Slerp(r0, rot, e);
                yield return null;
            }
        }

        static IEnumerator WearAndCorrupt(ApartmentCell apt, GameObject glasses)
        {
            var c = Game.Campaign;
            glasses.SetActive(false);
            AudioDirector.I?.Play("glasses_unfold", Game.Rig.Head.position, 0.6f);
            Fader.I.FadeTo(0.55f, 0.35f);
            yield return new WaitForSeconds(0.45f);
            Fader.I.FadeTo(0f, 0.8f);
            c.SetFlag(StoryFlags.GlassesWorn);
            Game.SaveNow();
            Hud.I?.SetObjective("", false);

            // Subtraction: the fridge hum stops.
            AudioDirector.I?.Play("fridge_stop", apt.FridgeRoot.position, 0.9f);
            AudioDirector.I?.SetAmbience("amb_margin", 4f, 0.5f);
            Hud.I?.ShowCaption("the fridge stops humming", 3f);
            yield return new WaitForSeconds(3.5f);
            // A recorded word repeats.
            AudioDirector.I?.Play("mom_01_glitch", apt.PhoneRoot.position, 0.9f, 1f, Bus.Voice);
            Hud.I?.ShowCaption("the voicemail repeats: \"unpacked... unpacked...\"", 4f);
            yield return new WaitForSeconds(4.5f);
            // The reflection becomes wrong: a corridor that exists only in the glass.
            AudioDirector.I?.Play("reflection_sting", null, 0.7f, 1f, Bus.SFX, true);
            float t = 0f;
            while (t < 1f) { t += Time.deltaTime / 3f; apt.ReflectionMat.SetColor("_Color", new Color(0.75f, 0.75f, 0.7f, t * 0.16f)); yield return null; }
            Narration.I?.Say("home_03");
            yield return new WaitForSeconds(3f);
            AudioDirector.I?.Play("skyline_shift", null, 0.8f);
            yield return new WaitForSeconds(1.5f);

            // The copy takes over (same layout, same coordinates: the player is not moved).
            Fader.I.FadeTo(0.85f, 0.5f);
            yield return new WaitForSeconds(0.55f);
            var corrupt = Game.Cells.Activate(CellId.ApartmentCorrupt);
            corrupt.RefreshProbe();
            Fader.I.FadeTo(0f, 1.2f);
            c.SetFlag(StoryFlags.SeamRevealed);
            Game.Passport.Available = true;
            c.MarkVisited(CountryId.UAE); // UAE is verified later, by the encounter
            Game.Passport.Refresh();
            Game.SaveCheckpoint(CellId.ApartmentCorrupt);
            StoryDirector.I.TearsAllowed = true;
            StoryDirector.I.ThreatAllowed = true;
            StoryDirector.I.ShowVitality = true;
            if (corrupt.Encounter != null) { corrupt.Encounter.Entered = true; corrupt.Encounter.OnEnter(true); }
        }
    }

    /// <summary>UAE — The Apartment That Kept You. Tutorial: one tear, the genuine detail, the first threat.</summary>
    public class UaeEncounter : Encounter
    {
        ApartmentCell A => (ApartmentCell)Cell;
        bool m_TraceRevealed, m_ThreatStarted, m_Escaping;
        float m_ReflectionAlpha;

        public override string Objective => Game.Campaign.IsVerified(CountryId.UAE)
            ? "Open a tear to Egypt, widen it to a doorway and step through."
            : "Open your passport and look through the seam. Find the one thing in this room the copy got right.";

        public override void OnEnter(bool firstVisit)
        {
            var c = Game.Campaign;
            m_TraceRevealed = c.IsVerified(CountryId.UAE);
            m_ThreatStarted = false;
            m_Escaping = false;
            A.ReflectionMat.SetColor("_Color", new Color(0.75f, 0.75f, 0.7f, 0.16f));
            A.ReflectionFigureMat.SetColor("_Color", new Color(0.03f, 0.035f, 0.04f, 0f));
            if (m_TraceRevealed) A.Trace.Reveal(false); else A.Trace.Hide();
            Game.Tears.Opened -= OnOpened; Game.Tears.Opened += OnOpened;
            Game.Tears.Sealed -= OnSealed; Game.Tears.Sealed += OnSealed;
            Game.Passenger?.Despawn();

            if (!c.HasFlag(StoryFlags.TutorialDone))
            {
                Narration.I?.Say("home_04", null, false, null, false, true);
                if (!c.IsVerified(CountryId.UAE))
                {
                    Tutorial.I?.Begin(TutorialStep.OpenPassport, TutorialStep.ChooseDestination, TutorialStep.OpenTear, TutorialStep.Seal);
                    Narration.I?.Say("tut_passport", null, true);
                }
                else BeginEscape();
            }
            else Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit()
        {
            Game.Tears.Opened -= OnOpened;
            Game.Tears.Sealed -= OnSealed;
            if (m_Escaping && Game.Campaign.IsVerified(CountryId.UAE))
            {
                Game.Campaign.SetFlag(StoryFlags.TutorialDone);
                Tutorial.I?.End();
            }
        }

        void OnOpened(Tear t)
        {
            if (!IsLive) return;
            Game.Campaign.SetFlag(StoryFlags.FirstTearDone);
            if (!m_TraceRevealed)
            {
                // Light from a real remembered place shows what the copy could not forge: Mum's handwriting.
                m_TraceRevealed = true;
                A.Trace.Reveal();
                Narration.I?.Say("tut_verify", null, true);
                Hud.I?.ShowToast("Warm light falls across the fridge. One note has real handwriting.", 4f);
            }
        }

        void OnSealed(Tear t)
        {
            if (!IsLive) return;
            if (Game.Campaign.IsVerified(CountryId.UAE) && !m_ThreatStarted) StartThreat();
        }

        public void OnVerified()
        {
            if (Game.Tears.Active != null) Narration.I?.Say("tut_seal", null, true);
            else StartThreat();
        }

        void StartThreat()
        {
            if (m_ThreatStarted) return;
            m_ThreatStarted = true;
            StartCoroutine(ThreatRoutine());
        }

        System.Collections.IEnumerator ThreatRoutine()
        {
            // Early escalation: first only in the reflection, then in the room, slowly.
            AudioDirector.I?.Play("reflection_sting", null, 0.8f, 1f, Bus.SFX, true);
            float t = 0f;
            while (t < 1f) { t += Time.deltaTime / 2.5f; A.ReflectionFigureMat.SetColor("_Color", new Color(0.03f, 0.035f, 0.04f, t * 0.55f)); yield return null; }
            yield return new WaitForSeconds(2.5f);
            var pc = Game.Passenger;
            pc.RetreatPoint = A.transform.TransformPoint(new Vector3(2.75f, 0f, -6.2f));
            pc.RetreatSeconds = 14f;
            pc.Spawn(Cell, new Vector3(2.75f, 0.01f, -5.2f), 0f, PassengerState.Observe, Disguise.Kandura);
            pc.Threatening = true;
            AudioDirector.I?.Play("pass_creak_1", pc.ChestPosition, 0.8f);
            Hud.I?.ShowCaption("glass creaks in the bathroom doorway", 3f);
            yield return new WaitForSeconds(3.5f);
            pc.SetState(PassengerState.Stalk);
            pc.WalkSpeed = 0.55f;
            BeginEscape();
        }

        void BeginEscape()
        {
            m_Escaping = true;
            Tutorial.I?.Begin(TutorialStep.OpenPassport, TutorialStep.OpenTear, TutorialStep.WidenTear, TutorialStep.StepThrough);
            Narration.I?.Say("tut_traverse", null, true);
            Hud.I?.SetObjective(Objective, false);
        }

        void Update()
        {
            if (!IsLive) return;
            // The reflection corridor breathes faintly.
            m_ReflectionAlpha = 0.14f + Mathf.Sin(Time.time * 0.6f) * 0.03f;
            A.ReflectionMat.SetColor("_Color", new Color(0.75f, 0.75f, 0.7f, m_ReflectionAlpha));
        }

        public override void ResetLocal()
        {
            Game.Passenger?.Despawn();
            m_ThreatStarted = false;
        }
    }
}

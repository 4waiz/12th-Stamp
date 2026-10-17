using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Home in Abu Dhabi: unpack (camera, passport, glasses case), Mum's voicemail, try the glasses on — then the
    /// room quietly stops being the room: the fridge hum stops, a recorded word repeats, the reflection becomes
    /// wrong, the skyline separates into layers. Not a jump scare; the Margin arrives by subtraction.
    /// </summary>
    public static class HomecomingChapter
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() => Homecoming.Routine = Run;

        static bool s_Worn;

        public static IEnumerator Run()
        {
            var story = StoryDirector.I;
            story.TearsAllowed = false;
            story.ThreatAllowed = false;
            story.ShowVitality = false;
            Game.Passport.Available = true;
            s_Worn = false;

            yield return story.EnterCell(CellId.ApartmentNormal, false, "Home");
            var apt = (ApartmentCell)Game.Cells.Active;
            Hud.I?.ShowTitleCard("ABU DHABI", "Home", 3.5f);
            Narration.I?.Say("home_01", null, false, null, false, true);

            // Unpacking: three items in the open suitcase, three spots on the table.
            var props = new GameObject("HomecomingProps").transform;
            props.SetParent(apt.transform, false);
            var cam = PropLibrary.MakeGrabbable(PropLibrary.VintageCamera(props), "camera", "Pick up your camera");
            cam.transform.position = apt.Suitcase.position + new Vector3(-0.18f, 0.22f, 0.02f);
            var pass = PropLibrary.MakeGrabbable(PropLibrary.PassportClosed(props), "passport", "Pick up your passport");
            pass.transform.position = apt.Suitcase.position + new Vector3(0.05f, 0.21f, -0.05f);
            var caseGo = PropLibrary.GlassesCase(props, false);
            var gcase = PropLibrary.MakeGrabbable(caseGo, "glasses_case", "Pick up the glasses case");
            gcase.transform.position = apt.Suitcase.position + new Vector3(0.22f, 0.21f, 0.05f);
            foreach (var g in new[] { cam, pass, gcase }) g.SetHome();

            Socket MakeSpot(string id, Vector3 offset, string label)
            {
                var go = new GameObject("Spot_" + id);
                go.transform.SetParent(props, false);
                go.transform.position = apt.TableTop.position + offset;
                var col = go.AddComponent<BoxCollider>();
                col.isTrigger = true; col.size = new Vector3(0.25f, 0.12f, 0.2f); col.center = new Vector3(0, 0.05f, 0);
                var s = go.AddComponent<Socket>();
                s.Accepts.Add(id);
                s.Prompt = label;
                var outline = MeshFactory.Quad("Outline", go.transform, 0.2f, 0.15f, Mats.Unlit(new Color(0.9f, 0.85f, 0.7f, 0.18f), Tex.Ring, true), new Vector3(0, 0.002f, 0), Quaternion.Euler(90, 0, 0));
                return s;
            }
            var spotCam = MakeSpot("camera", new Vector3(-0.45f, 0f, -0.1f), "Set the camera down");
            var spotPass = MakeSpot("passport", new Vector3(0.05f, 0f, -0.12f), "Set the passport down");
            var spotCase = MakeSpot("glasses_case", new Vector3(0.45f, 0f, -0.08f), "Set the glasses case down");
            apt.Recache();
            Game.Cells.Active.Recache();
            Hud.I?.SetObjective("Unpack: set your camera, passport and glasses case on the table.");
            Tutorial.I?.Begin(TutorialStep.Look, TutorialStep.Move, TutorialStep.Grab);

            while (spotCam.Current == null || spotPass.Current == null || spotCase.Current == null) yield return null;
            Tutorial.I?.End();
            Game.Campaign.SetFlag(StoryFlags.Unpacked);
            Game.SaveNow();
            yield return new WaitForSeconds(1.2f);

            // The voicemail.
            var phone = apt.PhoneRoot.gameObject;
            AudioDirector.I?.Play("phone_buzz", apt.PhoneRoot.position, 0.9f);
            Hud.I?.ShowCaption("your phone buzzes on the table", 3f);
            apt.PhoneScreen.SetColor("_Color", new Color(0.55f, 0.75f, 0.85f, 1f));
            apt.PhoneScreen.SetFloat("_Intensity", 1.4f);
            var phoneUse = phone.AddComponent<Interactable>();
            phoneUse.Prompt = "Play the voicemail";
            bool played = false;
            phoneUse.OnUse = _ =>
            {
                AudioDirector.I?.Play("voicemail_beep", apt.PhoneRoot.position, 0.8f);
                Narration.I?.Say("mom_01", apt.PhoneRoot, false, () => played = true, true);
                phoneUse.Prompt = "Replay the voicemail";
            };
            Hud.I?.SetObjective("Your phone: play the voicemail.");
            while (!played) yield return null;
            yield return new WaitForSeconds(1f);

            // The glasses: open the case, take them out.
            Hud.I?.SetObjective("Open the glasses case and try them on.");
            spotCase.Current.Usable = false;
            var handle = new GameObject("CaseOpenHandle");
            handle.transform.SetParent(caseGo.transform, false);
            var hcol = handle.AddComponent<BoxCollider>();
            hcol.isTrigger = true; hcol.size = new Vector3(0.19f, 0.07f, 0.09f); hcol.center = new Vector3(0, 0.025f, 0);
            var caseOpen = handle.AddComponent<Interactable>();
            caseOpen.Prompt = "Open the case";
            bool opened = false;
            caseOpen.OnUse = _ => { if (opened) return; opened = true; AudioDirector.I?.Play("case_open", caseGo.transform.position, 0.8f); PropLibrary.SetCaseOpen(caseGo, true); };
            while (!opened) yield return null;
            UnityEngine.Object.Destroy(handle);
            var glassesGo = PropLibrary.Glasses(props, true);
            glassesGo.transform.position = caseGo.transform.position + Vector3.up * 0.03f;
            glassesGo.transform.rotation = caseGo.transform.rotation * Quaternion.Euler(-90, 0, 0);
            var col = glassesGo.AddComponent<BoxCollider>();
            col.size = new Vector3(0.15f, 0.06f, 0.06f);
            var glasses = PropLibrary.MakeGrabbable(glassesGo, "glasses", "Take the glasses");
            glasses.ActivatePrompt = Game.Mode == GameMode.VR ? "Lift them to your face" : "Put on the glasses";
            glasses.OnActivate = (g, who) => s_Worn = true;
            glasses.Grabbed += (g, who) => Narration.I?.Say("home_02", null, false, null, false, true);
            apt.Recache();
            AudioDirector.I?.Play("glasses_unfold", glassesGo.transform.position, 0.7f);

            // Wearing: desktop = use the held item; VR = bring them up close to the eyes (no headset touching).
            while (!s_Worn)
            {
                if (Game.Mode == GameMode.VR && glasses.Holder != null && Vector3.Distance(glasses.transform.position, Game.Rig.Head.position) < 0.2f)
                    s_Worn = true;
                yield return null;
            }
            yield return WearAndCorrupt(apt, glasses);
        }

        static IEnumerator WearAndCorrupt(ApartmentCell apt, Grabbable glasses)
        {
            var c = Game.Campaign;
            if (glasses.Holder != null) glasses.Holder.ForceRelease(false);
            glasses.gameObject.SetActive(false);
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

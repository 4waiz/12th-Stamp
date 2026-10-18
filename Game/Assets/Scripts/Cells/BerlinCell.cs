using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>Germany / Berlin — The Departure That Never Happened. The finale: validate the eleven seals, expose and
    /// cancel the forged HOME stamp, and open the real way home on the wall that looked blank.</summary>
    public partial class BerlinCell : Cell
    {
        public TraceMark Plinth;
        public readonly List<Material> SealLights = new List<Material>();
        public readonly List<Transform> DecoyDoors = new List<Transform>();
        public readonly List<Material> DecoyGlows = new List<Material>();
        public SeamAnchor LightSeam, HomeSeam;
        public Grabbable VoidStamp;
        public Transform NoisemakerSpot, BlankWall;
        public Material BlankWallOutline;

        protected override void Build()
        {
            Id = CellId.Berlin;
            Lighting = new LightingPreset
            {
                // An overcast northern day through the glass; inside, the festival's colours read (playtest: grey and
                // sterile) and the false returns glow warm against the cool hall.
                SunColor = new Color(0.82f, 0.85f, 0.9f), SunIntensity = 0.55f, SunEuler = new Vector3(38f, 160f, 0f), ShadowStrength = 0.7f,
                AmbientSky = new Color(0.38f, 0.4f, 0.45f), AmbientEquator = new Color(0.33f, 0.32f, 0.32f), AmbientGround = new Color(0.15f, 0.14f, 0.14f),
                FogColor = new Color(0.62f, 0.64f, 0.68f), FogDensity = 0.012f,
                SkyZenith = new Color(0.42f, 0.46f, 0.54f), SkyHorizon = new Color(0.78f, 0.8f, 0.82f), SkyGround = new Color(0.4f, 0.4f, 0.42f),
                SunDiscColor = new Color(0.9f, 0.9f, 0.9f, 0.2f), CloudColor = new Color(0.85f, 0.86f, 0.9f, 0.6f),
                PostExposure = 0.12f, Saturation = -12f, Contrast = 10f, ColorFilter = new Color(0.97f, 0.98f, 1f), Bloom = 0.5f, Vignette = 0.24f
            };
            Ambience = "amb_berlin_hall";
            AddSun();
            var granite = Mats.Lit(new Color(0.9f, 0.9f, 0.91f), 0.45f, 0f, "granite_pavers");
            var concrete = Mats.Lit(new Color(0.68f, 0.68f, 0.68f), 0.2f, 0f, "concrete_panels");
            var steel = Mats.Lit(new Color(0.55f, 0.57f, 0.6f), 0.6f, 0.9f);
            var glass = Mats.Glass(new Color(0.75f, 0.82f, 0.88f, 0.12f));
            var wood = Mats.Lit(new Color(1f, 0.92f, 0.82f), 0.45f, 0f, "wood_oak_floor");
            var g = Group("Pavilion");
            MeshFactory.Floor("Floor", g, new Vector3(0, 0, 1f), new Vector2(18f, 16f), granite, "stone", 0.3f, 1.8f);
            MeshFactory.Box("Roof", g, new Vector3(0, 6.1f, 1f), new Vector3(18.4f, 0.3f, 16.4f), concrete, false, 3f);
            MeshFactory.Box("SouthWall", g, new Vector3(0, 3f, -7.1f), new Vector3(18.4f, 6f, 0.3f), concrete, true, 2f);
            MeshFactory.Box("EastWall", g, new Vector3(9.1f, 3f, 1f), new Vector3(0.3f, 6f, 16.4f), concrete, true, 2f);
            // The blank fourth wall (west).
            BlankWall = MeshFactory.Box("BlankWall", g, new Vector3(-9.1f, 3f, 1f), new Vector3(0.3f, 6f, 16.4f), concrete, true, 2f).transform;
            BlankWallOutline = Mats.UnlitInstance(new Color(1f, 0.85f, 0.6f, 0f), Tex.Ring, true, 1.6f);
            BlankWallOutline.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            MeshFactory.Quad("TrueReturnOutline", g, 1.4f, 2.4f, BlankWallOutline, new Vector3(-8.93f, 1.2f, 1f), Quaternion.Euler(0, -90, 0));
            // North: a glass wall framing the six-column gate across the square.
            for (int i = 0; i < 7; i++) MeshFactory.Box("Mullion", g, new Vector3(-9f + i * 3f, 3f, 9.1f), new Vector3(0.12f, 6f, 0.12f), steel, true);
            var gw = MeshFactory.Box("GlassWall", g, new Vector3(0, 3f, 9.1f), new Vector3(18.4f, 6f, 0.04f), glass, false);
            gw.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var gwc = new GameObject("GlassWallCollider"); gwc.transform.SetParent(g, false); gwc.transform.localPosition = new Vector3(0, 3f, 9.1f); gwc.AddComponent<BoxCollider>().size = new Vector3(18.4f, 6f, 0.2f);
            MeshFactory.Floor("Square", g, new Vector3(0, -0.05f, 40f), new Vector2(120f, 60f), granite, "stone", 0.2f, 4f);
            Landmarks.ColumnGate(g, new Vector3(0, -0.05f, 62f), 0f, Mats.Backdrop(new Color(0.72f, 0.7f, 0.66f), Lighting), 1f);
            // Festival banners, signs and the rest of the dressing: BuildPavilionDetail (Detail_Berlin.cs).
            for (int i = 0; i < 4; i++) AddPointLight(g, new Vector3(-6f + i * 4f, 5.6f, 1f), new Color(1f, 0.96f, 0.9f), 1.5f, 9f);

            // Three decoy "home" doors along the east wall, warm and inviting.
            for (int i = 0; i < 3; i++)
            {
                var d = new GameObject("DecoyDoor" + i).transform;
                d.SetParent(g, false);
                d.localPosition = new Vector3(8.9f, 0, -2.4f + i * 2.4f);
                d.localRotation = Quaternion.Euler(0, 90, 0); // local -Z faces into the room
                BuildDecoyDoor(d); // a front door from home: architrave, panelled leaf, brass, a mat
                var glowMat = Mats.UnlitInstance(new Color(1f, 0.72f, 0.42f, 0.9f), Tex.SoftDot, true, 1.6f);
                MeshFactory.Quad("Glow", d, 0.95f, 2.05f, glowMat, new Vector3(0, 1.05f, -0.12f));
                var label = TextFactory.World(d, CountryDatabase.CounterfeitLabel, 0.07f, new Color(0.55f, 0.08f, 0.07f), TMPro.TextAlignmentOptions.Center, true, 1.2f);
                label.transform.localPosition = new Vector3(0, 2.45f, -0.14f);
                DecoyDoors.Add(d);
                DecoyGlows.Add(glowMat);
            }

            // Registration plinth with eleven seal lights (lit for verified seals).
            var plinth = new GameObject("RegistrationPlinth").transform;
            plinth.SetParent(g, false);
            plinth.localPosition = new Vector3(0, 0, 1.5f);
            MeshFactory.Cylinder("Base", plinth, Vector3.zero, 0.75f, 1.0f, concrete, 32);
            MeshFactory.Cylinder("Top", plinth, new Vector3(0, 1.0f, 0), 0.8f, 0.06f, steel, 32, false);
            for (int i = 0; i < 11; i++)
            {
                float a = i / 11f * Mathf.PI * 2f;
                var m = Mats.UnlitInstance(new Color(0.25f, 0.25f, 0.25f, 1f), Tex.SoftDot, true, 1.5f);
                MeshFactory.Quad("SealLight" + i, plinth, 0.12f, 0.12f, m, new Vector3(Mathf.Cos(a) * 0.62f, 1.065f, Mathf.Sin(a) * 0.62f), Quaternion.Euler(90, 0, 0));
                SealLights.Add(m);
            }
            Plinth = TraceMark.Create(transform, CountryId.Germany, new Vector3(0, 1.07f, 1.5f), Quaternion.identity, 0.5f, true);
            Plinth.Prompt = "Validate the eleven seals";

            // Side table with the VOID stamp; a spot where the noisemaker is set down.
            MeshFactory.Box("SideTable", g, new Vector3(-3f, 0.45f, -4.5f), new Vector3(1.2f, 0.9f, 0.6f), wood, true);
            var vs = new GameObject("VoidStamp");
            vs.transform.SetParent(g, false);
            vs.transform.localPosition = new Vector3(-3.2f, 0.92f, -4.5f);
            var ink = Mats.Lit(new Color(0.55f, 0.06f, 0.06f), 0.4f);
            var stampWood = Mats.Lit(new Color(0.95f, 0.85f, 0.75f), 0.5f, 0f, "wood_dark");
            // The modelled stamp; its rubber die carries the red ink of the procedural pad.
            if (Models.Spawn("VoidStamp", vs.transform, Models.Slots(("TS_Rubber", ink), ("TS_WoodDark", stampWood))) == null)
            {
                MeshFactory.Box("Pad", vs.transform, new Vector3(0, 0.02f, 0), new Vector3(0.11f, 0.04f, 0.07f), ink, false);
                MeshFactory.Cylinder("Handle", vs.transform, new Vector3(0, 0.04f, 0), 0.02f, 0.09f, stampWood, 12, false);
                MeshFactory.Sphere("Knob", vs.transform, new Vector3(0, 0.15f, 0), 0.03f, Mats.Lit(new Color(0.3f, 0.2f, 0.12f), 0.5f));
            }
            vs.AddComponent<BoxCollider>().size = new Vector3(0.13f, 0.2f, 0.09f);
            VoidStamp = PropLibrary.MakeGrabbable(vs, "void_stamp", "Take the VOID stamp");
            VoidStamp.ActivatePrompt = "Stamp VOID over the forged HOME seal";
            NoisemakerSpot = new GameObject("NoisemakerSpot").transform;
            NoisemakerSpot.SetParent(g, false);
            NoisemakerSpot.localPosition = new Vector3(-2.4f, 0.92f, -4.5f);

            BuildPavilionDetail(g, steel, concrete);
            AddSeam("arrival", new Vector3(0f, 0f, -6.6f), 0f);
            LightSeam = AddSeam("light", new Vector3(2.0f, 0f, 0f), 90f, 1.4f); // faces the middle decoy door, 6.9 m away
            HomeSeam = AddSeam("home", new Vector3(-8.7f, 0f, 1f), 90f);
            HomeSeam.HomeSeam = true;
            HomeSeam.Revealed = false;
            SetArrival(new Vector3(0f, 0f, -5.6f), 0f);
            AddBounds(new Vector3(0, 0, 1f), new Vector3(17.8f, 6f, 15.8f));
            SetEncounter(gameObject.AddComponent<BerlinEncounter>());
            Expand_Germany.Build(this, g); // the Festivalhof outside the north glass wall (after AddBounds: it splits the north bound)
        }
    }

    public class BerlinEncounter : Encounter
    {
        BerlinCell B => (BerlinCell)Cell;
        // Read-only state for the guide (Guide/GuideBook.cs).
        public bool NoiseOn => m_NoiseOn;
        public bool ThreatUp => m_ThreatUp;
        Grabbable m_Noise;
        AudioSource m_NoiseLoop;
        bool m_NoiseOn, m_Exposed, m_ThreatUp;
        float m_ExposeTime, m_InvestigateCooldown;

        public bool Exposed => m_Exposed;

        public override string Objective
        {
            get
            {
                var c = Game.Campaign;
                if (!c.IsVerified(CountryId.Germany)) return "Validate the eleven seals at the registration plinth (" + c.VerifiedCount + " / 11 verified).";
                if (!m_Exposed) return "Draw it away with the noisemaker, and open a tear to Egypt at the seam facing the doors so sunlight exposes the forgery.";
                if (!c.HasFlag(StoryFlags.ForgedCancelled)) return "Take the VOID stamp and cancel the forged HOME seal in your passport.";
                if (Game.Tears.Active != null && !Game.Tears.Active.Anchor.HomeSeam) return "Seal the resource tear. Then open the real way home on the blank wall.";
                return "Open the genuine way home on the blank wall. Not the door that looks like home.";
            }
        }

        public override void OnEnter(bool firstVisit)
        {
            var c = Game.Campaign;
            c.SetFlag(StoryFlags.FinaleStarted);
            Narration.I?.Say("ger_01", null, false, null, false, true);
            B.Plinth.BlockedReason = () =>
            {
                int others = 0;
                foreach (var d in CountryDatabase.All) if (d.Id != CountryId.Germany && c.IsVerified(d.Id)) others++;
                if (others < 10) return "Registration incomplete: " + others + " of the 10 other seals verified. Come back with the rest.";
                if (!c.HasResource(ResourceId.CalibrationInsert)) return "The pattern won't resolve without the calibration insert (Marrakech).";
                return null;
            };
            B.Plinth.Reveal(false);
            B.VoidStamp.OnActivate = OnVoid;
            Game.Tears.Opened -= OnTearOpened; Game.Tears.Opened += OnTearOpened;
            m_Exposed = c.HasFlag(StoryFlags.ForgedCancelled);
            B.HomeSeam.Revealed = c.HasFlag(StoryFlags.ForgedCancelled);
            if (c.HasResource(ResourceId.Noisemaker) && m_Noise == null) SpawnNoisemaker();
            Game.Passenger?.Despawn();
            if (c.IsVerified(CountryId.Germany)) RaiseThreat();
            RefreshLights();
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit()
        {
            Game.Tears.Opened -= OnTearOpened;
            if (m_NoiseOn) ToggleNoise();
        }

        void SpawnNoisemaker()
        {
            var go = new GameObject("CarriedNoisemaker");
            go.transform.SetParent(B.transform, false);
            go.transform.position = B.NoisemakerSpot.position;
            if (!PropLibrary.NoisemakerModel(go.transform))
            {
                MeshFactory.Box("Body", go.transform, new Vector3(0, 0.07f, 0), new Vector3(0.22f, 0.14f, 0.09f), Mats.Lit(new Color(1f, 0.95f, 0.9f), 0.4f, 0f, "leather_navy"), false);
                MeshFactory.Box("Grille", go.transform, new Vector3(-0.04f, 0.07f, -0.047f), new Vector3(0.11f, 0.1f, 0.005f), Mats.Lit(new Color(0.75f, 0.7f, 0.6f), 0.6f, 0.9f, "metal_brushed"), false);
            }
            go.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.16f, 0.11f);
            m_Noise = PropLibrary.MakeGrabbable(go, "noisemaker", "Take the noisemaker (from New York)", true);
            m_Noise.ActivatePrompt = "Switch the noisemaker on/off";
            m_Noise.OnActivate = (g, w) => ToggleNoise();
            m_Noise.SetHome();
            Cell.Recache();
        }

        void ToggleNoise()
        {
            m_NoiseOn = !m_NoiseOn;
            if (m_NoiseOn) m_NoiseLoop = AudioDirector.I?.Loop("noisemaker_loop", m_Noise.transform, 0.9f, Bus.SFX, 1f, 2f, 30f);
            else if (m_NoiseLoop != null) { AudioDirector.I?.FadeOutAndStop(m_NoiseLoop, 0.2f); m_NoiseLoop = null; }
        }

        public void OnVerified()
        {
            // Eleven seals: the calibration insert overlays the pattern and the forgery shows its hand.
            RefreshLights();
            StartCoroutine(Reveal());
        }

        IEnumerator Reveal()
        {
            yield return new WaitForSeconds(1.5f);
            AudioDirector.I?.Play("seal_counterfeit", B.transform.position + Vector3.up, 0.9f, 0.9f, Bus.SFX, true);
            Hud.I?.ShowToast("The insert aligns with all eleven seals. Twelve stamps. One was never issued.", 4.5f);
            for (float t = 0; t < 1f; t += Time.deltaTime / 2f) { B.BlankWallOutline.SetColor("_Color", new Color(1f, 0.85f, 0.6f, t * 0.25f)); yield return null; }
            RaiseThreat();
            Hud.I?.SetObjective(Objective, false);
        }

        void RaiseThreat()
        {
            if (m_ThreatUp) return;
            m_ThreatUp = true;
            var pc = Game.Passenger;
            pc.GuardPoint = B.transform.TransformPoint(new Vector3(-7.6f, 0f, 1f));
            pc.RetreatPoint = B.transform.TransformPoint(new Vector3(-7.8f, 0f, 7.5f));
            pc.RetreatSeconds = 16f;
            pc.Spawn(Cell, new Vector3(-7.6f, 0f, 1f), 90f, PassengerState.Guard, Disguise.Mimic);
            pc.Threatening = true;
            pc.WalkSpeed = 0.9f;
            // Late escalation: it opens its own passport to inspect your world.
            pc.SetState(PassengerState.Mimic);
            AudioDirector.I?.Play("pp_open", pc.ChestPosition, 0.8f);
            Hud.I?.ShowCaption("it opens a passport of its own and studies you", 3.5f);
            StartCoroutine(BackToGuard(pc));
        }

        IEnumerator BackToGuard(PassengerController pc)
        {
            yield return new WaitForSeconds(4f);
            if (pc.State == PassengerState.Mimic) { pc.Body.ShowImitationPassport(false); pc.SetState(PassengerState.Guard); }
        }

        void OnTearOpened(Tear t)
        {
            if (!IsLive) return;
            if (t.Anchor.HomeSeam) Narration.I?.Say("fin_01", null, false, null, true, true);
        }

        void OnVoid(Grabbable g, Interactor who)
        {
            var c = Game.Campaign;
            if (!c.IsVerified(CountryId.Germany)) { Hud.I?.ShowToast("Validate the eleven seals first.", 2.5f); return; }
            if (!m_Exposed) { Hud.I?.ShowToast("Expose the forgery in daylight first: open a tear to Egypt facing the doors.", 3.5f); return; }
            if (c.HasFlag(StoryFlags.ForgedCancelled)) return;
            if (!Game.Passport.IsOpen) Game.Passport.Open(who != null ? (Game.Mode == GameMode.VR ? Game.Input.PassportHand : HandSide.Desktop) : HandSide.Desktop);
            c.SetFlag(StoryFlags.ForgedCancelled);
            AudioDirector.I?.Play("stamp_void", g.transform.position, 1f);
            AudioDirector.Haptic(who != null ? who.Side : HandSide.Desktop, 1f, 0.12f);
            Game.Passport.Refresh();
            B.HomeSeam.Revealed = true;
            foreach (var d in B.DecoyDoors) d.gameObject.SetActive(false);
            Hud.I?.ShowToast("VOID. The forged authorisation is cancelled.", 3.5f);
            Game.SaveNow();
            Hud.I?.SetObjective(Objective, false);
        }

        void RefreshLights()
        {
            int i = 0;
            foreach (var d in CountryDatabase.InRecommendedOrder)
            {
                if (i >= B.SealLights.Count) break;
                bool v = Game.Campaign.IsVerified(d.Id);
                B.SealLights[i].SetColor("_Color", v ? new Color(1f, 0.82f, 0.5f, 1f) : new Color(0.25f, 0.25f, 0.25f, 0.6f));
                i++;
            }
        }

        void Update()
        {
            if (!IsLive || Game.Paused) return;
            var c = Game.Campaign;
            // Egyptian sunlight on the middle decoy door exposes the forgery (all three doors fail together).
            if (!m_Exposed && c.IsVerified(CountryId.Germany))
            {
                var door = B.DecoyDoors[1];
                var front = door.position - door.forward * 0.35f;
                float e = Mathf.Max(LightVolumes.Exposure(front + Vector3.up * 0.9f, true),
                          Mathf.Max(LightVolumes.Exposure(front + Vector3.up * 1.3f, true), LightVolumes.Exposure(front + Vector3.up * 1.7f, true)));
                m_ExposeTime = e > 0.25f ? m_ExposeTime + Time.deltaTime : Mathf.Max(0f, m_ExposeTime - Time.deltaTime);
                for (int i = 0; i < B.DecoyGlows.Count; i++)
                    B.DecoyGlows[i].SetColor("_Color", new Color(1f, 0.72f, 0.42f, 0.9f * (1f - Mathf.Clamp01(m_ExposeTime / 1.2f) * 0.85f)));
                if (m_ExposeTime > 1.2f)
                {
                    m_Exposed = true;
                    AudioDirector.I?.Play("glasses_break", B.DecoyDoors[1].position + Vector3.up, 0.8f, 0.7f);
                    Hud.I?.ShowToast("In real daylight the doors are only paint and ink. Forged.", 3.5f);
                    Hud.I?.SetObjective(Objective, false);
                }
            }
            // The noisemaker buys time.
            var pc = Game.Passenger;
            m_InvestigateCooldown -= Time.deltaTime;
            if (m_NoiseOn && m_Noise != null && pc != null && pc.Visible && m_InvestigateCooldown <= 0f && pc.State != PassengerState.Repelled && pc.State != PassengerState.Retreat)
            {
                pc.Investigate(m_Noise.transform.position, 16f);
                m_InvestigateCooldown = 1.5f;
            }
            // The home tear opens at the wall it guarded: it comes for the threshold.
            var tear = Game.Tears.Active;
            if (tear != null && tear.Anchor.HomeSeam && tear.Size >= TearSize.Window && pc != null && pc.Visible && pc.State != PassengerState.Repelled)
            {
                pc.WalkSpeed = 1.15f;
                if (pc.State != PassengerState.Retreat) pc.GoTo(tear.OpeningCenter + tear.Anchor.Forward * 0.8f, PassengerState.Stalk);
            }
        }
    }
}

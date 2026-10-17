using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>Japan — The Empty Procession. Lanterns cast the shadows of people who aren't there. Hang the hand
    /// lantern where only the genuine route mark still holds a shadow.</summary>
    public partial class JapanCell : Cell
    {
        public readonly List<Socket> Hooks = new List<Socket>();
        public readonly List<GameObject> Marks = new List<GameObject>();
        public readonly List<Transform> ProcessionShadows = new List<Transform>();
        public Grabbable HandLantern;
        public TraceMark Trace;
        public int CorrectHook = 1;

        protected override void Build()
        {
            Id = CellId.Japan;
            Lighting = new LightingPreset
            {
                // Lantern-lit night that still reads on a monitor: warm chochin pools down the street, a cool moon raking in
                // from behind the arrival (the left-hand fronts and the gate catch it, the right-hand side stays in lantern
                // light), a lifted ambient, and a dusky horizon so roofs, poles and wires stand out against the sky.
                SunColor = new Color(0.55f, 0.62f, 0.85f), SunIntensity = 0.6f, SunEuler = new Vector3(34f, 335f, 0f), ShadowStrength = 0.6f,
                AmbientSky = new Color(0.3f, 0.31f, 0.43f), AmbientEquator = new Color(0.31f, 0.25f, 0.25f), AmbientGround = new Color(0.12f, 0.1f, 0.09f),
                FogColor = new Color(0.17f, 0.14f, 0.19f), FogDensity = 0.018f,
                SkyZenith = new Color(0.045f, 0.05f, 0.1f), SkyHorizon = new Color(0.36f, 0.24f, 0.32f), SkyGround = new Color(0.07f, 0.05f, 0.06f),
                SunDiscColor = new Color(0, 0, 0, 0), Stars = 0.45f,
                PostExposure = 0.45f, Saturation = 2f, Contrast = 10f, ColorFilter = new Color(1f, 0.94f, 0.88f), Bloom = 0.65f, Vignette = 0.24f
            };
            Ambience = "amb_japan_night";
            AddSun();
            var slate = Mats.Lit(new Color(0.64f, 0.64f, 0.68f), 0.72f, 0f, "granite_pavers");
            var wood = Mats.Lit(new Color(0.24f, 0.17f, 0.12f), 0.4f, 0f, "wood_dark");
            var postWood = Mats.Lit(new Color(0.8f, 0.78f, 0.76f), 0.35f, 0f, "wood_dark");
            var lanternMat = Mats.Lit(new Color(0.95f, 0.75f, 0.5f), 0.1f, 0f, "paper_shoji", false, new Color(1.6f, 0.75f, 0.3f));
            var g = Group("Street");
            // The street's collider and footstep surface (BuildStreetDetail draws the stone in pieces and hides this mesh).
            MeshFactory.Floor("Paving", g, new Vector3(0, 0, 3f), new Vector2(7f, 20f), slate, "stone", 0.3f, 1f);
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                {
                    // Each house stays solid for the rig and the Passenger; its front, chochin and light are built in BuildStreetDetail.
                    var house = new GameObject("House").transform;
                    house.SetParent(g, false);
                    house.localPosition = new Vector3(side * 3.8f, 0, -5f + i * 4.6f);
                    var wall = house.gameObject.AddComponent<BoxCollider>();
                    wall.center = new Vector3(side * 0.3f, 1.6f, 0);
                    wall.size = new Vector3(0.6f, 3.2f, 4.4f);
                }
            // The timber gate at the end of the street (shut until the puzzle is solved) and the lane beyond it are built in
            // BuildStreetDetail / BuildRoute, with their colliders.
            Landmarks.Pagoda(g, PagodaPos, 0f, Mats.Backdrop(new Color(0.14f, 0.1f, 0.12f), Lighting), Mats.Backdrop(new Color(0.22f, 0.12f, 0.12f), Lighting));

            // Shadows of an absent procession drift across the paving.
            var shadowMat = Mats.UnlitInstance(new Color(0f, 0f, 0f, 0.55f), Tex.SoftDot, true, 1f);
            for (int i = 0; i < 6; i++)
            {
                var s = MeshFactory.Quad("AbsentShadow", g, 0.6f, 1.9f, shadowMat, new Vector3(-1.5f + i * 0.6f, 0.012f, -2f + i * 2.2f), Quaternion.Euler(90, 0, 0));
                s.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                ProcessionShadows.Add(s.transform);
            }

            // Three route marks near the end gate; only one is real.
            var markMat = Mats.Unlit(new Color(0.95f, 0.92f, 0.85f, 0.5f), Tex.Ring, true);
            for (int i = 0; i < 3; i++)
                Marks.Add(MeshFactory.Quad("RouteMark", g, 0.7f, 0.7f, markMat, new Vector3(-1.8f + i * 1.8f, 0.014f, 10.5f), Quaternion.Euler(90, 0, 0)));
            Trace = TraceMark.Create(transform, CountryId.Japan, new Vector3(-1.8f + 1.8f * CorrectHook, 0.02f, 10.5f), Quaternion.identity, 0.6f, true);

            // Three lantern hooks on posts and the hand lantern.
            for (int i = 0; i < 3; i++)
            {
                var post = MeshFactory.Box("HookPost", g, new Vector3(-2.2f + i * 2.2f, 1.1f, 7.6f), new Vector3(0.12f, 2.2f, 0.12f), postWood, true);
                var hook = new GameObject("Hook" + i);
                hook.transform.SetParent(g, false);
                hook.transform.localPosition = new Vector3(-2.2f + i * 2.2f, 2.0f, 7.45f);
                var col = hook.AddComponent<BoxCollider>(); col.isTrigger = true; col.size = new Vector3(0.4f, 0.5f, 0.4f);
                var sock = hook.AddComponent<Socket>();
                sock.Accepts.Add("hand_lantern");
                sock.Prompt = "Hang the lantern";
                Hooks.Add(sock);
            }
            var hl = new GameObject("HandLantern");
            hl.transform.SetParent(g, false);
            hl.transform.localPosition = new Vector3(1.6f, 0.02f, 1.5f);
            // The modelled chochin hangs 0.51 m below its grip (its origin): lifted so it stands on the root like the
            // procedural lantern. Its paper glows like the street lanterns; the glow inside sits around the light.
            var glow = Mats.Emissive(new Color(1f, 0.78f, 0.5f), new Color(2.2f, 1.2f, 0.5f));
            if (Models.Spawn("HandLantern", hl.transform, Models.Slots(("TS_Paper", lanternMat), ("TS_WoodDark", wood), ("TS_EmissiveWarm", glow)), new Vector3(0f, 0.51f, 0f)) == null)
            {
                MeshFactory.Frustum("Body", hl.transform, Vector3.zero, 0.14f, 0.14f, 0.32f, lanternMat, 12);
                MeshFactory.Box("Handle", hl.transform, new Vector3(0, 0.38f, 0), new Vector3(0.02f, 0.12f, 0.02f), wood, false);
            }
            hl.AddComponent<BoxCollider>().size = new Vector3(0.3f, 0.45f, 0.3f);
            var lightGo = new GameObject("LanternLight");
            lightGo.transform.SetParent(hl.transform, false);
            lightGo.transform.localPosition = new Vector3(0, 0.18f, 0);
            var ll = lightGo.AddComponent<Light>(); ll.type = LightType.Point; ll.color = new Color(1f, 0.62f, 0.3f); ll.intensity = 1.6f; ll.range = 5f; ll.shadows = LightShadows.None;
            HandLantern = PropLibrary.MakeGrabbable(hl, "hand_lantern", "Take the hand lantern");
            HandLantern.HoldLocalPosition = new Vector3(0, -0.25f, 0);

            BuildStreetDetail(g, postWood);
            Expand_Japan.Build(this, g); // the tea garden off the lantern approach, street crows, lane lamps (Cells/Expand_Japan.cs)
            AddSeam("arrival", new Vector3(0f, 0f, -6.6f), 0f);
            AddSeam("exit", new Vector3(0f, 0f, 12.9f), 180f);
            SetArrival(new Vector3(0f, 0f, -5.6f), 0f);
            // Open world: one box round the street, the lane past the end gate and the precinct up to just past the stamp
            // spot on the lantern approach. The street keeps its old edges (walls of its own, BuildRoute), the lane is walled,
            // and the end gate stays shut until the lanterns tell the truth.
            AddBounds(new Vector3(-3.7f, -1.5f, 50.5f), new Vector3(16.6f, 7.5f, 113f), 7.5f);
            SetEncounter(gameObject.AddComponent<JapanEncounter>());
            Landmarks.MarkReachable(this);
        }
    }

    public class JapanEncounter : Encounter
    {
        JapanCell J => (JapanCell)Cell;
        bool m_Solved;

        public override string Objective => "Hang the hand lantern on the hook where only the genuine route mark still casts a shadow.";

        public override void OnEnter(bool firstVisit)
        {
            m_Solved = Game.Campaign.IsVerified(CountryId.Japan);
            // The end gate stands open once the seal is verified; the stamp is only offered out at the pagoda while the
            // whole way there (lane, precinct gate, approach) is actually walkable.
            J.Gate?.SetOpen(m_Solved, true);
            Landmarks.MarkReachable(J, J.RouteOpen());
            foreach (var h in J.Hooks) { h.Filled -= OnHung; h.Filled += OnHung; }
            if (m_Solved) { J.Trace.Reveal(false); return; }
            J.Trace.Hide();
            Narration.I?.Say("jpn_01", null, false, null, false, true);
            Game.Passenger?.Despawn();
            var pc = Game.Passenger;
            pc.RetreatPoint = J.transform.TransformPoint(new Vector3(0, 0, 12.5f));
            pc.Spawn(Cell, new Vector3(0.4f, 0f, 12.2f), 180f, PassengerState.Observe, Disguise.LanternPaper);
            pc.Threatening = true;
            pc.WalkSpeed = 0.5f;
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit()
        {
            foreach (var h in J.Hooks) h.Filled -= OnHung;
        }

        void OnHung(Socket s, Grabbable g)
        {
            if (!IsLive || m_Solved) return;
            int idx = J.Hooks.IndexOf(s);
            if (idx == J.CorrectHook)
            {
                m_Solved = true;
                for (int i = 0; i < J.Marks.Count; i++) if (i != J.CorrectHook) J.Marks[i].SetActive(false);
                J.Trace.Reveal();
                Hud.I?.ShowToast("Two marks vanish with the change of light. They were only shadows.", 3.5f);
                Narration.I?.Say("tut_verify", null, true, null, false, true);
                if (J.Gate != null && !J.Gate.IsOpen)
                {
                    // The way on opens: the end gate swings out onto the lane toward the pagoda.
                    J.Gate.SetOpen(true, false);
                    AudioDirector.I?.Play("door_open", J.Gate.Blocker.transform.position, 0.8f);
                    Hud.I?.ShowCaption("the gate at the end of the street swings open", 3f);
                }
                if (Game.Passenger != null && Game.Passenger.Visible) Game.Passenger.SetState(PassengerState.Stalk);
            }
            else
            {
                // Wrong hook: the procession turns toward the light; take the lantern down and try another.
                AudioDirector.I?.Play("pass_whisper", s.transform.position, 0.7f);
                Hud.I?.ShowCaption("the procession of shadows turns toward the lantern", 3f);
                s.LockWhenFilled = false;
                g.Usable = true;
            }
        }

        void Update()
        {
            if (!IsLive) return;
            // The absent procession walks slowly toward the gate.
            foreach (var s in J.ProcessionShadows)
            {
                var p = s.localPosition;
                p.z += Time.deltaTime * 0.35f;
                if (p.z > 11f) p.z = -5f;
                p.x += Mathf.Sin(Time.time * 0.7f + p.z) * Time.deltaTime * 0.05f;
                s.localPosition = p;
            }
        }

        public override void ResetLocal()
        {
            J.HandLantern.ReturnHome();
            Game.Passenger?.Despawn();
        }
    }

    /// <summary>Saudi Arabia / Riyadh — The False Horizon. Three arches with contradictory reflections; match the
    /// optical panel to the registration mark and walk through the honest arch. A wrong arch resets only this puzzle.</summary>
    public partial class RiyadhCell : Cell
    {
        public StepRotator Panel;
        public readonly List<Transform> Arches = new List<Transform>();
        public readonly List<Material> Reflections = new List<Material>();
        public TraceMark Trace;
        public int HonestArch = 2;
        public int TargetIndex = 3;

        protected override void Build()
        {
            Id = CellId.Riyadh;
            Lighting = new LightingPreset
            {
                // Dusk with the sun low behind the arrival's right shoulder: it lights the arches' faces and the west houses,
                // throws long shadows forward across the plaza, and leaves the sky warm over the skyline; lamps and lit vents
                // come on as practicals. (The east side is kept low so this sun still reaches the plaza.)
                SunColor = new Color(1f, 0.68f, 0.46f), SunIntensity = 1.15f, SunEuler = new Vector3(13f, 302f, 0f), ShadowStrength = 0.72f,
                AmbientSky = new Color(0.42f, 0.4f, 0.52f), AmbientEquator = new Color(0.52f, 0.4f, 0.33f), AmbientGround = new Color(0.24f, 0.17f, 0.13f),
                FogColor = new Color(0.56f, 0.42f, 0.38f), FogDensity = 0.011f,
                SkyZenith = new Color(0.13f, 0.16f, 0.33f), SkyHorizon = new Color(0.96f, 0.57f, 0.4f), SkyGround = new Color(0.36f, 0.25f, 0.2f),
                SunDiscColor = new Color(1f, 0.62f, 0.36f, 1f), SunSize = 0.007f, CloudColor = new Color(0.95f, 0.62f, 0.5f, 0.22f),
                PostExposure = 0.15f, Saturation = 4f, Contrast = 10f, ColorFilter = new Color(1f, 0.95f, 0.9f), Bloom = 0.6f, Vignette = 0.22f
            };
            Ambience = "amb_riyadh_wind";
            AddSun();
            var plaza = Mats.Lit(new Color(0.84f, 0.74f, 0.6f), 0.3f, 0f, "plaza_stone");
            var stone = Mats.Lit(new Color(0.82f, 0.7f, 0.56f), 0.2f, 0f, "sandstone_blocks");
            var metal = Mats.Lit(new Color(0.65f, 0.6f, 0.55f), 0.7f, 0.9f, "metal_brushed");
            var g = Group("Plaza");
            MeshFactory.Floor("Plaza", g, new Vector3(0, 0, 3f), new Vector2(16f, 16f), plaza, "stone", 0.3f, 1.6f);
            Landmarks.ArchTower(g, TowerPos, 0f, Mats.Backdrop(new Color(0.2f, 0.2f, 0.28f), Lighting), Mats.Backdrop(new Color(0.6f, 0.7f, 1f), Lighting, new Color(0.6f, 0.8f, 1.6f)));
            Landmarks.Skyline(g, new Vector3(0, -3f, 0), 230f, 60f, 0f, 12, Mats.Backdrop(new Color(0.25f, 0.22f, 0.3f), Lighting), null, 41);
            var skyTints = new[] { new Color(0.35f, 0.5f, 0.95f, 0.75f), new Color(0.95f, 0.35f, 0.25f, 0.75f), new Color(0.95f, 0.6f, 0.45f, 0.75f) };
            for (int i = 0; i < 3; i++)
            {
                var arch = new GameObject("Arch" + i).transform;
                arch.SetParent(g, false);
                arch.localPosition = new Vector3(-4f + i * 4f, 0, 6f);
                // Dressed sandstone at a block size that reads as masonry, not brick; BuildPlazaDetail plasters the rest.
                MeshFactory.Box("PierL", arch, new Vector3(-1.15f, 1.5f, 0), new Vector3(0.5f, 3f, 0.8f), stone, true, 2.5f);
                MeshFactory.Box("PierR", arch, new Vector3(1.15f, 1.5f, 0), new Vector3(0.5f, 3f, 0.8f), stone, true, 2.5f);
                var mb = new MeshBuilder();
                mb.AddArch(new Vector3(0, 3f, 0), 0.9f, 1.4f, 0.8f, 12, Quaternion.identity, 2.5f);
                MeshFactory.FromBuilder("Arc", mb, stone, arch);
                var refl = Mats.UnlitInstance(skyTints[i], Tex.Albedo("glass_smudge"), true, 1f);
                MeshFactory.Quad("Reflection", arch, 1.8f, 3.6f, refl, new Vector3(0, 1.8f, 0.05f));
                Reflections.Add(refl);
                Arches.Add(arch);
            }
            Trace = TraceMark.Create(transform, CountryId.SaudiArabia, new Vector3(-4f + 4f * HonestArch, 0.02f, 7.6f), Quaternion.identity, 0.6f, true);

            // Optical panel on a stand, with the registration mark plaque beside it.
            var stand = new GameObject("PanelStand").transform;
            stand.SetParent(g, false);
            stand.localPosition = new Vector3(0, 0, 1.5f);
            var post = MeshFactory.Cylinder("Post", stand, Vector3.zero, 0.05f, 1.1f, metal, 10);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(stand, false);
            pivot.localPosition = new Vector3(0, 1.35f, 0);
            pivot.localRotation = Quaternion.Euler(-90, 0, 0);
            var disc = new MeshBuilder();
            disc.AddDisc(Vector3.zero, Vector3.up, 0.32f, 40);
            disc.AddDisc(Vector3.zero, Vector3.down, 0.32f, 40);
            var calib = Tex.Art("calibration_insert");
            var optic = MeshFactory.FromBuilder("Optic", disc, Mats.Unlit(new Color(0.75f, 0.95f, 0.9f, 0.9f), calib, true, 1.1f), pivot);
            // The modelled stand and lens, scaled so the disc centre (1.255 m) meets the pivot. The model turns about its
            // vertical axis but this puzzle spins the pattern about the disc's axis, so the model stays still and only the
            // calibration pattern turns, now just in front of the lens glass.
            var lensGlass = Mats.Glass(new Color(0.75f, 0.92f, 0.9f, 0.12f));
            if (Models.Spawn("OpticalPanel", stand, Models.Slots(("TS_Metal", metal), ("TS_Glass", lensGlass)), Vector3.zero, 180f, 1.35f / 1.255f) != null)
            {
                Models.StripVisual(post);
                optic.transform.localPosition = new Vector3(0f, 0.022f, 0f);
            }
            else
            {
                var ring = new MeshBuilder();
                ring.AddTorus(Vector3.zero, 0.33f, 0.02f, 32, 6, Quaternion.identity);
                MeshFactory.FromBuilder("Ring", ring, metal, pivot);
            }
            pivot.gameObject.AddComponent<BoxCollider>().size = new Vector3(0.7f, 0.08f, 0.7f);
            Panel = pivot.gameObject.AddComponent<StepRotator>();
            Panel.Target = pivot;
            Panel.Axis = Vector3.up;
            Panel.StepDegrees = 45f;
            Panel.Steps = 8;
            Panel.Prompt = "Rotate the optical panel";
            var plaque = MeshFactory.Box("Plaque", g, new Vector3(0.85f, 1.1f, 1.5f), new Vector3(0.36f, 0.36f, 0.04f), metal, true);
            var arrow = TextFactory.World(plaque.transform, "↖", 0.25f, new Color(0.5f, 0.1f, 0.08f), TMPro.TextAlignmentOptions.Center, false, 0.3f);
            arrow.transform.localPosition = new Vector3(0, 0, -0.025f);
            var label = TextFactory.World(plaque.transform, "registration mark", 0.025f, new Color(0.25f, 0.2f, 0.18f), TMPro.TextAlignmentOptions.Center, false, 0.34f);
            label.transform.localPosition = new Vector3(0, -0.14f, -0.025f);

            BuildPlazaDetail(g);
            Expand_Riyadh.Build(this, g); // the coffee court outside the north gatehouse, plaza doves (Cells/Expand_Riyadh.cs)
            AddSeam("arrival", new Vector3(0f, 0f, -4.6f), 0f);
            AddSeam("plaza", new Vector3(-7.4f, 0f, 2f), 90f);
            SetArrival(new Vector3(0f, 0f, -3.6f), 0f);
            // Open world: one box round the plaza and the run north up the tower's sight line to just past the stamp spot.
            // The plaza keeps its south and west edges; the east parapet, the north wall and the gatehouse doors (shut until
            // the honest arch is walked through) have colliders of their own (RouteColliders, NorthGate).
            AddBounds(new Vector3(5.65f, -0.5f, 81.15f), new Vector3(26.7f, 6f, 171.7f), 6f);
            SetEncounter(gameObject.AddComponent<RiyadhEncounter>());
            Landmarks.MarkReachable(this);
        }
    }

    public class RiyadhEncounter : Encounter
    {
        RiyadhCell R => (RiyadhCell)Cell;
        bool m_Aligned, m_Solved;
        int m_LastArchCheck = -1;

        public override string Objective => !m_Aligned ? "Turn the optical panel to match the registration mark." : "Walk through the arch whose reflection tells the truth.";

        public override void OnEnter(bool firstVisit)
        {
            m_Solved = Game.Campaign.IsVerified(CountryId.SaudiArabia);
            // The north gate stands open once the seal is verified; the stamp is only offered out at the tower while the
            // way there is actually walkable.
            R.Gate?.SetOpen(m_Solved, true);
            Landmarks.MarkReachable(R, R.RouteOpen());
            R.Panel.Changed -= OnTurn; R.Panel.Changed += OnTurn;
            if (m_Solved) { R.Trace.Reveal(false); return; }
            R.Trace.Hide();
            R.Panel.SetIndex(0);
            m_Aligned = false;
            UpdateReflections();
            Narration.I?.Say("sau_01", null, false, null, false, true);
            Game.Passenger?.Despawn();
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit() => R.Panel.Changed -= OnTurn;

        void OnTurn(StepRotator r, int i)
        {
            if (!IsLive || m_Solved) return;
            m_Aligned = i == R.TargetIndex;
            UpdateReflections();
            Hud.I?.SetObjective(Objective, false);
            if (m_Aligned) { AudioDirector.I?.Play("seal_verify", r.transform.position, 0.5f); Hud.I?.ShowToast("The reflections settle. One arch now shows the plaza as it is.", 3f); }
        }

        void UpdateReflections()
        {
            for (int i = 0; i < R.Reflections.Count; i++)
            {
                bool honest = m_Aligned && i == R.HonestArch;
                var c = honest ? new Color(0.9f, 0.75f, 0.6f, 0.35f) : m_Aligned ? new Color(0.15f, 0.12f, 0.2f, 0.85f) : R.Reflections[i].GetColor("_Color");
                R.Reflections[i].SetColor("_Color", c);
            }
        }

        void Update()
        {
            if (!IsLive || m_Solved || Game.Rig == null) return;
            var feet = R.transform.InverseTransformPoint(Game.Rig.Feet);
            for (int i = 0; i < R.Arches.Count; i++)
            {
                var a = R.Arches[i].localPosition;
                bool inside = Mathf.Abs(feet.x - a.x) < 0.9f && Mathf.Abs(feet.z - a.z) < 0.5f;
                if (!inside) { if (m_LastArchCheck == i) m_LastArchCheck = -1; continue; }
                if (m_LastArchCheck == i) continue;
                m_LastArchCheck = i;
                if (m_Aligned && i == R.HonestArch)
                {
                    m_Solved = true;
                    R.Trace.Reveal();
                    Narration.I?.Say("tut_verify", null, true, null, false, true);
                    if (R.Gate != null && !R.Gate.IsOpen)
                    {
                        // The way on opens: the painted doors behind the honest arch swing out onto the city.
                        R.Gate.SetOpen(true, false);
                        AudioDirector.I?.Play("door_open", R.Gate.Blocker.transform.position, 0.8f);
                        Hud.I?.ShowCaption("the gate behind the arch swings open", 3f);
                    }
                }
                else WrongArch();
            }
        }

        void WrongArch()
        {
            // A mistake resets the local puzzle only. Campaign progress is untouched.
            Game.Campaign.falseExits++;
            AudioDirector.I?.Play("pass_sting", Game.Rig.Head.position, 0.6f, 1f, Bus.SFX, true);
            PostFx.I?.Flash(0.5f);
            Hud.I?.ShowToast("A false horizon. The plaza folds back to the start.", 3f);
            StartCoroutine(Reset());
        }

        System.Collections.IEnumerator Reset()
        {
            Game.InputLocks++;
            yield return Fader.I.FadeOut(0.3f);
            R.Panel.SetIndex(Random.Range(1, 8));
            m_Aligned = R.Panel.Index == R.TargetIndex;
            UpdateReflections();
            Game.Rig.PlaceAt(R.Arrival.position, R.Arrival.eulerAngles.y);
            Game.InputLocks = Mathf.Max(0, Game.InputLocks - 1);
            yield return Fader.I.FadeIn(0.4f);
            Hud.I?.SetObjective(Objective, false);
        }
    }
}

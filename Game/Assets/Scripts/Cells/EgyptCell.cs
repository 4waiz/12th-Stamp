using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>Egypt — The Sun Without a Shadow. Exhibition courtyard, pyramids far off, sand drifting upward,
    /// and one shadow that points at the sun. Turning the reflector burns it away; the reflector then feeds
    /// sunlight to any tear opened to Egypt.</summary>
    public class EgyptCell : Cell
    {
        public StepRotator Reflector;
        public LightCone ReflectorBeam;
        public Transform WrongShadow;
        public TraceMark Trace;
        public Vector3 ShadowTargetLocal = new Vector3(3.15f, 0.05f, 5.2f);
        public Vector3 MirrorLocal = new Vector3(-2.5f, 0f, 1.5f);

        protected override void Build()
        {
            Id = CellId.Egypt;
            Lighting = new LightingPreset
            {
                SunColor = new Color(1f, 0.93f, 0.8f), SunIntensity = 2.1f, SunEuler = new Vector3(52f, 32f, 0f), ShadowStrength = 0.9f,
                AmbientSky = new Color(0.62f, 0.68f, 0.78f), AmbientEquator = new Color(0.6f, 0.52f, 0.42f), AmbientGround = new Color(0.45f, 0.36f, 0.26f),
                FogColor = new Color(0.86f, 0.78f, 0.66f), FogDensity = 0.0085f,
                SkyZenith = new Color(0.28f, 0.46f, 0.72f), SkyHorizon = new Color(0.92f, 0.84f, 0.7f), SkyGround = new Color(0.7f, 0.58f, 0.44f),
                SunDiscColor = new Color(1f, 0.95f, 0.85f, 1f), SunSize = 0.0045f, CloudColor = new Color(1f, 0.98f, 0.95f, 0.15f),
                PostExposure = -0.1f, Saturation = 4f, Contrast = 10f, ColorFilter = new Color(1f, 0.97f, 0.92f), Bloom = 0.55f, Vignette = 0.18f
            };
            Ambience = "amb_egypt";
            AddSun();

            var sandstone = Mats.Lit(new Color(0.86f, 0.74f, 0.56f), 0.18f, 0f, "sandstone_blocks");
            var pavers = Mats.Lit(new Color(0.84f, 0.74f, 0.58f), 0.22f, 0f, "plaza_stone");
            var sand = Mats.Lit(new Color(0.9f, 0.76f, 0.55f), 0.12f, 0f, "sand_ripples");
            var cloth = Mats.Lit(new Color(0.78f, 0.72f, 0.62f), 0.15f, 0f, "fabric_linen");
            var wood = Mats.Lit(new Color(0.45f, 0.32f, 0.2f), 0.35f, 0f, "wood_dark");
            var bronze = Mats.Lit(new Color(0.72f, 0.52f, 0.28f), 0.75f, 0.95f, "metal_brushed");
            var mirrorMat = Mats.Lit(new Color(0.9f, 0.88f, 0.82f), 0.98f, 1f);

            var g = Group("Geometry");
            MeshFactory.Floor("Sand", g, new Vector3(0, -0.02f, 8f), new Vector2(60f, 60f), sand, "sand", 0.3f, 2f);
            MeshFactory.Floor("Court", g, new Vector3(0, 0.03f, 0.5f), new Vector2(13.5f, 13f), pavers, "stone", 0.3f, 1.2f);

            // Low exhibition walls (south and sides); the north side opens to the desert.
            MeshFactory.Box("SouthWall", g, new Vector3(0, 1.3f, -6.9f), new Vector3(14.5f, 2.6f, 0.6f), sandstone, true, 1.2f);
            MeshFactory.Box("WestWall", g, new Vector3(-7f, 1.1f, 0f), new Vector3(0.6f, 2.2f, 13.5f), sandstone, true, 1.2f);
            MeshFactory.Box("EastWall", g, new Vector3(7f, 1.1f, -2f), new Vector3(0.6f, 2.2f, 9.5f), sandstone, true, 1.2f);
            for (int i = 0; i < 4; i++) Kit.Column(g, new Vector3(5.6f, 0.03f, -4.5f + i * 3.4f), 0.32f, 3.6f, sandstone);
            // Shade sail and exhibit plinths with draped covers (generic, no sacred objects).
            Kit.Awning(g, new Vector3(-4.6f, 0.03f, -3.2f), 0f, 3.2f, 3.0f, 2.9f, cloth, wood);
            for (int i = 0; i < 3; i++)
            {
                var p = new Vector3(-5.4f + i * 1.6f, 0.03f, -5.2f);
                MeshFactory.Box("Plinth", g, p + Vector3.up * 0.45f, new Vector3(0.9f, 0.9f, 0.9f), sandstone, true, 1f);
                MeshFactory.Box("Drape", g, p + Vector3.up * 1.15f, new Vector3(0.7f, 0.5f, 0.6f), cloth, false, 1f, Quaternion.Euler(0, i * 12f, 0));
            }
            Landmarks.Pyramids(g, new Vector3(40f, -1f, 190f), 18f, Mats.Backdrop(new Color(0.86f, 0.72f, 0.52f), Lighting));

            // The plain pillar and its wrong shadow (pointing toward the sun, south-west).
            MeshFactory.Box("Pillar", g, new Vector3(4.2f, 1.6f, 6.4f), new Vector3(0.5f, 3.2f, 0.5f), sandstone, true, 1f);
            var shadowMat = Mats.UnlitInstance(new Color(0.05f, 0.04f, 0.03f, 0.75f), Tex.SoftDot, true, 1f);
            var shadow = MeshFactory.Quad("WrongShadow", transform, 1.0f, 3.2f, shadowMat, new Vector3(0, 0, 0), Quaternion.Euler(90, 0, 0));
            shadow.transform.localPosition = ShadowTargetLocal;
            shadow.transform.localRotation = Quaternion.Euler(90f, 32f + 180f, 0f);
            shadow.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            WrongShadow = shadow.transform;

            Trace = TraceMark.Create(transform, CountryId.Egypt, ShadowTargetLocal + Vector3.up * 0.02f, Quaternion.identity, 0.55f, true);

            // The reflector: a bronze mirror on a pivot stand.
            var stand = new GameObject("ReflectorStand").transform;
            stand.SetParent(g, false);
            stand.localPosition = MirrorLocal;
            MeshFactory.Cylinder("Base", stand, Vector3.zero, 0.38f, 0.12f, bronze, 18);
            MeshFactory.Cylinder("Post", stand, new Vector3(0, 0.12f, 0), 0.05f, 1.0f, bronze, 10);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(stand, false);
            pivot.localPosition = new Vector3(0, 1.3f, 0);
            var frame = new MeshBuilder();
            frame.AddTorus(Vector3.zero, 0.46f, 0.035f, 36, 8, Quaternion.Euler(90, 0, 0));
            MeshFactory.FromBuilder("Frame", frame, bronze, pivot);
            var mirror = new MeshBuilder();
            mirror.AddDisc(Vector3.zero, Vector3.forward, 0.44f, 36);
            mirror.AddDisc(Vector3.zero, Vector3.back, 0.44f, 36);
            MeshFactory.FromBuilder("Mirror", mirror, mirrorMat, pivot);
            MeshFactory.Box("Yoke", pivot, new Vector3(0, -0.5f, 0), new Vector3(0.95f, 0.06f, 0.06f), bronze, false);
            var col = pivot.gameObject.AddComponent<BoxCollider>();
            col.size = new Vector3(1.0f, 1.0f, 0.2f);
            Reflector = pivot.gameObject.AddComponent<StepRotator>();
            Reflector.Target = pivot;
            Reflector.Axis = Vector3.up;
            Reflector.StepDegrees = 15f;
            Reflector.Steps = 24;
            Reflector.Prompt = "Turn the reflector";
            Reflector.MaxDistance = 2.8f;

            var beamGo = new GameObject("ReflectedSunlight");
            beamGo.transform.SetParent(pivot, false);
            beamGo.transform.localPosition = new Vector3(0, 0, 0.06f);
            beamGo.transform.localRotation = Quaternion.Euler(5f, 0, 0);
            ReflectorBeam = beamGo.AddComponent<LightCone>();
            ReflectorBeam.Length = 9.5f;
            ReflectorBeam.StartRadius = 0.42f;
            ReflectorBeam.EndRadius = 0.9f;
            ReflectorBeam.Sunlight = true;
            ReflectorBeam.OwnerCell = this;
            ReflectorBeam.Build(new Color(1f, 0.85f, 0.6f), 1.3f);
            ReflectorBeam.SetStrength(1f);

            // Sand drifting upward (the one dominant impossible event besides the shadow).
            Kit.Weather(g, new Vector3(0, 0.3f, 2f), new Vector3(16f, 0.2f, 16f), false, new Color(0.95f, 0.82f, 0.6f, 0.65f), 90f, 0.35f, true);

            AddSeam("arrival", new Vector3(0f, 0.03f, -6.5f), 0f);
            AddSeam("court", new Vector3(-6.6f, 0.03f, 2.4f), 90f);
            SetArrival(new Vector3(0f, 0.03f, -5.4f), 0f);
            AddBounds(new Vector3(0, 0, 1f), new Vector3(13.4f, 6f, 15.5f));

            SetEncounter(gameObject.AddComponent<EgyptEncounter>());
        }
    }

    public class EgyptEncounter : Encounter
    {
        EgyptCell E => (EgyptCell)Cell;
        float m_Burn;
        float m_MummyTimer;
        bool m_MummySpawned, m_Revealed, m_LookedAtShadow;
        Material m_ShadowMat;

        public override void OnEnter(bool firstVisit)
        {
            m_ShadowMat = E.WrongShadow.GetComponent<Renderer>().sharedMaterial;
            bool verified = Game.Campaign.IsVerified(CountryId.Egypt);
            E.Reflector.Changed -= OnTurned; E.Reflector.Changed += OnTurned;
            m_MummySpawned = false; m_MummyTimer = 0f; m_Burn = 0f;
            if (verified)
            {
                m_Revealed = true;
                E.WrongShadow.gameObject.SetActive(false);
                E.Trace.Reveal(false);
                Game.Passenger?.Despawn();
                return;
            }
            m_Revealed = false;
            E.WrongShadow.gameObject.SetActive(true);
            m_ShadowMat.SetColor("_Color", new Color(0.05f, 0.04f, 0.03f, 0.75f));
            E.Trace.Hide();
            Narration.I?.Say("egy_01", null, false, null, false, true);
            Game.Passenger?.Despawn();
        }

        public override void OnExit()
        {
            E.Reflector.Changed -= OnTurned;
            if (Game.Passenger != null && Game.Passenger.Visible) Game.Passenger.Despawn();
        }

        void OnTurned(StepRotator r, int index)
        {
            if (!IsLive) return;
            if (!m_MummySpawned && !Game.Campaign.IsVerified(CountryId.Egypt)) m_MummyTimer = Mathf.Max(m_MummyTimer, 14f);
        }

        void Update()
        {
            if (!IsLive || Game.Paused) return;
            if (Game.Campaign.IsVerified(CountryId.Egypt)) return;

            // "That shadow is pointing at the sun."
            if (!m_LookedAtShadow && Game.Rig != null)
            {
                var to = E.WrongShadow.position - Game.Rig.Head.position;
                if (to.magnitude < 10f && Vector3.Angle(Game.Rig.Head.forward, to) < 18f)
                {
                    m_LookedAtShadow = true;
                    Narration.I?.Say("egy_02", null, false, null, false, true);
                }
            }

            // The wrapped Passenger rises out of the sand behind the player (telegraphed, slow, repelled by light).
            m_MummyTimer += Time.deltaTime;
            if (!m_MummySpawned && m_MummyTimer > 18f && Game.Story != null && Game.Story.ThreatAllowed) SpawnMummy();

            // Reflected sunlight on the wrong shadow burns it away and reveals the arrival trace.
            if (!m_Revealed)
            {
                var beam = E.ReflectorBeam.transform;
                var to = E.transform.TransformPoint(E.ShadowTargetLocal) - beam.position;
                to.y = 0;
                var f = beam.forward; f.y = 0;
                bool aligned = Vector3.Angle(f, to) < 8.5f;
                m_Burn = Mathf.MoveTowards(m_Burn, aligned ? 1f : 0f, Time.deltaTime / (aligned ? 1.1f : 3f));
                m_ShadowMat.SetColor("_Color", new Color(0.05f + m_Burn * 0.6f, 0.04f + m_Burn * 0.35f, 0.03f, 0.75f * (1f - m_Burn)));
                if (aligned && Random.value < Time.deltaTime * 3f) AudioDirector.I?.Play("sun_sizzle", E.WrongShadow.position, 0.5f, Random.Range(0.9f, 1.15f));
                if (m_Burn >= 1f)
                {
                    m_Revealed = true;
                    E.WrongShadow.gameObject.SetActive(false);
                    E.Trace.Reveal();
                    Hud.I?.ShowToast("The false shadow burns away. Something was underneath.", 3f);
                    Narration.I?.Say("tut_verify", null, true, null, false, true);
                    PostFx.I?.Flash(0.6f);
                }
            }
        }

        void SpawnMummy()
        {
            m_MummySpawned = true;
            var rig = Game.Rig;
            var behind = rig.Feet - rig.Forward * 5f;
            var local = E.transform.InverseTransformPoint(behind);
            local.x = Mathf.Clamp(local.x, -5.8f, 5.8f);
            local.z = Mathf.Clamp(local.z, -5.6f, 7f);
            local.y = 0.03f;
            var pc = Game.Passenger;
            pc.RetreatPoint = E.transform.TransformPoint(new Vector3(-5.5f, 0.03f, 6.5f));
            pc.RetreatSeconds = 12f;
            pc.Spawn(Cell, local, rig.Head.eulerAngles.y, PassengerState.Rise, Disguise.ExhibitLinen);
            pc.Threatening = true;
            Hud.I?.ShowCaption("linen rustles in the sand behind you", 3.5f);
            AudioDirector.I?.Play("pass_whisper", E.transform.TransformPoint(local) + Vector3.up, 0.9f);
            Narration.I?.Say("tut_danger", null, true, null, false, true);
        }

        public void OnVerified()
        {
            var pc = Game.Passenger;
            if (pc != null && pc.Visible) StartCoroutine(Collapse(pc));
            Hud.I?.ShowToast("Egyptian sunlight will now pour through any tear you open to Egypt.", 4.5f);
        }

        System.Collections.IEnumerator Collapse(PassengerController pc)
        {
            pc.SetState(PassengerState.Retreat);
            yield return new WaitForSeconds(1.2f);
            AudioDirector.I?.Play("pass_creak_3", pc.ChestPosition, 0.8f);
            float t = 0f;
            while (t < 1f && pc.Visible)
            {
                t += Time.deltaTime / 1.2f;
                pc.Body.RiseAmount = 1f - t;
                yield return null;
            }
            pc.Despawn();
        }

        public override void ResetLocal()
        {
            m_MummySpawned = false;
            m_MummyTimer = 6f;
            Game.Passenger?.Despawn();
        }
    }
}

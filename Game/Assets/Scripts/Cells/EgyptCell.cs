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
            var trim = Mats.Lit(new Color(0.92f, 0.82f, 0.64f), 0.2f, 0f, "sandstone_blocks");
            var pavers = Mats.Lit(new Color(0.84f, 0.74f, 0.58f), 0.22f, 0f, "plaza_stone");
            var sand = Mats.Lit(new Color(0.9f, 0.76f, 0.55f), 0.12f, 0f, "sand_ripples");
            var cloth = Mats.Lit(new Color(0.78f, 0.72f, 0.62f), 0.15f, 0f, "fabric_linen");
            var wood = Mats.Lit(new Color(1f, 0.9f, 0.8f), 0.35f, 0f, "wood_dark");
            var bronze = Mats.Lit(new Color(0.82f, 0.6f, 0.32f), 0.72f, 0.95f);
            var mirrorMat = Mats.Lit(new Color(0.9f, 0.88f, 0.82f), 0.98f, 1f);

            var g = Group("Geometry");
            MeshFactory.Floor("Sand", g, new Vector3(0, -0.02f, 8f), new Vector2(60f, 60f), sand, "sand", 0.3f, 2f);
            MeshFactory.Floor("Court", g, new Vector3(0, 0.03f, 0.5f), new Vector2(13.5f, 13f), pavers, "stone", 0.3f, 1.2f);

            // Low exhibition walls (south and sides) at monumental block scale with cavetto cornices; the north side opens
            // to the desert through a pylon gateway that frames the pyramids.
            MonumentKit.Wall(g, "SouthWall", new Vector3(0, 1.3f, -6.9f), new Vector3(14.5f, 2.6f, 0.6f), sandstone, trim);
            MonumentKit.Wall(g, "WestWall", new Vector3(-7f, 1.1f, 0f), new Vector3(0.6f, 2.2f, 13.5f), sandstone, trim);
            MonumentKit.Wall(g, "EastWall", new Vector3(7f, 1.1f, -2f), new Vector3(0.6f, 2.2f, 9.5f), sandstone, trim);
            for (int i = 0; i < 4; i++) MonumentKit.Column(g, new Vector3(5.6f, 0.03f, -4.5f + i * 3.4f), 0.32f, 3.6f, sandstone, trim);
            var paint = new[] { Mats.Lit(new Color(0.2f, 0.32f, 0.5f), 0.2f), Mats.Lit(new Color(0.62f, 0.25f, 0.16f), 0.2f), Mats.Lit(new Color(0.85f, 0.75f, 0.55f), 0.2f), Mats.Lit(new Color(0.24f, 0.42f, 0.32f), 0.2f) };
            var groove = Mats.Lit(new Color(0.55f, 0.43f, 0.3f), 0.15f, 0f, "sandstone_blocks");
            var cedar = Mats.Lit(new Color(0.95f, 0.85f, 0.75f), 0.3f, 0f, "wood_dark");
            var pennantCloth = Mats.Lit(new Color(0.95f, 0.35f, 0.25f), 0.2f, 0f, "fabric_linen");
            MonumentKit.Pylon(g, new Vector3(-2.6f, 0.03f, 8.7f), 0f, 3.2f, 7.2f, 2.4f, sandstone, trim, paint, cedar, pennantCloth, groove);
            var trunk = Mats.Lit(new Color(1f, 0.9f, 0.8f), 0.15f, 0f, "wood_dark");
            var frond = Mats.Lit(new Color(0.45f, 0.62f, 0.3f), 0.25f, 0f, "fabric_linen");
            var deadFrond = Mats.Lit(new Color(0.85f, 0.7f, 0.45f), 0.15f, 0f, "fabric_linen");
            var dates = Mats.Lit(new Color(0.58f, 0.3f, 0.09f), 0.45f);
            foreach (var (pos, h, seed) in new[] { (new Vector3(-8.6f, -0.02f, 3.2f), 7.5f, 1), (new Vector3(-9.4f, -0.02f, -2.4f), 8.4f, 2),
                (new Vector3(8.7f, -0.02f, 4.6f), 7.8f, 3), (new Vector3(8.4f, -0.02f, -4.2f), 6.9f, 4), (new Vector3(3.2f, -0.02f, 10.4f), 8.8f, 5),
                (new Vector3(-7.8f, -0.02f, 9.6f), 7.2f, 6) })
                MonumentKit.Palm(g, pos, h, seed, trunk, frond, deadFrond, dates);
            // Low dunes beyond the walls give the sand depth between the courtyard and the pyramids.
            var duneMat = Mats.Lit(new Color(0.9f, 0.76f, 0.55f), 0.1f, 0f, "sand_ripples");
            foreach (var (c, sz) in new[] { (new Vector3(14f, -1.6f, 26f), new Vector3(30f, 4.5f, 14f)), (new Vector3(-15f, -1.8f, 28f), new Vector3(26f, 5f, 14f)) })
            {
                var dune = MeshFactory.Sphere("Dune", g, c, 1f, duneMat, 24, 12);
                dune.transform.localScale = sz;
                dune.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                // Walkable hills now that the desert is open (the walk to the pyramids passes them).
                dune.AddComponent<MeshCollider>();
                dune.layer = Layers.Floor;
                dune.AddComponent<Surface>().Kind = "sand";
            }
            // Shade sail and exhibit plinths with draped covers (generic, no sacred objects).
            Kit.Awning(g, new Vector3(-4.6f, 0.03f, -3.2f), 0f, 3.2f, 3.0f, 2.9f, cloth, wood);
            for (int i = 0; i < 3; i++)
            {
                var p = new Vector3(-5.4f + i * 1.6f, 0.03f, -5.2f);
                MeshFactory.Box("Plinth", g, p + Vector3.up * 0.45f, new Vector3(0.9f, 0.9f, 0.9f), sandstone, true, 1f);
                MeshFactory.Box("Drape", g, p + Vector3.up * 1.15f, new Vector3(0.7f, 0.5f, 0.6f), cloth, false, 1f, Quaternion.Euler(0, i * 12f, 0));
            }
            Landmarks.Pyramids(g, new Vector3(40f, -1f, 190f), 18f, Mats.Backdrop(new Color(0.86f, 0.72f, 0.52f), Lighting));
            BuildCourtDetail(g, sandstone, trim, wood, bronze);

            // The plain pillar and its wrong shadow (pointing toward the sun, south-west).
            MeshFactory.Box("Pillar", g, new Vector3(4.2f, 1.6f, 6.4f), new Vector3(0.5f, 3.2f, 0.5f), sandstone, true, 3.5f);
            MeshFactory.Box("PillarCap", g, new Vector3(4.2f, 3.27f, 6.4f), new Vector3(0.66f, 0.14f, 0.66f), trim, false, 3.5f);
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
            var standBase = MeshFactory.Cylinder("Base", stand, Vector3.zero, 0.38f, 0.12f, bronze, 18);
            var post = MeshFactory.Cylinder("Post", stand, new Vector3(0, 0.12f, 0), 0.05f, 1.0f, bronze, 10);
            var pivot = new GameObject("Pivot").transform;
            pivot.SetParent(stand, false);
            pivot.localPosition = new Vector3(0, 1.3f, 0);
            // The modelled reflector, scaled so its mirror centre (1.25 m) meets this pivot (1.3 m). Its static Base stays on
            // the stand; its Pivot (turntable, yoke, mirror) moves under the puzzle's pivot. Both turn about the same
            // vertical axis, and the mirror faces the pivot's +Z, where the reflected beam goes.
            var model = Models.Spawn("Reflector", stand, Models.Slots(("TS_Brass", bronze), ("TS_Chrome", mirrorMat)), Vector3.zero, 0f, 1.3f / 1.25f);
            var turning = Models.Child(model, "Pivot");
            if (turning != null)
            {
                turning.SetParent(pivot, true);
                Models.StripVisual(standBase);
                Models.StripVisual(post);
            }
            else
            {
                if (model != null) Object.DestroyImmediate(model);
                var frame = new MeshBuilder();
                frame.AddTorus(Vector3.zero, 0.46f, 0.035f, 36, 8, Quaternion.Euler(90, 0, 0));
                MeshFactory.FromBuilder("Frame", frame, bronze, pivot);
                var mirror = new MeshBuilder();
                mirror.AddDisc(Vector3.zero, Vector3.forward, 0.44f, 36);
                mirror.AddDisc(Vector3.zero, Vector3.back, 0.44f, 36);
                MeshFactory.FromBuilder("Mirror", mirror, mirrorMat, pivot);
                MeshFactory.Box("Yoke", pivot, new Vector3(0, -0.5f, 0), new Vector3(0.95f, 0.06f, 0.06f), bronze, false);
            }
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
            // Open world: out through the pylon gate (or the open east side) across the sand to the pyramids, where the
            // country's stamp waits once the seal is verified. The court walls still frame the puzzle.
            AddBounds(new Vector3(11f, 0, 56f), new Vector3(76f, 8f, 146f), 8f);
            Landmarks.MarkReachable(this);

            SetEncounter(gameObject.AddComponent<EgyptEncounter>());
        }

        /// <summary>
        /// The courtyard as a festival pavilion that has stood in the sun for a season: a processional path to the gate,
        /// two plain obelisks, sand drifted against the walls, fallen drums and rubble, storage jars, a rope line round the
        /// covered exhibits and the pavilion's placard. Nothing sits on the puzzle line (reflector, pillar, wrong shadow).
        /// </summary>
        void BuildCourtDetail(Transform g, Material sandstone, Material trim, Material wood, Material bronze)
        {
            var k = new KitBatch();
            var rnd = new System.Random(52);
            // Processional path: larger pale slabs from the middle of the court to the pylon gate.
            for (int i = 0; i < 8; i++)
                k.Box(trim, new Vector3(-2.6f, 0.04f, -0.6f + i * 1.12f), new Vector3(2.2f, 0.025f, 1.08f), 2f);
            // Rose granite, so they stand out against the sandstone pylon behind them.
            var gilt = Mats.Lit(new Color(0.88f, 0.68f, 0.34f), 0.72f, 0.9f);
            var granite = Mats.Lit(new Color(1f, 0.78f, 0.7f), 0.45f, 0f, "granite_pavers");
            MonumentKit.Obelisk(g, new Vector3(-5.3f, 0.03f, 5.6f), 5.4f, granite, trim, gilt);
            MonumentKit.Obelisk(g, new Vector3(0.6f, 0.03f, 5.6f), 5.4f, granite, trim, gilt);

            // Sand drifted over the paving against the walls and in the corners.
            var drift = Mats.Lit(new Color(0.9f, 0.77f, 0.56f), 0.1f, 0f, "sand_ripples");
            foreach (var (c, sz) in new[]
            {
                (new Vector3(-6.3f, -0.12f, -6.2f), new Vector3(1.6f, 0.42f, 1.3f)), (new Vector3(3.6f, -0.1f, -6.35f), new Vector3(2.4f, 0.36f, 0.9f)),
                (new Vector3(6.3f, -0.1f, -6.1f), new Vector3(1.4f, 0.4f, 1.6f)), (new Vector3(-6.45f, -0.1f, -3.4f), new Vector3(0.8f, 0.3f, 1.6f)),
                (new Vector3(-6.45f, -0.1f, 5.4f), new Vector3(0.9f, 0.34f, 2.0f)), (new Vector3(6.45f, -0.1f, -0.4f), new Vector3(0.7f, 0.28f, 1.4f)),
                (new Vector3(-7.4f, -0.1f, 8.4f), new Vector3(1.8f, 0.5f, 1.4f)), (new Vector3(3.4f, -0.12f, 8.9f), new Vector3(2.2f, 0.45f, 1.1f)),
            })
            {
                var mound = MeshFactory.Sphere("SandDrift", g, c, 1f, drift, 18, 9);
                mound.transform.localScale = sz;
                mound.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            // Rubble along the wall feet and fallen drums on the open east side.
            foreach (var (c, s, yaw) in new[]
            {
                (new Vector3(-6.2f, 0.18f, 3.6f), new Vector3(0.6f, 0.36f, 0.45f), 18f), (new Vector3(-5.9f, 0.12f, 4.3f), new Vector3(0.35f, 0.24f, 0.3f), 40f),
                (new Vector3(4.6f, 0.15f, -6.1f), new Vector3(0.5f, 0.3f, 0.4f), -12f), (new Vector3(6.2f, 0.2f, 7.8f), new Vector3(0.7f, 0.4f, 0.5f), 25f),
                (new Vector3(1.6f, 0.1f, 8.1f), new Vector3(0.4f, 0.2f, 0.35f), 60f), (new Vector3(-6.1f, 0.14f, -5.4f), new Vector3(0.45f, 0.28f, 0.35f), -30f),
            })
                k.Box(sandstone, c, s, Quaternion.Euler((float)rnd.NextDouble() * 8f, yaw, (float)rnd.NextDouble() * 10f), 2f);
            foreach (var (c, yaw, len, r) in new[] { (new Vector3(6.3f, 0f, 6.6f), 15f, 1.1f, 0.38f), (new Vector3(4.9f, 0f, 8.2f), 78f, 0.9f, 0.34f) })
            {
                var rot = Quaternion.Euler(0f, yaw, 90f);
                var axis = rot * Vector3.up;
                k.Of(sandstone).AddCylinder(c + Vector3.up * r - axis * len / 2f, r, len, 16, 2f, rot);
            }

            // Storage jars beside the shade sail.
            var clay = Mats.Lit(new Color(1f, 0.82f, 0.7f), 0.25f, 0f, "plaster_terracotta");
            var jar = new[] { new Vector2(0f, 0f), new Vector2(0.12f, 0f), new Vector2(0.2f, 0.08f), new Vector2(0.28f, 0.3f), new Vector2(0.3f, 0.45f), new Vector2(0.26f, 0.62f), new Vector2(0.16f, 0.76f), new Vector2(0.11f, 0.82f), new Vector2(0.13f, 0.86f), new Vector2(0.12f, 0.88f), new Vector2(0f, 0.88f) };
            foreach (var (c, sc) in new[] { (new Vector3(-6.15f, 0.03f, -1.3f), 1.15f), (new Vector3(-5.6f, 0.03f, -0.75f), 0.9f), (new Vector3(-6.2f, 0.03f, -0.2f), 1.0f) })
            {
                var prof = new Vector2[jar.Length];
                for (int i = 0; i < jar.Length; i++) prof[i] = jar[i] * sc;
                k.Of(clay).AddLathe(c, prof, 16);
            }

            // A rope line in front of the covered exhibits.
            var rope = Mats.Lit(new Color(0.85f, 0.3f, 0.24f), 0.3f, 0f, "fabric_linen");
            float[] posts = { -6.0f, -4.6f, -3.2f, -1.8f };
            foreach (float x in posts)
            {
                k.Of(bronze).AddCylinder(new Vector3(x, 0.03f, -4.25f), 0.03f, 0.92f, 10);
                k.Of(bronze).AddCylinder(new Vector3(x, 0.03f, -4.25f), 0.13f, 0.03f, 14);
                k.Of(bronze).AddSphere(new Vector3(x, 0.98f, -4.25f), 0.045f, 10, 6);
            }
            for (int i = 0; i < posts.Length - 1; i++)
            {
                var path = new System.Collections.Generic.List<Vector3>();
                for (int j = 0; j <= 8; j++)
                {
                    float t = j / 8f;
                    path.Add(new Vector3(Mathf.Lerp(posts[i], posts[i + 1], t), 0.9f - 0.16f * 4f * t * (1f - t), -4.25f));
                }
                k.Of(rope).AddTube(path, 0.018f, 6, false);
            }
            k.Bake(g, "CourtDetail");

            // The pavilion's placard, angled toward the arrival point.
            var sign = new GameObject("PavilionPlacard").transform;
            sign.SetParent(g, false);
            sign.localPosition = new Vector3(1.8f, 0.03f, -4.5f);
            sign.localRotation = Quaternion.Euler(0f, 65f, 0f);
            MeshFactory.Box("Post", sign, new Vector3(0f, 0.5f, 0.05f), new Vector3(0.06f, 1.0f, 0.06f), wood, false);
            MeshFactory.Box("Board", sign, new Vector3(0f, 1.05f, 0f), new Vector3(0.78f, 0.5f, 0.04f), Mats.Lit(new Color(0.7f, 0.6f, 0.52f), 0.4f, 0f, "wood_dark"), false, 1f, Quaternion.Euler(-12f, 0f, 0f));
            var head = TextFactory.World(sign, "PAVILION OF EGYPT", 0.06f, new Color(0.92f, 0.82f, 0.58f), TMPro.TextAlignmentOptions.Center, true, 0.7f);
            head.transform.localPosition = new Vector3(0f, 1.15f, -0.045f);
            head.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            var body = TextFactory.World(sign, "Courtyard of the Sun · festival replica\nPlease keep to the paving", 0.034f, new Color(0.85f, 0.8f, 0.7f), TMPro.TextAlignmentOptions.Center, false, 0.7f);
            body.transform.localPosition = new Vector3(0f, 0.98f, -0.06f);
            body.transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
        }
    }

    public class EgyptEncounter : Encounter
    {
        EgyptCell E => (EgyptCell)Cell;
        float m_Burn;
        float m_MummyTimer;
        bool m_MummySpawned, m_Revealed, m_LookedAtShadow;
        Material m_ShadowMat;

        /// <summary>The false shadow has burned away and the arrival trace shows (guide/state).</summary>
        public bool Revealed => m_Revealed;
        /// <summary>0..1: how far the reflected sun has burned the false shadow (guide/state).</summary>
        public float BurnProgress => m_Burn;
        /// <summary>The wrapped figure has risen out of the sand (guide/state).</summary>
        public bool MummyRisen => m_MummySpawned;
        /// <summary>World cues for the puzzle: reflector glint, smouldering shadow, aim error (see EgyptCues).</summary>
        public EgyptCues Cues { get; private set; }

        public override void OnEnter(bool firstVisit)
        {
            if (Cues == null) Cues = gameObject.AddComponent<EgyptCues>();
            m_ShadowMat = E.WrongShadow.GetComponent<Renderer>().sharedMaterial;
            bool verified = Game.Campaign.IsVerified(CountryId.Egypt);
            E.Reflector.Changed -= OnTurned; E.Reflector.Changed += OnTurned;
            m_MummySpawned = false; m_MummyTimer = 0f; m_Burn = 0f;
            Cues.Setup(E, this);
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
            var local = RiseSpot(rig);
            var pc = Game.Passenger;
            pc.RetreatPoint = E.transform.TransformPoint(new Vector3(-5.5f, 0.03f, 6.5f));
            pc.RetreatSeconds = 12f;
            pc.Spawn(Cell, local, rig.Head.eulerAngles.y, PassengerState.Rise, Disguise.ExhibitLinen);
            pc.Threatening = true;
            Hud.I?.ShowCaption("linen rustles in the sand behind you", 3.5f);
            AudioDirector.I?.Play("pass_whisper", E.transform.TransformPoint(local) + Vector3.up, 0.9f);
            Narration.I?.Say("tut_danger", null, true, null, false, true);
        }

        /// <summary>
        /// Where the wrapped figure rises: about five metres behind the player, inside the court. Clamping "behind" to the
        /// walls could put it on top of a player standing by a wall (with sudden death that killed without warning), so try
        /// a fan of directions and take the one with the most room, preferring behind.
        /// </summary>
        Vector3 RiseSpot(PlayerRig rig)
        {
            var feet = E.transform.InverseTransformPoint(rig.Feet); feet.y = 0f;
            var fwd = E.transform.InverseTransformDirection(rig.Forward); fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
            fwd.Normalize();
            var best = feet; float bestScore = float.MinValue;
            for (int i = 0; i < 12; i++)
            {
                float a = 180f + (i % 2 == 0 ? 1f : -1f) * ((i + 1) / 2) * 30f;
                var p = feet + Quaternion.Euler(0f, a, 0f) * fwd * 5f;
                p.x = Mathf.Clamp(p.x, -5.8f, 5.8f); p.z = Mathf.Clamp(p.z, -5.6f, 7f); p.y = 0f;
                var d = p - feet;
                float score = Mathf.Min(d.magnitude, 4.5f) * 2f - Vector3.Dot(d.normalized, fwd);
                if (score > bestScore) { bestScore = score; best = p; }
            }
            best.y = 0.03f;
            return best;
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

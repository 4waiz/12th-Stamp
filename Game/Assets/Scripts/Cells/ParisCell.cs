using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// France / Paris — The Street That Folds. A rainy Seine embankment on festival night; the signature encounter.
    /// Layout (cell-local, metres): the street runs north (+Z) between Haussmann blocks on the west and the river
    /// parapet on the east. The festival checkpoint (desk under an awning) stands at z = 6 with the Passenger guarding
    /// it. The seam for borrowed light is on the festival poster board north-west of the desk, angled so that from the
    /// approach the tear, the sunbeam and the Passenger are all in one view.
    /// </summary>
    public partial class ParisCell : Cell
    {
        public Transform DeskRoot;
        public TraceMark Trace;
        public SeamAnchor DeskSeam, RiverSeam, ArrivalSeam;
        /// <summary>Where a player naturally stands to open the desk seam with the desk in view (AutoTest and judges).</summary>
        public Transform TearVantage;
        public Vector3 GuardLocal = new Vector3(0f, 0f, 6.75f);
        public Vector3 RetreatLocal = new Vector3(-2.6f, 0f, 10.6f);
        public static readonly Vector3 BoardLocal = new Vector3(-2.2f, 0f, 8.0f);
        public const float BoardYaw = 120f;

        protected override void Build()
        {
            Id = CellId.Paris;
            // Rainy night that still reads: a cool moonlit key raking along the street, warm practicals (lamps, shop
            // windows, the booth and desk lamps), a lighter horizon so roofs and the tower separate from the sky.
            Lighting = new LightingPreset
            {
                SunColor = new Color(0.6f, 0.68f, 0.84f), SunIntensity = 0.55f, SunEuler = new Vector3(40f, -38f, 0f), ShadowStrength = 0.62f,
                AmbientSky = new Color(0.27f, 0.32f, 0.42f), AmbientEquator = new Color(0.21f, 0.23f, 0.28f), AmbientGround = new Color(0.1f, 0.1f, 0.115f),
                FogColor = new Color(0.25f, 0.29f, 0.37f), FogDensity = 0.022f,
                SkyZenith = new Color(0.08f, 0.1f, 0.15f), SkyHorizon = new Color(0.4f, 0.44f, 0.52f), SkyGround = new Color(0.1f, 0.11f, 0.13f),
                SunDiscColor = new Color(0, 0, 0, 0), CloudColor = new Color(0.26f, 0.29f, 0.35f, 0.65f),
                PostExposure = 0.35f, Saturation = -8f, Contrast = 10f, ColorFilter = new Color(0.93f, 0.96f, 1f), Bloom = 0.6f, Vignette = 0.24f
            };
            Ambience = "amb_paris_rain";
            AddSun();

            var cobble = Mats.Lit(new Color(0.95f, 0.95f, 1f), 0.6f, 0f, "cobble_wet");
            var pavers = Mats.Lit(new Color(0.9f, 0.89f, 0.87f), 0.5f, 0f, "granite_pavers");
            var curb = Mats.Lit(new Color(0.95f, 0.94f, 0.92f), 0.4f, 0f, "granite_pavers");
            var puddle = Mats.Lit(new Color(0.03f, 0.035f, 0.045f), 0.96f, 0f);
            var stone = Mats.Lit(new Color(0.8f, 0.76f, 0.68f), 0.3f, 0f, "limestone_blocks");
            var metal = Mats.Lit(new Color(0.06f, 0.07f, 0.07f), 0.55f, 0.8f);
            var wood = Mats.Lit(new Color(1f, 0.92f, 0.85f), 0.45f, 0f, "wood_dark");
            var paper = Mats.Lit(new Color(0.92f, 0.89f, 0.8f), 0.2f, 0f, "paper_ivory");
            var brass = Mats.Lit(new Color(0.75f, 0.58f, 0.3f), 0.6f, 0.9f);
            var canvas = Mats.Lit(new Color(0.85f, 0.3f, 0.26f), 0.3f, 0f, "fabric_linen");
            var facade = new UrbanKit.FacadeMats
            {
                Stone = stone,
                StoneBase = Mats.Lit(new Color(0.7f, 0.66f, 0.59f), 0.3f, 0f, "limestone_blocks"),
                Trim = Mats.Lit(new Color(0.86f, 0.82f, 0.74f), 0.3f, 0f, "limestone_blocks"),
                Zinc = Mats.Lit(new Color(0.85f, 0.9f, 0.97f), 0.55f, 0.6f, "metal_brushed"),
                Iron = Mats.Lit(new Color(0.03f, 0.03f, 0.032f), 0.45f, 0.7f),
                Frame = Mats.Lit(new Color(0.1f, 0.13f, 0.12f), 0.4f, 0f),
                GlassDark = Mats.Lit(new Color(0.04f, 0.05f, 0.065f), 0.92f, 0.2f),
                GlassLit = Mats.Emissive(new Color(0.3f, 0.25f, 0.18f), new Color(0.75f, 0.47f, 0.25f)),
                Chimney = Mats.Lit(new Color(1f, 0.92f, 0.88f), 0.3f, 0f, "brick_london"),
                ShopGlow = Mats.Emissive(new Color(0.24f, 0.17f, 0.11f), new Color(0.3f, 0.18f, 0.085f)),
                ShopLamp = Mats.Emissive(new Color(1f, 0.9f, 0.72f), new Color(2.4f, 1.7f, 0.95f)),
                ShopGlass = Mats.Glass(new Color(0.55f, 0.65f, 0.7f, 0.1f)),
                Goods = new[]
                {
                    Mats.Lit(new Color(0.58f, 0.15f, 0.11f), 0.3f), Mats.Lit(new Color(0.15f, 0.32f, 0.22f), 0.55f),
                    Mats.Lit(new Color(0.86f, 0.79f, 0.62f), 0.25f, 0f, "paper_ivory"), Mats.Lit(new Color(0.2f, 0.25f, 0.44f), 0.35f),
                    Mats.Lit(new Color(0.72f, 0.54f, 0.26f), 0.6f, 0.7f),
                },
                Fascias = new[] { Mats.Lit(new Color(0.09f, 0.2f, 0.16f), 0.45f), Mats.Lit(new Color(0.3f, 0.06f, 0.06f), 0.45f), Mats.Lit(new Color(0.06f, 0.09f, 0.2f), 0.45f) },
                Awnings = new[] { Mats.Lit(new Color(0.85f, 0.28f, 0.24f), 0.25f, 0f, "fabric_linen"), Mats.Lit(new Color(0.35f, 0.7f, 0.5f), 0.25f, 0f, "fabric_linen") },
            };

            var g = Group("Geometry");
            // Street in segments (forward rendering lights each object with its eight nearest lights).
            for (int i = 0; i < 4; i++)
            {
                float z0 = -10f + i * 5.75f;
                MeshFactory.Floor("Street" + i, g, new Vector3(-0.2f, 0, z0 + 2.875f), new Vector2(8.6f, 5.75f), cobble, "stone", 0.3f, 1f);
                MeshFactory.Floor("Pavement" + i, g, new Vector3(-3.55f, 0.12f, z0 + 2.875f), new Vector2(1.5f, 5.75f), pavers, "stone", 0.3f, 1.2f);
            }
            MeshFactory.Box("Curb", g, new Vector3(-2.78f, 0.06f, 1.5f), new Vector3(0.16f, 0.13f, 23f), curb, true, 1f);
            UrbanKit.Puddles(g, new Vector3(0.2f, 0f, 1.5f), new Vector2(5.5f, 19f), 11, 75, puddle);

            // West side: three Haussmann blocks facing the street (front = local -Z, so yaw -90 faces +X). The upper
            // storeys fold slowly inward over the street: the one impossible event the player can watch.
            var rnd = new System.Random(1789);
            var shopNames = new[] { "Café des Voyageurs", "Librairie", "Tabac", "Pâtisserie", "Café du Quai", "Fleuriste" };
            int si = 0;
            for (int i = 0; i < 3; i++)
            {
                float z = -6.4f + i * 7.4f;
                var names = new[] { shopNames[si++ % shopNames.Length], shopNames[si++ % shopNames.Length] };
                UrbanKit.Haussmann(g, "Haussmann" + i, new Vector3(-4.35f, 0.12f, z), -90f, 3, facade, rnd, names, out var upper, out var shopLights);
                var fold = upper.gameObject.AddComponent<FoldingFacade>();
                fold.MaxAngle = 4f + i * 1.6f;
                fold.Phase = i * 1.3f;
                if (shopLights.Count > 0)
                    AddPointLight(g, shopLights[shopLights.Count / 2], new Color(1f, 0.74f, 0.46f), 1.7f, 6f);
            }
            UrbanKit.CafeSet(g, new Vector3(-3.2f, 0.12f, -4.0f), 90f, Mats.Lit(new Color(0.8f, 0.78f, 0.72f), 0.6f, 0f, "marble_white"), metal, Mats.Lit(new Color(0.55f, 0.4f, 0.22f), 0.3f, 0f, "fabric_linen"));
            UrbanKit.CafeSet(g, new Vector3(-3.25f, 0.12f, -2.3f), 70f, Mats.Lit(new Color(0.8f, 0.78f, 0.72f), 0.6f, 0f, "marble_white"), metal, Mats.Lit(new Color(0.55f, 0.4f, 0.22f), 0.3f, 0f, "fabric_linen"));

            // East side: the river parapet with a stone coping, benches, the Seine, the far bank, a bridge, the tower.
            MeshFactory.Box("Parapet", g, new Vector3(3.85f, 0.45f, 1.5f), new Vector3(0.45f, 0.9f, 23f), stone, true, 2.5f);
            MeshFactory.Box("Coping", g, new Vector3(3.85f, 0.94f, 1.5f), new Vector3(0.6f, 0.09f, 23.2f), facade.Trim, false, 2.5f);
            Kit.Bench(g, new Vector3(3.0f, 0, -3.5f), -90f, wood, metal);
            Kit.Water(g, new Vector3(30f, -1.4f, 10f), new Vector2(52f, 90f), new Color(0.05f, 0.07f, 0.08f));
            // The far bank (x > 55) and the river beyond z = 55 are the landmark vista's (Landmarks.cs); the bridge stops short.
            var lampGlow = Mats.Backdrop(new Color(0.9f, 0.7f, 0.45f), Lighting, new Color(1.4f, 0.9f, 0.5f));
            // The bridge at z 38 is a walkable stone bridge now, built with the way to it: BuildRiverWalk (Detail_ParisWalk.cs).
            Landmarks.LatticeTower(g, new Vector3(120f, -2f, 150f), 0.9f, Mats.Backdrop(new Color(0.2f, 0.21f, 0.25f), Lighting), lampGlow);
            // North end: the street continues into an alley mouth (shadow, the retreat point).
            MeshFactory.Box("NorthWall", g, new Vector3(0.6f, 3f, 12.6f), new Vector3(6.8f, 6f, 0.6f), stone, true, 4f);
            MeshFactory.Box("AlleyDark", g, new Vector3(-2.6f, 2f, 12.25f), new Vector3(2.2f, 4f, 0.2f), Mats.Lit(new Color(0.01f, 0.01f, 0.012f), 0.1f), false);

            // The festival verification checkpoint: desk, awning with a string of bulbs, rope line, programmes.
            DeskRoot = Kit.Desk(g, new Vector3(0f, 0f, 6f), 180f, wood, paper, brass, out var ledgerTop, out _).transform;
            Kit.Awning(g, new Vector3(0, 0, 6.2f), 0f, 3.6f, 2.6f, 2.7f, canvas, metal);
            var bulbs = Mats.Emissive(new Color(1f, 0.85f, 0.6f), new Color(2.2f, 1.5f, 0.8f));
            var bulbLine = new MeshBuilder();
            for (int i = 0; i < 9; i++) bulbLine.AddSphere(new Vector3(-1.6f + i * 0.4f, 2.45f - 0.08f * Mathf.Sin(i / 8f * Mathf.PI), 4.95f), 0.035f, 8, 6);
            MeshFactory.FromBuilder("BoothBulbs", bulbLine, bulbs, g).GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            AddPointLight(g, new Vector3(0f, 2.35f, 5.4f), new Color(1f, 0.8f, 0.55f), 1.3f, 5.5f); // lights the guard from the front
            var rope = Mats.Lit(new Color(0.5f, 0.1f, 0.1f), 0.3f);
            foreach (var x in new[] { -1.4f, 1.4f }) MeshFactory.Cylinder("Stanchion", g, new Vector3(x, 0, 4.6f), 0.04f, 0.95f, brass, 10);
            MeshFactory.Box("Rope", g, new Vector3(0, 0.85f, 4.6f), new Vector3(2.8f, 0.03f, 0.03f), rope, false);
            var prints = new[] { Tex.Art("stamp_france"), Tex.Art("stamp_egypt"), Tex.Art("stamp_japan"), Tex.Art("stamp_brazil"), Tex.Art("postcard_1"), Tex.Art("postcard_3") };
            UrbanKit.Litter(g, new Vector3(0.3f, 0f, 3.6f), new Vector2(3.4f, 2.4f), 7, 12, System.Array.FindAll(prints, t => t != null));
            UrbanKit.Banner(g, new Vector3(3.62f, 0.98f, -1.2f), -90f, 2.4f, 0.9f, canvas, metal, "FESTIVAL OF PASSPORTS", "PARIS  ·  CHECKPOINT");

            Trace = TraceMark.Create(transform, CountryId.France, transform.InverseTransformPoint(ledgerTop.position) + Vector3.up * 0.003f, Quaternion.identity, 0.2f, true);
            Trace.Reveal(false);

            Kit.LampPost(g, new Vector3(-2.75f, 0.12f, -2f), new Color(1f, 0.75f, 0.45f), 2.4f, metal, out _, "StreetLamp_Paris");
            Kit.LampPost(g, new Vector3(3.2f, 0f, 2.8f), new Color(1f, 0.75f, 0.45f), 2.4f, metal, out _, "StreetLamp_Paris");
            Kit.LampPost(g, new Vector3(-2.75f, 0.12f, 9.4f), new Color(1f, 0.72f, 0.42f), 1.8f, metal, out _, "StreetLamp_Paris");
            // Festival bunting strung from the building line to the river lamps.
            var cord = Mats.Lit(new Color(0.08f, 0.07f, 0.06f), 0.3f);
            var cloth = new[] { Mats.Lit(new Color(0.9f, 0.3f, 0.25f), 0.25f, 0f, "fabric_linen"), Mats.Lit(new Color(1f, 0.92f, 0.7f), 0.25f, 0f, "fabric_linen"), Mats.Lit(new Color(0.35f, 0.5f, 0.9f), 0.25f, 0f, "fabric_linen"), Mats.Lit(new Color(0.4f, 0.75f, 0.55f), 0.25f, 0f, "fabric_linen") };
            UrbanKit.Bunting(g, new Vector3(-4.2f, 6.4f, -1.0f), new Vector3(3.2f, 3.9f, 2.8f), 0.6f, cord, cloth);
            // A festival flagpole on the parapet carries the second string.
            MeshFactory.Cylinder("FlagPole", g, new Vector3(3.75f, 0.98f, 7.6f), 0.035f, 3.6f, metal, 8, false);
            MeshFactory.Box("Flag", g, new Vector3(3.75f, 4.15f, 8.05f), new Vector3(0.02f, 0.55f, 0.85f), cloth[0], false);
            UrbanKit.Bunting(g, new Vector3(-4.2f, 6.2f, 5.0f), new Vector3(3.75f, 4.5f, 7.6f), 0.7f, cord, cloth);

            // The seam for borrowed light: on the festival poster board, angled across the desk (see class summary).
            var posters = new[] { Tex.Art("postcard_2"), Tex.Art("postcard_1"), Tex.Art("postcard_3"), Tex.Art("postcard_4") };
            UrbanKit.PosterBoard(g, BoardLocal, BoardYaw, wood, metal, paper, posters, "FESTIVAL DES PASSEPORTS", "Pavillons du monde  ·  douze pays  ·  entrée libre");

            // Street furniture: an advertising column, a cast-iron drinking fountain, the booksellers' boxes shut for the
            // night on the parapet, and a building closing the north end (the alley mouth stays dark).
            var iron = Mats.Lit(new Color(0.07f, 0.17f, 0.12f), 0.45f, 0.6f);
            var bills = new[]
            {
                Mats.Lit(new Color(0.86f, 0.8f, 0.66f), 0.2f, 0f, "paper_ivory"), Mats.Lit(new Color(0.62f, 0.16f, 0.12f), 0.25f),
                Mats.Lit(new Color(0.18f, 0.26f, 0.46f), 0.25f), Mats.Lit(new Color(0.8f, 0.6f, 0.22f), 0.25f), Mats.Lit(new Color(0.2f, 0.36f, 0.26f), 0.25f),
            };
            UrbanKit.MorrisColumn(g, new Vector3(2.75f, 0f, 10.4f), iron, Mats.Lit(new Color(0.12f, 0.12f, 0.12f), 0.3f), bills, new System.Random(7),
                new[] { "THÉÂTRE", "CONCERT", "EXPOSITION" });
            UrbanKit.WallaceFountain(g, new Vector3(-3.5f, 0.12f, 2.6f), iron, Mats.Glass(new Color(0.7f, 0.8f, 0.85f, 0.35f)));
            var stall = Mats.Lit(new Color(0.16f, 0.34f, 0.24f), 0.4f, 0.3f);
            foreach (float z in new[] { -8.6f, -6.4f, 4.7f, 9.7f, 11.7f })
                UrbanKit.Bouquiniste(g, new Vector3(3.85f, 0.985f, z), 90f, stall, brass); // fronts face the street
            UrbanKit.Haussmann(g, "HaussmannNorth", new Vector3(1.35f, 0f, 12.27f), 0f, 2, facade, new System.Random(1830), new[] { "Brasserie" }, out _, out var northShop);
            if (northShop.Count > 0) AddPointLight(g, northShop[0], new Color(1f, 0.74f, 0.46f), 1.2f, 5f);

            Kit.Weather(g, new Vector3(0, 7f, 2f), new Vector3(14f, 0.5f, 26f), true, new Color(0.75f, 0.8f, 0.9f, 0.4f), 900f, 9f);
            BuildRiverWalk(g); // open world: the way from the street over the river to the tower's garden

            var boardFwd = Quaternion.Euler(0, BoardYaw, 0) * Vector3.forward;
            DeskSeam = AddSeam("desk", BoardLocal + boardFwd * 0.06f, BoardYaw, 1.45f);
            RiverSeam = AddSeam("river", new Vector3(3.35f, 0f, -0.5f), -90f);
            ArrivalSeam = AddSeam("arrival", new Vector3(0.5f, 0f, -7.6f), 0f);
            TearVantage = new GameObject("TearVantage").transform;
            TearVantage.SetParent(transform, false);
            TearVantage.localPosition = new Vector3(-0.3f, 0f, 2.8f);
            var toBoard = BoardLocal - TearVantage.localPosition; toBoard.y = 0f;
            TearVantage.localRotation = Quaternion.LookRotation(toBoard.normalized, Vector3.up);
            SetArrival(new Vector3(0.6f, 0f, -2.6f), 0f);
            // One box round the street, the river walk, the bridge and the far bank up to the stamp (x -6..104, z -8.25..142,
            // walls from y -2.5 to 9.5 so they hold at street, quay and bank level; the south wall is where it always was).
            AddBounds(new Vector3(49f, -2.5f, 66.875f), new Vector3(110f, 6f, 150.25f), 12f);

            SetEncounter(gameObject.AddComponent<ParisEncounter>());
            Expand_France.Build(this, g); // the Cour du Douzième south of the street (after AddBounds: it shortens the south bound)
        }
    }

    /// <summary>Upper storeys fold slowly inward over the street (scenery moves; the player's horizon never does).</summary>
    public class FoldingFacade : MonoBehaviour
    {
        public float MaxAngle = 8f;
        public float Phase;
        Quaternion m_Base;
        void Start() => m_Base = transform.localRotation;
        void Update()
        {
            float t = 0.5f - 0.5f * Mathf.Cos(Time.time * 0.12f + Phase);
            transform.localRotation = m_Base * Quaternion.Euler(-MaxAngle * t, 0, 0);
        }
    }

    public class ParisEncounter : Encounter
    {
        ParisCell P => (ParisCell)Cell;
        // Read-only state for the guide (Guide/GuideBook.cs).
        public bool Cleared => m_Cleared;            // the guard left the desk while a tear was open
        public bool HandShown => m_HandShown;        // its hand came through the tear
        public float RespawnIn => m_RespawnTimer;    // seconds until it comes back after a seal (< 0: not waiting)
        PassengerHand m_Hand;
        float m_WindowTime;
        bool m_HandShown, m_LightTaught, m_Cleared;
        float m_RespawnTimer = -1f;

        public PassengerHand Hand => m_Hand;

        public override void OnEnter(bool firstVisit)
        {
            m_HandShown = false; m_WindowTime = 0f; m_Cleared = false; m_RespawnTimer = -1f;
            if (m_Hand == null) m_Hand = PassengerHand.Create(transform);
            m_Hand.Detach();
            var tears = Game.Tears;
            tears.SizeChanged -= OnSize; tears.SizeChanged += OnSize;
            tears.Sealed -= OnSealed; tears.Sealed += OnSealed;
            tears.Breached -= OnBreach; tears.Breached += OnBreach;
            tears.Opened -= OnOpened; tears.Opened += OnOpened;

            P.Trace.BlockedReason = () => !m_Cleared && PassengerAtDesk() ? "It is standing at the desk. Drive it away first." : null;
            if (!Game.Campaign.IsVerified(CountryId.France))
            {
                SpawnGuard();
                Narration.I?.Say("par_01", null, false, null, false, true);
                if (Game.Campaign.HasResource(ResourceId.Sunlight))
                    Narration.I?.Say("tut_danger", null, true, null, false, true);
                else
                    Hud.I?.ShowToast("Its back is to the light. Egypt's reflector could help, if it were turned.", 4f);
            }
            else Game.Passenger?.Despawn();
        }

        public override void OnExit()
        {
            var tears = Game.Tears;
            if (tears != null) { tears.SizeChanged -= OnSize; tears.Sealed -= OnSealed; tears.Breached -= OnBreach; tears.Opened -= OnOpened; }
            m_Hand?.Detach();
        }

        void SpawnGuard()
        {
            var pc = Game.Passenger;
            pc.GuardPoint = transform.TransformPoint(P.GuardLocal);
            pc.RetreatPoint = transform.TransformPoint(P.RetreatLocal);
            pc.RetreatSeconds = 26f;
            pc.Spawn(Cell, P.GuardLocal, 180f, PassengerState.Guard, Disguise.RainCoat);
            pc.Threatening = true;
        }

        bool PassengerAtDesk()
        {
            var pc = Game.Passenger;
            if (pc == null || !pc.Visible) return false;
            return Vector3.Distance(pc.transform.position, P.DeskRoot.position) < 2.3f && pc.State != PassengerState.Retreat;
        }

        void OnOpened(Tear t)
        {
            if (!IsLive) return;
            if (t.Destination.Id == CountryId.Egypt && t.Beam != null && !m_LightTaught)
            {
                m_LightTaught = true;
                Narration.I?.Say("tut_light", null, true, null, false, true);
            }
        }

        void OnSize(Tear t, TearSize size)
        {
            if (!IsLive) return;
            if (size == TearSize.Window) Narration.I?.Say("tut_widen", null, true, null, false, true);
        }

        void Update()
        {
            if (!IsLive || Game.Paused) return;
            var tear = Game.Tears.Active;
            var pc = Game.Passenger;

            if (m_RespawnTimer > 0f)
            {
                m_RespawnTimer -= Time.deltaTime;
                if (m_RespawnTimer <= 0f && !Game.Campaign.IsVerified(CountryId.France))
                {
                    pc.Spawn(Cell, P.RetreatLocal, 180f, PassengerState.Retreat, Disguise.RainCoat);
                    pc.RetreatSeconds = 22f;
                    pc.Threatening = true;
                }
            }

            if (tear == null || tear.Closing) { m_WindowTime = 0f; return; }
            if (pc != null && pc.State == PassengerState.Retreat) m_Cleared = true;

            // A wide opening invites it back through the Margin: its hand comes through the tear.
            if (tear.Size >= TearSize.Window) m_WindowTime += Time.deltaTime;
            if (!m_HandShown && !Game.Campaign.IsVerified(CountryId.France) && tear.Size >= TearSize.Window && (m_WindowTime > 3.2f || tear.Contamination > 0.45f))
            {
                m_HandShown = true;
                if (pc != null && pc.Visible)
                {
                    AudioDirector.I?.Play("pass_creak_2", pc.ChestPosition, 0.8f);
                    pc.Despawn();
                }
                m_Hand.Attach(tear);
                tear.SetPassengerPressure(0.6f);
                Narration.I?.Say("par_02", null, false, null, true);
                Narration.I?.Say("tut_seal", null, true, null, false, true);
                AudioDirector.I?.Play("pass_sting", tear.OpeningCenter, 0.75f, 1f, Bus.SFX, true);
            }
            if (m_HandShown && m_Hand.Active)
                m_Hand.SetProgress(Mathf.InverseLerp(0.25f, 1f, tear.Contamination));
        }

        void OnSealed(Tear t)
        {
            if (!IsLive) return;
            if (m_HandShown && m_Hand != null && m_Hand.Active)
            {
                m_Hand.Recoil();
                Hud.I?.ShowToast("Sealed. It's caught on the other side.", 3f);
                Game.Story?.RestoreVitality(0.15f);
                m_Cleared = true;
                m_RespawnTimer = 14f; // it comes back, but gives you time to verify
            }
            m_HandShown = false;
        }

        void OnBreach(Tear t)
        {
            if (!IsLive) return;
            // The hand closes around your wrist: sudden death, then Retry at the embankment.
            Hud.I?.ShowCaption("the hand closes around your wrist", 2.5f);
            m_Hand?.Detach();
            Game.Tears.Close(false);
            m_HandShown = false;
            m_Cleared = false;
            Game.Story?.Kill("pulled through the tear");
        }

        public override void ResetLocal()
        {
            m_Hand?.Detach();
            m_HandShown = false; m_Cleared = false; m_WindowTime = 0f; m_RespawnTimer = -1f;
            if (!Game.Campaign.IsVerified(CountryId.France)) SpawnGuard();
        }
    }
}

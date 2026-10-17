using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>United Kingdom / London — The Minute Before Midnight. The arrival trace only shows in light from another
    /// place; opening a tear is loud. Prepare the passport and open the tear while the clock chime masks it.</summary>
    public class LondonCell : Cell
    {
        public SeamAnchor CheckpointSeam;
        public TraceMark Trace;
        public Transform MinuteHand, BellPivot;
        public Material ChimeGlow;
        public const float ChimeInterval = 16f;
        public const float ChimeWindow = 3.2f;

        protected override void Build()
        {
            Id = CellId.London;
            Lighting = new LightingPreset
            {
                SunColor = new Color(0.45f, 0.52f, 0.7f), SunIntensity = 0.16f, SunEuler = new Vector3(55f, 210f, 0f), ShadowStrength = 0.5f,
                AmbientSky = new Color(0.12f, 0.15f, 0.22f), AmbientEquator = new Color(0.12f, 0.12f, 0.14f), AmbientGround = new Color(0.05f, 0.05f, 0.06f),
                FogColor = new Color(0.12f, 0.15f, 0.2f), FogDensity = 0.032f,
                SkyZenith = new Color(0.02f, 0.03f, 0.07f), SkyHorizon = new Color(0.2f, 0.24f, 0.32f), SkyGround = new Color(0.05f, 0.06f, 0.08f),
                SunDiscColor = new Color(0, 0, 0, 0), CloudColor = new Color(0.2f, 0.24f, 0.3f, 0.6f),
                PostExposure = 0.2f, Saturation = -14f, Contrast = 12f, ColorFilter = new Color(0.9f, 0.95f, 1f), Bloom = 0.65f, Vignette = 0.28f
            };
            Ambience = "amb_london_river";
            AddSun();
            var granite = Mats.Lit(new Color(0.42f, 0.42f, 0.44f), 0.7f, 0f, "granite_pavers");
            var brick = Mats.Lit(new Color(0.55f, 0.38f, 0.3f), 0.3f, 0f, "brick_london");
            var metal = Mats.Lit(new Color(0.08f, 0.09f, 0.09f), 0.6f, 0.85f);
            var wood = Mats.Lit(new Color(0.3f, 0.2f, 0.13f), 0.45f, 0f, "wood_dark");
            var g = Group("Embankment");
            MeshFactory.Floor("Promenade", g, new Vector3(0, 0, 2f), new Vector2(9f, 18f), granite, "stone", 0.3f, 1.2f);
            MeshFactory.Box("RiverWall", g, new Vector3(4.7f, 0.5f, 2f), new Vector3(0.5f, 1f, 18f), granite, true, 1f);
            MeshFactory.Box("BrickWall", g, new Vector3(-4.7f, 2f, 2f), new Vector3(0.6f, 4f, 18f), brick, true, 1.4f);
            Kit.Water(g, new Vector3(40f, -1.4f, 2f), new Vector2(70f, 120f), new Color(0.04f, 0.06f, 0.08f));
            for (int i = 0; i < 3; i++) Kit.LampPost(g, new Vector3(4.2f, 0f, -4f + i * 6f), new Color(0.95f, 0.85f, 0.7f), 2f, metal, out _);
            // Suspended water: rain that hangs in the air.
            var drops = Kit.Weather(g, new Vector3(0, 2f, 2f), new Vector3(9f, 3.5f, 17f), true, new Color(0.75f, 0.85f, 0.95f, 0.5f), 0f, 0f);
            var em = drops.emission; em.rateOverTime = 0f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 900) });
            var main = drops.main; main.startLifetime = 9999f; main.loop = false;
            // The clock tower across the river, its face showing one minute to midnight.
            var faceTex = Tex.Art("stamp_uk");
            Landmarks.ClockTower(g, new Vector3(120f, -2f, 90f), -60f, Mats.Backdrop(new Color(0.16f, 0.15f, 0.14f), Lighting), Mats.Backdrop(new Color(0.95f, 0.85f, 0.55f), Lighting, new Color(1.1f, 0.9f, 0.5f)));

            // A riverside checkpoint kiosk with a large clock face and a bell.
            var kiosk = new GameObject("Checkpoint").transform;
            kiosk.SetParent(g, false);
            kiosk.localPosition = new Vector3(-2.6f, 0, 7.5f);
            MeshFactory.Box("Kiosk", kiosk, new Vector3(0, 1.3f, 0), new Vector3(2.2f, 2.6f, 1.6f), wood, true);
            MeshFactory.Box("KioskRoof", kiosk, new Vector3(0, 2.7f, 0), new Vector3(2.5f, 0.2f, 1.9f), metal, false);
            MeshFactory.Cylinder("ClockBody", kiosk, new Vector3(1.15f, 2.2f, 0), 0.42f, 0.08f, metal, 24, false).transform.localRotation = Quaternion.Euler(0, 0, 90);
            MeshFactory.Make("ClockFace", MeshFactory.QuadMesh(0.76f, 0.76f), Mats.Unlit(new Color(0.95f, 0.92f, 0.82f), faceTex, false, 0.9f), kiosk, new Vector3(1.24f, 2.2f, 0), Quaternion.Euler(0, -90, 0));
            MinuteHand = new GameObject("MinuteHand").transform;
            MinuteHand.SetParent(kiosk, false);
            MinuteHand.localPosition = new Vector3(1.25f, 2.2f, 0);
            MinuteHand.localRotation = Quaternion.Euler(-6f, -90, 0);
            MeshFactory.Box("Hand", MinuteHand, new Vector3(0, 0.16f, -0.005f), new Vector3(0.02f, 0.32f, 0.01f), metal, false);
            BellPivot = new GameObject("BellPivot").transform;
            BellPivot.SetParent(kiosk, false);
            BellPivot.localPosition = new Vector3(0, 3.2f, 0);
            MeshFactory.Frustum("Bell", BellPivot, new Vector3(0, -0.45f, 0), 0.32f, 0.12f, 0.45f, Mats.Lit(new Color(0.65f, 0.5f, 0.28f), 0.7f, 0.9f), 18);
            MeshFactory.Box("BellFrame", kiosk, new Vector3(0, 3.25f, 0), new Vector3(0.9f, 0.06f, 0.06f), metal, false);
            ChimeGlow = Mats.UnlitInstance(new Color(0.95f, 0.85f, 0.55f, 0f), Tex.SoftDot, true, 1.5f);
            ChimeGlow.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            MeshFactory.Quad("ChimeGlow", kiosk, 1.4f, 1.4f, ChimeGlow, new Vector3(1.3f, 2.2f, 0), Quaternion.Euler(0, -90, 0));

            CheckpointSeam = AddSeam("checkpoint", new Vector3(-4.3f, 0f, 3f), 90f);
            AddSeam("arrival", new Vector3(0f, 0f, -6.6f), 0f);
            Trace = TraceMark.Create(transform, CountryId.UnitedKingdom, new Vector3(-1.2f, 0.02f, 3.0f), Quaternion.identity, 0.55f, true);
            SetArrival(new Vector3(0f, 0f, -5.6f), 0f);
            AddBounds(new Vector3(0, 0, 2f), new Vector3(8.6f, 5f, 17f));
            SetEncounter(gameObject.AddComponent<LondonEncounter>());
        }
    }

    public class LondonEncounter : Encounter
    {
        LondonCell L => (LondonCell)Cell;
        float m_Clock;
        bool m_Revealed;

        public override string Objective => "Have your passport ready. Open a tear at the checkpoint seam while the clock chimes, so the rip is masked.";

        public bool Chiming => (m_Clock % LondonCell.ChimeInterval) < LondonCell.ChimeWindow;
        float UntilChime => LondonCell.ChimeInterval - (m_Clock % LondonCell.ChimeInterval);

        public override void OnEnter(bool firstVisit)
        {
            m_Revealed = Game.Campaign.IsVerified(CountryId.UnitedKingdom);
            m_Clock = LondonCell.ChimeWindow + 2f;
            Game.Tears.Opened -= OnTear; Game.Tears.Opened += OnTear;
            Game.Passenger?.Despawn();
            if (m_Revealed) { L.Trace.Reveal(false); return; }
            L.Trace.Hide();
            Narration.I?.Say("uk_01", null, false, null, false, true);
            var pc = Game.Passenger;
            pc.GuardPoint = L.transform.TransformPoint(new Vector3(2.5f, 0f, 9f));
            pc.RetreatPoint = L.transform.TransformPoint(new Vector3(2f, 0f, 10.5f));
            pc.Spawn(Cell, new Vector3(2.5f, 0f, 9f), 200f, PassengerState.Observe, Disguise.ClockCoat);
            pc.Threatening = true;
            pc.WalkSpeed = 0.75f;
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit() => Game.Tears.Opened -= OnTear;

        void OnTear(Tear t)
        {
            if (!IsLive || m_Revealed) return;
            if (t.Anchor != L.CheckpointSeam) return;
            if (Chiming)
            {
                m_Revealed = true;
                L.Trace.Reveal();
                Hud.I?.ShowToast("The chime swallows the sound. Light from elsewhere falls on the promenade.", 3.5f);
                Narration.I?.Say("tut_verify", null, true, null, false, true);
            }
            else
            {
                Hud.I?.ShowToast("The rip echoes across the frozen water. It heard you. Seal it and wait for the chime.", 4f);
                Game.Passenger?.Investigate(t.OpeningCenter, 10f);
                t.SetPassengerPressure(0.7f);
            }
        }

        void Update()
        {
            if (!IsLive) return;
            float before = m_Clock % LondonCell.ChimeInterval;
            m_Clock += Time.deltaTime;
            float after = m_Clock % LondonCell.ChimeInterval;
            if (after < before)
            {
                AudioDirector.I?.Play("clock_chime", L.BellPivot.position, 1f);
                Hud.I?.ShowCaption("the clock chimes", 2.5f);
            }
            // Anticipation: the bell draws back and the minute hand strains in the last seconds before the chime.
            float until = UntilChime;
            float pull = until < 3f ? (3f - until) / 3f : 0f;
            float swing = Chiming ? Mathf.Sin(after * 6f) * 18f * (1f - after / LondonCell.ChimeWindow) : -pull * 14f;
            L.BellPivot.localRotation = Quaternion.Euler(swing, 0, 0);
            L.MinuteHand.localRotation = Quaternion.Euler(-6f + (pull > 0 ? Mathf.Sin(Time.time * 40f) * pull * 1.5f : 0f), -90, 0);
            L.ChimeGlow.SetColor("_Color", new Color(0.95f, 0.85f, 0.55f, Chiming ? 0.55f * (1f - after / LondonCell.ChimeWindow) : pull * 0.15f));
            if (!m_Revealed && pull > 0.01f && until < 2.95f && until > 2.85f) Hud.I?.ShowToast("Get ready...", 1.5f);
        }

        public override void ResetLocal() => Game.Passenger?.Despawn();
    }
}

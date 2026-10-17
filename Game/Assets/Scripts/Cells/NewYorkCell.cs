using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>USA / New York — The Last Arrival. Harbour terminal at night; power the security booth, read its
    /// warning, and use the portable noisemaker to draw the Passenger off the arrival gate.</summary>
    public class NewYorkCell : Cell
    {
        public PushButton Breaker;
        public TMPro.TextMeshPro Screen;
        public GameObject ScreenGlow;
        public Grabbable Noisemaker;
        public Transform Gate;
        public TraceMark Trace;
        public Vector3 GuardLocal = new Vector3(2.2f, 0f, 5.2f);

        protected override void Build()
        {
            Id = CellId.NewYork;
            Lighting = new LightingPreset
            {
                SunColor = new Color(0.45f, 0.5f, 0.68f), SunIntensity = 0.2f, SunEuler = new Vector3(55f, 140f, 0f), ShadowStrength = 0.5f,
                AmbientSky = new Color(0.14f, 0.15f, 0.2f), AmbientEquator = new Color(0.16f, 0.13f, 0.1f), AmbientGround = new Color(0.06f, 0.05f, 0.045f),
                FogColor = new Color(0.12f, 0.12f, 0.15f), FogDensity = 0.03f,
                SkyZenith = new Color(0.03f, 0.04f, 0.08f), SkyHorizon = new Color(0.25f, 0.2f, 0.2f), SkyGround = new Color(0.06f, 0.06f, 0.07f),
                SunDiscColor = new Color(0, 0, 0, 0), Stars = 0.2f, CloudColor = new Color(0.2f, 0.18f, 0.2f, 0.4f),
                PostExposure = 0.2f, Saturation = -8f, Contrast = 12f, ColorFilter = new Color(1f, 0.93f, 0.85f), Bloom = 0.7f, Vignette = 0.27f
            };
            Ambience = "amb_ny_harbour";
            AddSun();
            var asphalt = Mats.Lit(new Color(0.35f, 0.35f, 0.37f), 0.7f, 0f, "asphalt_wet");
            var concrete = Mats.Lit(new Color(0.6f, 0.6f, 0.6f), 0.25f, 0f, "concrete_panels");
            var metal = Mats.Lit(new Color(0.3f, 0.32f, 0.34f), 0.5f, 0.85f, "metal_brushed");
            var paint = Mats.Lit(new Color(0.18f, 0.28f, 0.34f), 0.45f, 0.2f);
            var glass = Mats.Glass(new Color(0.55f, 0.65f, 0.7f, 0.22f));
            var sodium = new Color(1f, 0.62f, 0.3f);
            var g = Group("Terminal");

            MeshFactory.Floor("Apron", g, new Vector3(0, 0, 2f), new Vector2(14f, 16f), asphalt, "stone", 0.3f, 1.2f);
            // Terminal canopy on columns.
            MeshFactory.Box("Canopy", g, new Vector3(0, 4.2f, 3f), new Vector3(12f, 0.3f, 9f), concrete, false, 2f);
            for (int i = 0; i < 4; i++) foreach (var x in new[] { -5.5f, 5.5f })
                MeshFactory.Box("Column", g, new Vector3(x, 2.1f, -1f + i * 2.8f), new Vector3(0.35f, 4.2f, 0.35f), concrete, true);
            MeshFactory.Box("BackWall", g, new Vector3(0, 2.1f, 7.8f), new Vector3(12f, 4.2f, 0.3f), concrete, true, 2f);
            MeshFactory.Box("WestWall", g, new Vector3(-6f, 2.1f, 3f), new Vector3(0.3f, 4.2f, 9.6f), concrete, true, 2f);
            // Harbour edge to the east with a railing, the water and the distant statue.
            Kit.Railing(g, new Vector3(6.5f, 0f, -5f), new Vector3(6.5f, 0f, 9f), 1.05f, metal, 0.18f);
            var rail = new GameObject("RailingCollider"); rail.transform.SetParent(g, false); rail.transform.localPosition = new Vector3(6.5f, 0.6f, 2f); rail.AddComponent<BoxCollider>().size = new Vector3(0.2f, 1.2f, 14f);
            Kit.Water(g, new Vector3(40f, -1.5f, 2f), new Vector2(66f, 140f), new Color(0.04f, 0.05f, 0.07f));
            Landmarks.Liberty(g, new Vector3(150f, -1.5f, -70f), -60f, Mats.Backdrop(new Color(0.16f, 0.2f, 0.2f), Lighting), Mats.Backdrop(new Color(1f, 0.8f, 0.5f), Lighting, new Color(2f, 1.3f, 0.5f)));
            Landmarks.Skyline(g, new Vector3(0, -2f, 0), 210f, 70f, 70f, 14, Mats.Backdrop(new Color(0.1f, 0.1f, 0.13f), Lighting), Mats.Backdrop(new Color(1f, 0.8f, 0.5f), Lighting, new Color(1.2f, 0.8f, 0.45f)), 31);

            // Arrival gate (the objective) and a luggage carousel.
            Gate = new GameObject("ArrivalGate").transform;
            Gate.SetParent(g, false);
            Gate.localPosition = new Vector3(2.2f, 0, 6.2f);
            MeshFactory.Box("GateFrameL", Gate, new Vector3(-1.1f, 1.2f, 0), new Vector3(0.15f, 2.4f, 0.15f), metal, true);
            MeshFactory.Box("GateFrameR", Gate, new Vector3(1.1f, 1.2f, 0), new Vector3(0.15f, 2.4f, 0.15f), metal, true);
            MeshFactory.Box("GateTop", Gate, new Vector3(0, 2.45f, 0), new Vector3(2.4f, 0.2f, 0.2f), metal, false);
            MeshFactory.Box("GateSign", Gate, new Vector3(0, 2.8f, -0.05f), new Vector3(2f, 0.4f, 0.06f), Mats.Emissive(new Color(0.1f, 0.12f, 0.1f), new Color(0.25f, 0.6f, 0.4f)), false);
            var sign = TextFactory.World(Gate, "ARRIVALS", 0.18f, new Color(0.85f, 1f, 0.9f), TMPro.TextAlignmentOptions.Center, false, 2f);
            sign.transform.localPosition = new Vector3(0, 2.8f, -0.09f);
            Trace = TraceMark.Create(transform, CountryId.USA, new Vector3(2.2f, 0.02f, 5.5f), Quaternion.identity, 0.55f, true);
            Trace.Reveal(false);
            var belt = Mats.Lit(new Color(0.08f, 0.08f, 0.09f), 0.4f);
            MeshFactory.Box("Carousel", g, new Vector3(-2.8f, 0.35f, 4.2f), new Vector3(1.4f, 0.7f, 5.5f), metal, true);
            MeshFactory.Box("Belt", g, new Vector3(-2.8f, 0.72f, 4.2f), new Vector3(1.2f, 0.04f, 5.3f), belt, false);
            for (int i = 0; i < 3; i++) Kit.Crate(g, new Vector3(-2.8f, 0.74f, 2.5f + i * 1.6f), i * 20f, new Vector3(0.5f, 0.32f, 0.7f), Mats.Lit(new Color(0.2f, 0.16f, 0.12f), 0.3f, 0f, "leather_navy"));

            // Security booth with breaker, screen and the noisemaker.
            var booth = new GameObject("SecurityBooth").transform;
            booth.SetParent(g, false);
            booth.localPosition = new Vector3(-3.6f, 0, -1.5f);
            MeshFactory.Box("BoothBase", booth, new Vector3(0, 0.5f, 0), new Vector3(1.8f, 1f, 1.4f), paint, true);
            var bg = MeshFactory.Box("BoothGlass", booth, new Vector3(0, 1.7f, 0), new Vector3(1.8f, 1.4f, 1.4f), glass, false);
            bg.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            MeshFactory.Box("BoothRoof", booth, new Vector3(0, 2.45f, 0), new Vector3(2f, 0.1f, 1.6f), paint, false);
            MeshFactory.Box("Monitor", booth, new Vector3(0.3f, 1.25f, -0.55f), new Vector3(0.5f, 0.38f, 0.3f), Mats.Lit(new Color(0.12f, 0.12f, 0.11f), 0.3f), false);
            ScreenGlow = MeshFactory.Quad("ScreenGlow", booth, 0.42f, 0.3f, Mats.Unlit(new Color(0.35f, 0.95f, 0.6f), null, false, 0.9f), new Vector3(0.3f, 1.25f, -0.705f));
            ScreenGlow.SetActive(false);
            Screen = TextFactory.World(booth, "", 0.016f, new Color(0.1f, 0.18f, 0.12f), TMPro.TextAlignmentOptions.TopLeft, false, 0.4f);
            Screen.transform.localPosition = new Vector3(0.3f, 1.27f, -0.71f);
            Screen.rectTransform.sizeDelta = new Vector2(0.4f, 0.28f);
            var breaker = MeshFactory.Box("Breaker", booth, new Vector3(-0.93f, 1.1f, 0.2f), new Vector3(0.08f, 0.3f, 0.2f), Mats.Lit(new Color(0.6f, 0.1f, 0.08f), 0.4f), true);
            Breaker = breaker.AddComponent<PushButton>();
            Breaker.Prompt = "Switch on the booth power";
            Breaker.Sound = "lever_clunk";
            Breaker.OneShot = true;
            var noise = new GameObject("Noisemaker");
            noise.transform.SetParent(booth, false);
            noise.transform.localPosition = new Vector3(-0.35f, 1.04f, -0.45f);
            if (!PropLibrary.NoisemakerModel(noise.transform))
            {
                MeshFactory.Box("Body", noise.transform, new Vector3(0, 0.07f, 0), new Vector3(0.22f, 0.14f, 0.09f), Mats.Lit(new Color(0.5f, 0.35f, 0.2f), 0.4f, 0f, "leather_navy"), false);
                MeshFactory.Box("Grille", noise.transform, new Vector3(-0.04f, 0.07f, -0.047f), new Vector3(0.11f, 0.1f, 0.005f), Mats.Lit(new Color(0.75f, 0.7f, 0.6f), 0.6f, 0.9f, "metal_brushed"), false);
                MeshFactory.Cylinder("Dial", noise.transform, new Vector3(0.06f, 0.07f, -0.05f), 0.025f, 0.01f, Mats.Lit(new Color(0.1f, 0.1f, 0.1f), 0.5f), 12, false).transform.localRotation = Quaternion.Euler(-90, 0, 0);
                MeshFactory.Cylinder("Antenna", noise.transform, new Vector3(0.09f, 0.14f, 0), 0.003f, 0.25f, Mats.Lit(new Color(0.7f, 0.7f, 0.72f), 0.8f, 1f), 6, false);
            }
            noise.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.16f, 0.11f);
            Noisemaker = PropLibrary.MakeGrabbable(noise, "noisemaker", "Take the noisemaker", true);
            Noisemaker.ActivatePrompt = "Switch the noisemaker on/off";
            AddSpotLight(booth, new Vector3(0, 2.3f, 0), new Vector3(90, 0, 0), new Color(0.9f, 0.95f, 1f), 1.2f, 4f, 90f);

            // Sodium lamps.
            for (int i = 0; i < 3; i++) Kit.LampPost(g, new Vector3(5.6f, 0, -3f + i * 4.5f), sodium, 2.4f, metal, out _);
            AddPointLight(g, new Vector3(0, 3.9f, 3f), sodium, 1.6f, 9f, true);

            AddSeam("arrival", new Vector3(-1f, 0f, -5.4f), 0f);
            AddSeam("pier", new Vector3(6.1f, 0f, 3.5f), -90f);
            AddSeam("hall", new Vector3(-5.6f, 0f, 4.8f), 90f);
            SetArrival(new Vector3(-1f, 0f, -4.4f), 10f);
            AddBounds(new Vector3(0, 0, 2f), new Vector3(12.6f, 5f, 15f));
            SetEncounter(gameObject.AddComponent<NewYorkEncounter>());
        }
    }

    public class NewYorkEncounter : Encounter
    {
        NewYorkCell N => (NewYorkCell)Cell;
        AudioSource m_Noise;
        bool m_On, m_Powered;
        float m_InvestigateCooldown;

        public override string Objective => !m_Powered ? "Power the security booth and read its screen." :
            "Use the noisemaker to draw it away from the arrival gate, then verify the trace at the gate.";

        public override void OnEnter(bool firstVisit)
        {
            m_Powered = N.ScreenGlow.activeSelf;
            N.Breaker.Pressed -= OnBreaker; N.Breaker.Pressed += OnBreaker;
            N.Noisemaker.OnActivate = (g, who) => Toggle();
            N.Trace.BlockedReason = () => PassengerNearGate() ? "It is standing in the arrival gate." : null;
            Game.Passenger?.Despawn();
            if (!Game.Campaign.IsVerified(CountryId.USA))
            {
                var pc = Game.Passenger;
                pc.GuardPoint = N.transform.TransformPoint(N.GuardLocal);
                pc.RetreatPoint = N.transform.TransformPoint(new Vector3(-5f, 0f, 7f));
                pc.Spawn(Cell, N.GuardLocal, 180f, PassengerState.Guard, Disguise.LostLuggage);
                pc.Threatening = true;
                pc.WalkSpeed = 0.8f;
            }
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit()
        {
            N.Breaker.Pressed -= OnBreaker;
            if (m_On) Toggle();
        }

        void OnBreaker(PushButton b)
        {
            m_Powered = true;
            AudioDirector.I?.Play("booth_power", b.transform.position, 0.9f);
            N.ScreenGlow.SetActive(true);
            N.Screen.text = "<b>FESTIVAL OF PASSPORTS — ARRIVALS</b>\nAL MANSOORI, S.  .....  ARRIVED\nAL MANSOORI, S.  .....  ARRIVED\n<color=#7a1010>2 PASSENGERS  /  1 PASSPORT</color>";
            Narration.I?.Say("usa_01", null, false, null, false, true);
            Hud.I?.SetObjective(Objective, false);
        }

        void Toggle()
        {
            m_On = !m_On;
            AudioDirector.I?.Play("button_press", N.Noisemaker.transform.position, 0.7f);
            if (m_On) m_Noise = AudioDirector.I?.Loop("noisemaker_loop", N.Noisemaker.transform, 0.9f, Bus.SFX, 1f, 2f, 30f);
            else if (m_Noise != null) { AudioDirector.I?.FadeOutAndStop(m_Noise, 0.2f); m_Noise = null; }
            Hud.I?.ShowCaption(m_On ? "the noisemaker crackles and warbles" : "the noisemaker falls silent", 2f);
        }

        bool PassengerNearGate()
        {
            var pc = Game.Passenger;
            return pc != null && pc.Visible && Vector3.Distance(pc.transform.position, N.Gate.position) < 2.8f && pc.State != PassengerState.Retreat;
        }

        void Update()
        {
            if (!IsLive || Game.Paused) return;
            var pc = Game.Passenger;
            if (pc == null || !pc.Visible) return;
            m_InvestigateCooldown -= Time.deltaTime;
            if (m_On && m_InvestigateCooldown <= 0f && pc.State != PassengerState.Repelled && pc.State != PassengerState.Retreat)
            {
                // It always goes to the sound — including to you, if you are holding it.
                pc.Investigate(N.Noisemaker.transform.position, 14f);
                m_InvestigateCooldown = 1.5f;
            }
        }

        public void OnVerified()
        {
            if (m_On) Toggle();
            Hud.I?.ShowToast("Noisemaker kept. Recall it any time: " + Game.Input.Label(Intent.Recall), 4f);
            var pc = Game.Passenger;
            if (pc != null && pc.Visible) pc.SetState(PassengerState.Retreat);
        }

        public override void ResetLocal()
        {
            if (m_On) Toggle();
            N.Noisemaker.ReturnHome();
        }
    }
}

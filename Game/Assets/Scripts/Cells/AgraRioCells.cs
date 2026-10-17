using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>India / Agra — The Reflection That Stayed. A garden checkpoint at dawn; the pool reflects one person too
    /// many. Set the lamp where its reflection exposes the genuine trace. (No sacred objects are used as tools.)</summary>
    public class AgraCell : Cell
    {
        public readonly List<Socket> Pedestals = new List<Socket>();
        public Grabbable Lamp;
        public TraceMark Trace;
        public Material ExtraPersonMat;
        public Transform ExtraPerson;
        public int CorrectPedestal = 1;

        protected override void Build()
        {
            Id = CellId.Agra;
            Lighting = new LightingPreset
            {
                SunColor = new Color(1f, 0.78f, 0.65f), SunIntensity = 0.9f, SunEuler = new Vector3(10f, 70f, 0f), ShadowStrength = 0.6f,
                AmbientSky = new Color(0.55f, 0.52f, 0.62f), AmbientEquator = new Color(0.55f, 0.45f, 0.42f), AmbientGround = new Color(0.25f, 0.24f, 0.2f),
                FogColor = new Color(0.85f, 0.72f, 0.7f), FogDensity = 0.016f,
                SkyZenith = new Color(0.45f, 0.55f, 0.75f), SkyHorizon = new Color(0.98f, 0.8f, 0.72f), SkyGround = new Color(0.55f, 0.5f, 0.45f),
                SunDiscColor = new Color(1f, 0.85f, 0.7f, 0.8f), SunSize = 0.006f, CloudColor = new Color(1f, 0.85f, 0.8f, 0.25f),
                PostExposure = -0.05f, Saturation = -6f, Contrast = 6f, ColorFilter = new Color(1f, 0.96f, 0.95f), Bloom = 0.6f, Vignette = 0.2f
            };
            Ambience = "amb_india_garden";
            AddSun();
            var marble = Mats.Lit(new Color(0.92f, 0.9f, 0.88f), 0.6f, 0f, "marble_white");
            var grass = Mats.Lit(new Color(0.55f, 0.65f, 0.45f), 0.15f, 0f, "grass_short");
            var brass = Mats.Lit(new Color(0.78f, 0.6f, 0.32f), 0.7f, 0.9f, "metal_brushed");
            var cypress = Mats.Lit(new Color(0.12f, 0.22f, 0.14f), 0.2f);
            var g = Group("Garden");
            MeshFactory.Floor("Walk", g, new Vector3(0, 0, 4f), new Vector2(9f, 22f), marble, "stone", 0.3f, 1.2f);
            MeshFactory.Floor("LawnL", g, new Vector3(-6.5f, -0.02f, 4f), new Vector2(4f, 22f), grass, "grass", 0.3f, 1.5f);
            MeshFactory.Floor("LawnR", g, new Vector3(6.5f, -0.02f, 4f), new Vector2(4f, 22f), grass, "grass", 0.3f, 1.5f);
            MeshFactory.Box("PoolRim", g, new Vector3(0, 0.08f, 5f), new Vector3(2.6f, 0.16f, 16.6f), marble, true);
            var pool = Kit.Water(g, new Vector3(0, 0.1f, 5f), new Vector2(2.2f, 16f), new Color(0.35f, 0.42f, 0.5f), 0.97f);
            pool.layer = Layers.Default;
            for (int i = 0; i < 6; i++) foreach (var x in new[] { -5.2f, 5.2f })
                MeshFactory.Frustum("Cypress", g, new Vector3(x, 0, -3f + i * 3.4f), 0.55f, 0.05f, 3.6f, cypress, 10, true);
            Landmarks.DomedPavilion(g, new Vector3(0, -2f, 230f), 0f, Mats.Backdrop(new Color(0.93f, 0.9f, 0.9f), Lighting));
            // The extra reflected person: a figure that exists only below the water surface.
            ExtraPersonMat = Mats.UnlitInstance(new Color(0.05f, 0.06f, 0.07f, 0.45f), Tex.SoftDot, true, 1f);
            var ep = MeshFactory.Quad("ExtraReflection", g, 0.5f, 1.8f, ExtraPersonMat, new Vector3(0.3f, 0.105f, 8.5f), Quaternion.Euler(90, 0, 180));
            ep.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ExtraPerson = ep.transform;
            // Suspended petals (frozen in the air).
            var petals = Kit.Weather(g, new Vector3(0, 1.6f, 4f), new Vector3(9f, 2.5f, 18f), false, new Color(0.98f, 0.72f, 0.75f, 0.9f), 0f, 0f);
            var em = petals.emission; em.rateOverTime = 0f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 350) });
            var main = petals.main; main.startLifetime = 9999f; main.loop = false;
            Trace = TraceMark.Create(transform, CountryId.India, new Vector3(1.55f, 0.17f, 8.5f), Quaternion.identity, 0.5f, true);

            for (int i = 0; i < 3; i++)
            {
                var p = new Vector3(2.4f, 0, 2f + i * 3.2f);
                MeshFactory.Box("Pedestal", g, p + Vector3.up * 0.45f, new Vector3(0.45f, 0.9f, 0.45f), marble, true);
                var sock = new GameObject("PedestalTop" + i);
                sock.transform.SetParent(g, false);
                sock.transform.localPosition = p + Vector3.up * 0.92f;
                var col = sock.AddComponent<BoxCollider>(); col.isTrigger = true; col.size = new Vector3(0.45f, 0.3f, 0.45f); col.center = new Vector3(0, 0.12f, 0);
                var s = sock.AddComponent<Socket>();
                s.Accepts.Add("garden_lamp");
                s.Prompt = "Set the lamp down";
                Pedestals.Add(s);
            }
            var lamp = new GameObject("GardenLamp");
            lamp.transform.SetParent(g, false);
            lamp.transform.localPosition = new Vector3(-1.9f, 0.02f, -1.2f);
            var flame = Mats.Emissive(new Color(1f, 0.8f, 0.5f), new Color(3f, 1.6f, 0.6f));
            // The modelled brass oil lamp: its flame burns inside a clear glass chimney, under the existing lamp light.
            var chimney = Mats.Glass(new Color(0.85f, 0.9f, 0.88f, 0.16f));
            if (Models.Spawn("GardenLamp", lamp.transform, Models.Slots(("TS_Brass", brass), ("TS_EmissiveWarm", flame), ("TS_Glass", chimney))) == null)
            {
                MeshFactory.Cylinder("Base", lamp.transform, Vector3.zero, 0.09f, 0.04f, brass, 14, false);
                MeshFactory.Frustum("Bowl", lamp.transform, new Vector3(0, 0.04f, 0), 0.05f, 0.11f, 0.08f, brass, 14);
                MeshFactory.Sphere("Flame", lamp.transform, new Vector3(0, 0.15f, 0), 0.03f, flame);
            }
            var ll = new GameObject("LampLight").AddComponent<Light>();
            ll.transform.SetParent(lamp.transform, false); ll.transform.localPosition = new Vector3(0, 0.2f, 0);
            ll.type = LightType.Point; ll.color = new Color(1f, 0.7f, 0.4f); ll.intensity = 1.4f; ll.range = 4f; ll.shadows = LightShadows.None;
            lamp.AddComponent<BoxCollider>().size = new Vector3(0.24f, 0.22f, 0.24f);
            Lamp = PropLibrary.MakeGrabbable(lamp, "garden_lamp", "Take the lamp");

            AddSeam("arrival", new Vector3(0f, 0f, -5.6f), 0f);
            AddSeam("lawn", new Vector3(-4.4f, 0f, 6f), 90f);
            SetArrival(new Vector3(0f, 0f, -4.6f), 0f);
            AddBounds(new Vector3(0, 0, 4f), new Vector3(16f, 5f, 21f));
            SetEncounter(gameObject.AddComponent<AgraEncounter>());
        }
    }

    public class AgraEncounter : Encounter
    {
        AgraCell A => (AgraCell)Cell;
        bool m_Solved;

        public override string Objective => "Set the lamp on the pedestal where its reflection shows what the extra person is standing on.";

        public override void OnEnter(bool firstVisit)
        {
            m_Solved = Game.Campaign.IsVerified(CountryId.India);
            foreach (var p in A.Pedestals) { p.Filled -= OnPlaced; p.Filled += OnPlaced; }
            Game.Passenger?.Despawn();
            if (m_Solved) { A.Trace.Reveal(false); A.ExtraPerson.gameObject.SetActive(false); return; }
            A.Trace.Hide();
            A.ExtraPerson.gameObject.SetActive(true);
            Narration.I?.Say("ind_01", null, false, null, false, true);
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit()
        {
            foreach (var p in A.Pedestals) p.Filled -= OnPlaced;
        }

        void OnPlaced(Socket s, Grabbable g)
        {
            if (!IsLive || m_Solved) return;
            if (A.Pedestals.IndexOf(s) == A.CorrectPedestal)
            {
                m_Solved = true;
                A.Trace.Reveal();
                Hud.I?.ShowToast("In the lamp's reflection, a mark glows where the extra person stood.", 3.5f);
                StartCoroutine(StepOut());
            }
            else
            {
                Hud.I?.ShowCaption("the reflected figure turns its head toward the lamp", 3f);
                AudioDirector.I?.Play("pass_creak_2", A.ExtraPerson.position, 0.6f);
            }
        }

        System.Collections.IEnumerator StepOut()
        {
            // The reflection stays... then it is no longer only a reflection.
            float t = 0f;
            while (t < 1f) { t += Time.deltaTime / 2f; A.ExtraPersonMat.SetColor("_Color", new Color(0.05f, 0.06f, 0.07f, 0.45f * (1f - t))); yield return null; }
            A.ExtraPerson.gameObject.SetActive(false);
            var pc = Game.Passenger;
            pc.RetreatPoint = A.transform.TransformPoint(new Vector3(0f, 0f, 15f));
            pc.Spawn(Cell, new Vector3(-1.6f, 0f, 9.4f), 180f, PassengerState.Rise, Disguise.PetalWet);
            pc.Threatening = true;
            pc.WalkSpeed = 0.55f;
            AudioDirector.I?.Play("pass_breath", pc.ChestPosition, 0.8f);
            Hud.I?.ShowCaption("water drips onto marble", 2.5f);
        }

        public override void ResetLocal()
        {
            A.Lamp.ReturnHome();
            Game.Passenger?.Despawn();
        }
    }

    /// <summary>Brazil / Rio — The Carnival Without Breathing. Everything is frozen except one drum. Stamp only on the
    /// beat so the drum masks the sound from the float's moving shadow. Off-beat stamps are recoverable mistakes.</summary>
    public class RioCell : Cell
    {
        public Transform Beater, Ring, FloatShadow, Plate;
        public Material RingMat;
        public Interactable StampPlate;
        public const float BeatInterval = 1.25f;
        public TraceMark Trace;

        protected override void Build()
        {
            Id = CellId.Rio;
            Lighting = new LightingPreset
            {
                SunColor = new Color(0.5f, 0.5f, 0.7f), SunIntensity = 0.15f, SunEuler = new Vector3(60f, 30f, 0f), ShadowStrength = 0.5f,
                AmbientSky = new Color(0.14f, 0.12f, 0.2f), AmbientEquator = new Color(0.18f, 0.12f, 0.12f), AmbientGround = new Color(0.06f, 0.05f, 0.05f),
                FogColor = new Color(0.12f, 0.09f, 0.14f), FogDensity = 0.028f,
                SkyZenith = new Color(0.03f, 0.03f, 0.08f), SkyHorizon = new Color(0.25f, 0.15f, 0.25f), SkyGround = new Color(0.06f, 0.05f, 0.07f),
                SunDiscColor = new Color(0, 0, 0, 0), Stars = 0.3f,
                PostExposure = 0.25f, Saturation = -18f, Contrast = 12f, ColorFilter = new Color(1f, 0.95f, 0.95f), Bloom = 0.8f, Vignette = 0.3f
            };
            Ambience = "amb_rio_frozen";
            AddSun();
            var asphalt = Mats.Lit(new Color(0.3f, 0.3f, 0.32f), 0.5f, 0f, "asphalt_wet");
            var concrete = Mats.Lit(new Color(0.55f, 0.55f, 0.55f), 0.25f, 0f, "concrete_panels");
            var g = Group("Avenue");
            MeshFactory.Floor("Avenue", g, new Vector3(0, 0, 4f), new Vector2(10f, 22f), asphalt, "stone", 0.3f, 1.5f);
            // Bleachers on both sides.
            for (int side = -1; side <= 1; side += 2)
                for (int step = 0; step < 5; step++)
                    MeshFactory.Box("Bleacher", g, new Vector3(side * (5.6f + step * 0.6f), 0.25f + step * 0.5f, 4f), new Vector3(0.6f, 0.5f + step * 1f, 22f), concrete, true, 1.5f);
            Landmarks.Skyline(g, new Vector3(0, -6f, 0), 200f, 80f, 0f, 12, Mats.Backdrop(new Color(0.08f, 0.08f, 0.12f), Lighting), Mats.Backdrop(new Color(1f, 0.8f, 0.55f), Lighting, new Color(1f, 0.7f, 0.4f)), 51);
            // Floodlights on poles.
            foreach (var x in new[] { -4.6f, 4.6f })
                for (int i = 0; i < 2; i++)
                {
                    MeshFactory.Cylinder("Pole", g, new Vector3(x, 0, 0f + i * 9f), 0.1f, 6f, concrete, 8);
                    AddSpotLight(g, new Vector3(x, 6f, 0f + i * 9f), new Vector3(70f, x < 0 ? 60f : -60f, 0f), new Color(1f, 0.85f, 0.7f), 3f, 14f, 60f);
                }
            // The parade float: a desaturated platform with stylised feathers/fans and the one running drum.
            var floatRoot = new GameObject("Float").transform;
            floatRoot.SetParent(g, false);
            floatRoot.localPosition = new Vector3(0, 0, 9f);
            var colors = new[] { new Color(0.6f, 0.45f, 0.3f), new Color(0.35f, 0.5f, 0.42f), new Color(0.52f, 0.4f, 0.55f) };
            MeshFactory.Box("Platform", floatRoot, new Vector3(0, 0.6f, 0), new Vector3(4.6f, 1.2f, 3.4f), Mats.Lit(colors[0], 0.4f), true);
            for (int i = 0; i < 7; i++)
            {
                var fan = MeshFactory.Box("Fan", floatRoot, new Vector3(-2f + i * 0.66f, 2.4f, 1.2f), new Vector3(0.45f, 2.2f, 0.06f), Mats.Lit(colors[i % 3], 0.5f), false, 1f, Quaternion.Euler(0, 0, -30f + i * 10f));
            }
            var drum = MeshFactory.Cylinder("Drum", floatRoot, new Vector3(1.2f, 1.2f, -0.6f), 0.45f, 0.6f, Mats.Lit(new Color(0.7f, 0.66f, 0.6f), 0.4f), 18, true);
            Beater = new GameObject("Beater").transform;
            Beater.SetParent(floatRoot, false);
            Beater.localPosition = new Vector3(1.2f, 2.3f, -0.6f);
            MeshFactory.Box("Arm", Beater, new Vector3(0, -0.3f, 0), new Vector3(0.05f, 0.6f, 0.05f), concrete, false);
            MeshFactory.Sphere("Head", Beater, new Vector3(0, -0.62f, 0), 0.07f, Mats.Lit(new Color(0.2f, 0.18f, 0.16f), 0.4f));
            // Frozen confetti.
            var confetti = Kit.Weather(g, new Vector3(0, 2f, 4f), new Vector3(9f, 3f, 18f), false, new Color(0.9f, 0.75f, 0.55f, 0.9f), 0f, 0f);
            var em = confetti.emission; em.rateOverTime = 0f; em.SetBursts(new[] { new ParticleSystem.Burst(0f, 500) });
            var main = confetti.main; main.startLifetime = 9999f; main.loop = false; main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.8f, 0.5f, 0.35f), new Color(0.45f, 0.6f, 0.75f));

            // The stamp plate with its rhythm ring.
            Plate = new GameObject("StampPlate").transform;
            Plate.SetParent(g, false);
            Plate.localPosition = new Vector3(-1.2f, 0f, 7.0f);
            MeshFactory.Box("PlateBase", Plate, new Vector3(0, 0.5f, 0), new Vector3(0.7f, 1f, 0.5f), concrete, true);
            MeshFactory.Box("Plate", Plate, new Vector3(0, 1.02f, 0), new Vector3(0.5f, 0.04f, 0.35f), Mats.Lit(new Color(0.76f, 0.58f, 0.3f), 0.7f, 0.9f), false);
            RingMat = Mats.UnlitInstance(new Color(0.95f, 0.85f, 0.6f, 0.8f), Tex.Ring, true, 1.4f);
            Ring = MeshFactory.Quad("RhythmRing", Plate, 0.6f, 0.6f, RingMat, new Vector3(0, 1.05f, 0), Quaternion.Euler(90, 0, 0)).transform;
            var col = Plate.gameObject.AddComponent<BoxCollider>(); col.center = new Vector3(0, 1.05f, 0); col.size = new Vector3(0.6f, 0.2f, 0.45f);
            StampPlate = Plate.gameObject.AddComponent<Interactable>();
            StampPlate.Prompt = "Stamp (on the drum beat)";
            Trace = TraceMark.Create(transform, CountryId.Brazil, Plate.localPosition + new Vector3(0, 1.045f, 0), Quaternion.identity, 0.3f, true);
            // The float's shadow that moves on its own.
            FloatShadow = MeshFactory.Quad("FloatShadow", g, 2.6f, 1.6f, Mats.UnlitInstance(new Color(0f, 0f, 0f, 0.65f), Tex.SoftDot, true, 1f), new Vector3(0, 0.013f, 4f), Quaternion.Euler(90, 0, 0)).transform;

            AddSeam("arrival", new Vector3(0f, 0f, -5.6f), 0f);
            AddSeam("stand", new Vector3(4.6f, 0f, 3f), -90f);
            SetArrival(new Vector3(0f, 0f, -4.6f), 0f);
            AddBounds(new Vector3(0, 0, 4f), new Vector3(9f, 5f, 20f));
            SetEncounter(gameObject.AddComponent<RioEncounter>());
        }
    }

    public class RioEncounter : Encounter
    {
        RioCell R => (RioCell)Cell;
        float m_Clock;
        int m_Hits;
        bool m_Done;
        float m_ShadowTarget;
        float m_Alert;

        public override string Objective => "Stamp the plate only on the drum beat. Three masked stamps verify the seal (" + m_Hits + "/3).";

        public override void OnEnter(bool firstVisit)
        {
            m_Done = Game.Campaign.IsVerified(CountryId.Brazil);
            R.StampPlate.OnUse = OnStamp;
            R.StampPlate.Usable = !m_Done;
            Game.Tears.Opened -= OnTear; Game.Tears.Opened += OnTear;
            Game.Passenger?.Despawn();
            if (m_Done) { R.Trace.Reveal(false); return; }
            R.Trace.Hide();
            m_Hits = 0;
            Narration.I?.Say("bra_01", null, false, null, false, true);
            Hud.I?.SetObjective(Objective, false);
        }

        public override void OnExit() => Game.Tears.Opened -= OnTear;

        float Phase => (m_Clock % RioCell.BeatInterval) / RioCell.BeatInterval;
        public bool OnBeat => Phase < 0.14f || Phase > 0.9f;

        void OnTear(Tear t)
        {
            if (!IsLive || m_Done) return;
            if (!OnBeat) Alert("The rip carries across the silent avenue.");
        }

        void OnStamp(Interactor who)
        {
            if (m_Done) return;
            if (OnBeat)
            {
                m_Hits++;
                AudioDirector.I?.Play("stamp_hit", R.Plate.position, 0.35f);
                AudioDirector.Haptic(who != null ? who.Side : HandSide.Desktop, 0.5f, 0.06f);
                Hud.I?.ShowToast("Masked by the drum. (" + m_Hits + "/3)", 1.4f);
                if (m_Hits >= 3)
                {
                    m_Done = true;
                    R.StampPlate.Usable = false;
                    Game.Story.VerifyCountry(CountryId.Brazil, null);
                }
            }
            else
            {
                AudioDirector.I?.Play("stamp_hit", R.Plate.position, 1f, 0.9f, Bus.SFX, true);
                m_Hits = 0;
                Alert("Too early. The stamp echoes, and the float's shadow turns toward you.");
            }
            Hud.I?.SetObjective(Objective, false);
        }

        void Alert(string text)
        {
            m_Alert = 5f;
            Hud.I?.ShowToast(text, 3f);
            if (Game.Settings.scareIntensity > 0) AudioDirector.I?.Play("pass_sting", R.FloatShadow.position, 0.5f, 1f, Bus.SFX, true);
        }

        void Update()
        {
            if (!IsLive) return;
            float dt = Time.deltaTime;
            float before = m_Clock % RioCell.BeatInterval;
            m_Clock += dt;
            float after = m_Clock % RioCell.BeatInterval;
            if (after < before)
            {
                AudioDirector.I?.Play("drum_hit", R.Beater.position, 1f);
                if (Game.Settings.reducedEffects == false) PostFx.I?.Flash(0.05f);
            }
            // Beater swing and the shrinking ring cue (closes exactly on the beat).
            R.Beater.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-35f, 0f, Mathf.Pow(Phase, 4f)));
            float ringScale = Mathf.Lerp(2.2f, 0.6f, Phase);
            R.Ring.localScale = Vector3.one * ringScale;
            R.RingMat.SetColor("_Color", OnBeat ? new Color(0.6f, 1f, 0.8f, 0.95f) : new Color(0.95f, 0.85f, 0.6f, 0.6f));
            // The shadow drifts on its own; after an alert it slides toward the player, then loses interest.
            m_Alert = Mathf.Max(0f, m_Alert - dt);
            var target = m_Alert > 0f && Game.Rig != null ? R.transform.InverseTransformPoint(Game.Rig.Feet) : new Vector3(Mathf.Sin(Time.time * 0.21f) * 2.5f, 0f, 6f + Mathf.Cos(Time.time * 0.17f) * 3f);
            var p = R.FloatShadow.localPosition;
            p = Vector3.MoveTowards(p, new Vector3(target.x, 0.013f, target.z), dt * (m_Alert > 0f ? 1.2f : 0.4f));
            R.FloatShadow.localPosition = p;
            if (m_Alert > 0f && Game.Rig != null && Vector3.Distance(R.FloatShadow.position, Game.Rig.Feet) < 1.1f)
                Game.Story.DrainVitality(0.08f * dt);
        }
    }
}

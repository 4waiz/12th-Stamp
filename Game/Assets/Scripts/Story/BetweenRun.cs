using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace TwelfthStamp
{
    /// <summary>
    /// The Between: where the glasses throw Saeed first, on the way into the Margin. A stone causeway hangs in a red void
    /// over a glowing abyss; pieces of his apartment drift past; the landmarks of the countries ahead hang huge and tilted
    /// in the dark. Something runs after him, and its shadow reaches him first. Three pieces of his home stamp hang over the
    /// causeway; with all three, the seal at the far end opens and throws him into the Margin copy of the apartment.
    /// Built inside the host cell but 3 km from its play area (past the camera's far plane), so nothing of the apartment
    /// shows. The runner is its own body (PassengerBody), not the Passenger: no drain and no death here. Caught, Saeed is
    /// thrown back to the start and keeps his pieces; after two catches the runner tires. Gentle: it never closes on a
    /// walking player. The camera is never moved except by the player (and by the fade-through when caught).
    /// </summary>
    public class BetweenRun : MonoBehaviour
    {
        public static BetweenRun Current { get; private set; }
        public const int Pieces = 3;
        public const float Length = 150f, Width = 5f;
        static readonly Vector3 s_Offset = new Vector3(0f, 0f, 3000f);
        static readonly Color s_Void = new Color(0.07f, 0.008f, 0.016f);

        public int Collected { get; private set; }
        public bool Finished { get; private set; }
        public int Catches { get; private set; }
        /// <summary>In test mode the player is carried along the causeway unless this is turned off (the review does).</summary>
        public bool AutoPilot = true;
        public Transform Root => transform;
        public PassengerBody Runner => m_Ghost;
        public StampPortal Portal => m_Portal;
        public IReadOnlyList<StampPortal> Stamps => m_Stamps;

        Cell m_Host;
        PassengerBody m_Ghost;
        Transform m_Shadow;
        Material m_ShadowMat;
        StampPortal m_Portal;
        readonly List<StampPortal> m_Stamps = new List<StampPortal>();
        readonly List<Drifter> m_Drifters = new List<Drifter>();
        Vector3 m_GhostLocal;
        float m_GhostSpeed, m_StepTimer;
        bool m_Chasing, m_Caught;
        float m_Beat = -1f, m_FinishedAt = -1f;
        int m_Step;

        /// <summary>The runner's top speed: the Passenger's chase speed (it catches a walker, not a sprinter), 85 % after two
        /// catches; Gentle stays below walking pace.</summary>
        float MaxSpeed
        {
            get
            {
                bool vr = Game.Mode == GameMode.VR;
                if (DimensionShift.Gentle) return vr ? 1.8f : 2.3f;
                float s = vr ? PassengerController.ChaseSpeedVR : PassengerController.ChaseSpeed;
                return Catches >= 2 ? s * 0.85f : s;
            }
        }

        // ------------------------------------------------------------------ enter / play / leave
        /// <summary>Builds the Between in <paramref name="host"/> (the active cell) and places the player at its start.</summary>
        public static BetweenRun Enter(Cell host)
        {
            if (Current != null) Destroy(Current.gameObject);
            var go = new GameObject("TheBetween");
            go.transform.SetParent(host.transform, false);
            go.transform.localPosition = s_Offset;
            var r = go.AddComponent<BetweenRun>();
            r.m_Host = host;
            r.Build();
            Current = r;
            // The void: its own fog, ambient and background while the run lasts (the host's are restored on leaving).
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = s_Void;
            RenderSettings.fogDensity = 0.011f;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.1f, 0.035f, 0.05f);
            RenderSettings.ambientEquatorColor = new Color(0.16f, 0.04f, 0.04f);
            RenderSettings.ambientGroundColor = new Color(0.42f, 0.08f, 0.05f);
            if (host.Sun != null) host.Sun.intensity = 0f;
            Game.Rig.Cam.backgroundColor = s_Void;
            Game.Rig.PlaceAt(go.transform.TransformPoint(new Vector3(0f, 0f, 3f)), host.transform.eulerAngles.y);
            return r;
        }

        /// <summary>The run itself: returns when the seal at the far end is reached (or the run is skipped).</summary>
        public static IEnumerator Play()
        {
            var r = Current;
            if (r == null) yield break;
            AudioDirector.I?.SetMusic("mus_chase", 2.5f, 0.65f);
            yield return new WaitForSeconds(Game.IsAutoTest ? 0.2f : 1.4f);
            // The reveal: footsteps and breath behind, the long shadow sliding past the player's feet.
            AudioDirector.I?.Play("pass_breath", r.m_Ghost.Chest.position, 1f, 0.9f, Bus.SFX, true);
            Hud.I?.ShowCaption("running footsteps, right behind you", 3.5f);
            r.m_Chasing = true;
            yield return new WaitForSeconds(Game.IsAutoTest ? 0.1f : 0.8f);
            Narration.I?.Say("home_03");
            Hud.I?.SetObjective("RUN. Grab the three pieces of your home stamp, then reach the seal.", true);
            Hud.I?.ShowToast(Game.Mode == GameMode.VR ? "Something is behind you. Run." : Game.Input.Label(Intent.Sprint) + ": sprint. Something is behind you.", 4f);
            while (!r.Finished)
            {
                r.m_Beat = Time.time;
                if (DimensionShift.Seen && DimensionShift.SkipPressed()) r.SkipToEnd();
                if (Game.IsAutoTest && r.AutoPilot && !r.m_Caught) r.AutoRunStep();
                yield return null;
            }
            r.m_Chasing = false;
            r.m_FinishedAt = Time.time;
            AudioDirector.I?.SetMusic(null, 2f);
        }

        /// <summary>Back to the host cell: the player at <paramref name="feet"/>, its light restored, the Between removed.</summary>
        public static void Leave(Vector3 feet, float yaw)
        {
            var r = Current;
            if (r == null) return;
            var host = r.m_Host;
            Game.Rig.PlaceAt(feet, yaw);
            host.Lighting.ApplyGlobal(host.Sun);
            Game.Rig.Cam.backgroundColor = host.Lighting.ClearColor;
            Destroy(r.gameObject);
            Current = null;
        }

        /// <summary>The runner stops where it is (review shots).</summary>
        public void Hold() => m_Chasing = false;

        void SkipToEnd()
        {
            foreach (var s in m_Stamps) if (s != null && !s.IsCollected) s.Collect();
            if (m_Portal != null && !m_Portal.IsCollected) { m_Portal.Ready = true; m_Portal.Collect(); }
            Finished = true;
        }

        /// <summary>Test mode: the player is carried along the causeway (through each piece, then the seal).</summary>
        void AutoRunStep()
        {
            var p = transform.InverseTransformPoint(Game.Rig.Feet);
            StampPortal next = null;
            foreach (var s in m_Stamps) if (s != null && !s.IsCollected) { next = s; break; }
            if (next == null) next = m_Portal;
            if (next == null) return;
            var goal = transform.InverseTransformPoint(next.transform.position);
            p.x = Mathf.MoveTowards(p.x, goal.x, Time.deltaTime * 4f);
            p.z += Time.deltaTime * 12f;
            p.y = 0f;
            Game.Rig.PlaceAt(transform.TransformPoint(p), transform.eulerAngles.y);
        }

        // ------------------------------------------------------------------ the chase
        void Update()
        {
            // Left behind (the run's coroutine stopped, say by quitting to the menu, or nobody called Leave): clean up.
            bool abandoned = (m_FinishedAt < 0f && m_Beat > 0f && Time.time - m_Beat > 1f) || (m_FinishedAt > 0f && Time.time - m_FinishedAt > 12f);
            if (abandoned)
            {
                if (Current == this) Current = null;
                if (m_Host != null && Game.Cells != null && Game.Cells.Active == m_Host) m_Host.Lighting.ApplyGlobal(m_Host.Sun);
                Destroy(gameObject);
                return;
            }
            foreach (var d in m_Drifters) d.Step(Time.deltaTime);
            if (m_Ghost == null || Game.Rig == null) return;
            float dt = Time.deltaTime;
            var player = transform.InverseTransformPoint(Game.Rig.Feet);
            var to = new Vector3(player.x - m_GhostLocal.x, 0f, player.z - m_GhostLocal.z);
            float dist = to.magnitude;
            if (m_Chasing && !m_Caught && !Finished)
            {
                m_GhostSpeed = Mathf.MoveTowards(m_GhostSpeed, MaxSpeed, dt * 2.2f);
                float keep = DimensionShift.Gentle ? 1.8f : 0f; // Gentle: it stops short instead of catching
                if (dist > keep + 0.05f) m_GhostLocal += to.normalized * Mathf.Min(m_GhostSpeed * dt, dist - keep);
                m_GhostLocal.x = Mathf.Clamp(m_GhostLocal.x, -Width / 2f + 0.4f, Width / 2f - 0.4f);
                m_Ghost.Pose = m_GhostSpeed > 1.2f ? Pose.Run : Pose.Walk;
                m_Ghost.Speed01 = Mathf.Clamp01(m_GhostSpeed / Mathf.Max(0.1f, MaxSpeed));
                m_StepTimer -= dt * Mathf.Max(0.5f, m_GhostSpeed);
                if (m_StepTimer <= 0f)
                {
                    m_StepTimer = 1.15f;
                    AudioDirector.I?.Play("pass_step_" + (1 + m_Step++ % 3), m_Ghost.transform.position + Vector3.up * 0.2f, 0.9f, Random.Range(0.85f, 1.05f));
                }
                if (!DimensionShift.Gentle && dist < 1.25f) StartCoroutine(Caught());
            }
            else
            {
                m_Ghost.Speed01 = Mathf.MoveTowards(m_Ghost.Speed01, 0f, dt * 3f);
                if (m_Ghost.Speed01 < 0.05f) m_Ghost.Pose = Pose.Idle;
            }
            m_Ghost.transform.localPosition = m_GhostLocal;
            if (to.sqrMagnitude > 0.01f)
                m_Ghost.transform.localRotation = Quaternion.Slerp(m_Ghost.transform.localRotation, Quaternion.LookRotation(to.normalized, Vector3.up), dt * 6f);
            // The shadow is thrown forward by the glow behind the runner, so it reaches the player before the runner does.
            var fwd = Vector3.forward;
            m_Shadow.localPosition = m_GhostLocal + fwd * 4.2f + Vector3.up * 0.04f;
            m_Shadow.localRotation = Quaternion.Euler(90f, 0f, 0f);
            m_ShadowMat.SetColor("_TintColor", new Color(0f, 0f, 0f, m_Chasing ? 0.8f : 0.45f));
        }

        IEnumerator Caught()
        {
            m_Caught = true;
            Catches++;
            AudioDirector.I?.Play("pass_sting", null, 1f, 1f, Bus.SFX, true);
            Fader.I.SetColor(new Color(0.16f, 0.01f, 0.02f));
            Fader.I.FadeTo(1f, 0.25f);
            yield return new WaitForSeconds(0.3f);
            Game.Rig.PlaceAt(transform.TransformPoint(new Vector3(0f, 0f, 3f)), transform.eulerAngles.y);
            m_GhostLocal = new Vector3(0f, 0f, -16f);
            m_GhostSpeed = 0f;
            Hud.I?.ShowCaption(Catches >= 2 ? "thrown back again. It is slowing." : "thrown back to where you landed", 3f);
            yield return new WaitForSeconds(0.2f);
            Fader.I.FadeTo(0f, 0.6f);
            yield return new WaitForSeconds(1f);
            Fader.I.SetColor(Color.black);
            m_Caught = false;
        }

        // ------------------------------------------------------------------ the place
        static Material Void(Color color, Color emission, float fogK)
        {
            // Self-lit like the landmark vistas: lit from the abyss below, fading into the void.
            var m = new Material(Mats.Template("TS_PocketLit")) { name = "Between" };
            m.SetColor("_BaseColor", color);
            m.SetTexture("_BaseMap", Tex.White);
            m.SetColor("_EmissionColor", emission);
            m.SetVector("_SunDir", new Vector3(0.15f, -1f, -0.2f).normalized);
            m.SetColor("_SunColor", new Color(1f, 0.28f, 0.14f) * 1.3f);
            m.SetColor("_SkyAmb", new Color(0.16f, 0.06f, 0.09f));
            m.SetColor("_GroundAmb", new Color(0.55f, 0.12f, 0.06f));
            m.SetColor("_FogColor", s_Void);
            m.SetFloat("_FogDensity", fogK);
            m.SetFloat("_StencilComp", (float)CompareFunction.Always);
            m.SetFloat("_UsePocketClip", 0f);
            m.renderQueue = 2000;
            return m;
        }

        void Build()
        {
            var rnd = new System.Random(2026);
            float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
            var stone = Mats.Lit(new Color(0.36f, 0.33f, 0.34f), 0.35f, 0f, "stone_dark_slate");
            var edge = Mats.Lit(new Color(0.24f, 0.2f, 0.2f), 0.3f, 0f, "stone_dark_slate");
            var rock = Mats.Lit(new Color(0.12f, 0.09f, 0.09f), 0.2f, 0f, null, false, new Color(0.12f, 0.015f, 0.01f));

            // The causeway: slabs with a low lip along each side, broken rock hanging under them.
            for (float z = -6f; z < Length + 6f; z += 6f)
            {
                float y = R(-0.03f, 0.03f);
                MeshFactory.Floor("Slab", transform, new Vector3(0f, y, z + 3f), new Vector2(Width, 5.94f), stone, "stone", 0.8f, 1.2f);
                foreach (float s in new[] { -1f, 1f })
                    MeshFactory.Box("Lip", transform, new Vector3(s * (Width / 2f - 0.15f), y + 0.12f, z + 3f), new Vector3(0.3f, 0.24f, 5.94f), edge, true, 1f);
                int hang = 1 + rnd.Next(2);
                for (int i = 0; i < hang; i++)
                {
                    var c = new Vector3(R(-2f, 2f), y - 0.8f, z + R(1f, 5f));
                    var f = MeshFactory.Frustum("Rock", transform, c, R(0.8f, 1.6f), 0.1f, R(2.5f, 6f), rock, 6);
                    f.transform.localRotation = Quaternion.Euler(180f, R(0f, 360f), 0f);
                    f.transform.localPosition = c;
                }
            }
            // Invisible walls along the edges and behind the start, so nobody falls into the void.
            void Wall(Vector3 c, Vector3 size) { var w = new GameObject("BetweenWall"); w.transform.SetParent(transform, false); w.transform.localPosition = c; w.AddComponent<BoxCollider>().size = size; }
            Wall(new Vector3(-Width / 2f - 0.15f, 1.5f, Length / 2f), new Vector3(0.3f, 3f, Length + 14f));
            Wall(new Vector3(Width / 2f + 0.15f, 1.5f, Length / 2f), new Vector3(0.3f, 3f, Length + 14f));
            Wall(new Vector3(0f, 1.5f, -6.2f), new Vector3(Width, 3f, 0.3f));
            Wall(new Vector3(0f, 1.5f, Length + 6.2f), new Vector3(Width, 3f, 0.3f));

            // A sky of its own: black overhead, a red glow round the horizon and below, faint stars.
            var sky = new LightingPreset
            {
                SkyZenith = new Color(0.012f, 0.002f, 0.01f), SkyHorizon = new Color(0.42f, 0.05f, 0.06f), SkyGround = new Color(0.62f, 0.09f, 0.05f),
                SunDiscColor = new Color(0f, 0f, 0f, 0f), CloudColor = new Color(0.45f, 0.06f, 0.07f, 0.45f), Stars = 0.35f, SkyExposure = 1f,
            };
            var dome = new MeshBuilder();
            dome.AddSphere(Vector3.zero, 520f, 32, 16);
            MeshFactory.Make("BetweenSky", dome.ToMesh("BetweenSky"), Mats.SkyInstance(sky), transform, new Vector3(0f, 0f, Length / 2f));

            // Glowing seams along both edges lead the eye to the seal; lamps of red light hang over the causeway.
            var seam = Mats.Emissive(new Color(0.4f, 0.05f, 0.03f), new Color(1.6f, 0.22f, 0.08f));
            foreach (float s in new[] { -1f, 1f })
                MeshFactory.Box("EdgeGlow", transform, new Vector3(s * (Width / 2f - 0.15f), 0.25f, Length / 2f), new Vector3(0.08f, 0.03f, Length + 12f), seam, false);
            for (int i = 0; i < 10; i++)
            {
                var l = new GameObject("CausewayLight").AddComponent<Light>();
                l.transform.SetParent(transform, false);
                l.transform.localPosition = new Vector3(i % 2 == 0 ? -1.8f : 1.8f, 2.6f, 4f + i * 15.5f);
                l.type = LightType.Point; l.color = new Color(1f, 0.3f, 0.16f); l.range = 8f; l.intensity = 1.5f; l.shadows = LightShadows.None;
            }

            // Red light rising from below the edges, and the glowing abyss far beneath.
            for (int i = 0; i < 9; i++)
            {
                var l = new GameObject("AbyssLight").AddComponent<Light>();
                l.transform.SetParent(transform, false);
                l.transform.localPosition = new Vector3(i % 2 == 0 ? -3.6f : 3.6f, -1.6f, 6f + i * 18f);
                l.type = LightType.Point; l.color = new Color(1f, 0.2f, 0.1f); l.range = 13f; l.intensity = 1.7f; l.shadows = LightShadows.None;
            }
            var glow = Mats.Particle(Tex.SoftDot, true, new Color(0.9f, 0.12f, 0.05f), 1f);
            MeshFactory.Quad("Abyss", transform, 700f, 700f, glow, new Vector3(0f, -70f, Length / 2f), Quaternion.Euler(90f, 0f, 0f));
            var mist = Mats.Particle(Tex.SoftDot, true, new Color(0.6f, 0.06f, 0.04f), 0.8f);
            for (int i = 0; i < 10; i++)
                MeshFactory.Quad("AbyssMist", transform, R(60f, 140f), R(20f, 40f), mist, new Vector3(R(-80f, 80f), R(-45f, -20f), R(-20f, Length + 40f)), Quaternion.Euler(R(-10f, 10f), R(0f, 360f), 0f));

            // Pieces of the apartment drifting past, dark and edged with red.
            var furniture = Mats.Lit(new Color(0.1f, 0.08f, 0.08f), 0.4f, 0f, null, false, new Color(0.22f, 0.03f, 0.02f));
            System.Func<string, Material> dark = _ => furniture;
            string[] pieces = { "Sofa", "DiningChair", "Fridge", "Door", "DiningTable", "Suitcase", "KitchenRun", "DiningChair", "Door", "Sofa", "DiningChair", "Fridge" };
            for (int i = 0; i < pieces.Length; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                var p = new Vector3(side * R(8f, 26f), R(-8f, 12f), R(10f, Length + 20f));
                var m = Models.Spawn(pieces[i], transform, dark, p, R(0f, 360f), R(1f, 1.6f));
                if (m == null) continue;
                m.transform.localRotation = Random.rotation;
                foreach (var rend in m.GetComponentsInChildren<Renderer>()) rend.shadowCastingMode = ShadowCastingMode.Off;
                m_Drifters.Add(new Drifter(m.transform, R(4f, 18f), R(0.2f, 0.6f)));
            }
            for (int i = 0; i < 26; i++)
            {
                float side = i % 2 == 0 ? -1f : 1f;
                var shard = MeshFactory.Box("Shard", transform, new Vector3(side * R(7f, 40f), R(-20f, 20f), R(-10f, Length + 30f)), new Vector3(R(0.4f, 3f), R(0.2f, 2f), R(0.4f, 3f)), rock, false, 1f, Random.rotation);
                shard.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
                m_Drifters.Add(new Drifter(shard.transform, R(6f, 30f), R(0.3f, 1f)));
            }

            // The countries ahead: their landmarks hang huge and tilted in the void, lit red from below.
            var silhouette = Void(new Color(0.32f, 0.27f, 0.28f), new Color(0.1f, 0.016f, 0.012f), 0.0032f);
            var beacon = Void(Color.black, new Color(2.2f, 0.45f, 0.25f), 0.0036f);
            System.Func<string, Material> lm = s => Models.SlotOf(s) == "TS_EmissiveWarm" ? beacon : silhouette;
            var landmarks = new (string name, Vector3 pos, Vector3 euler)[]
            {
                ("Landmark_Eiffel", new Vector3(-95f, -40f, 70f), new Vector3(8f, 30f, -16f)),
                ("Landmark_Liberty", new Vector3(110f, -50f, 110f), new Vector3(-6f, -110f, 12f)),
                ("Landmark_Pagoda", new Vector3(-70f, 6f, 170f), new Vector3(22f, 60f, 10f)),
                ("Landmark_ArchTower", new Vector3(140f, -90f, 215f), new Vector3(-12f, -40f, -18f)),
                ("Landmark_DomedPavilion", new Vector3(-160f, -55f, 245f), new Vector3(10f, 20f, 6f)),
                ("Landmark_ClockTower", new Vector3(80f, -30f, 30f), new Vector3(-20f, 15f, 22f)),
                ("Landmark_ColumnGate", new Vector3(0f, -10f, 280f), new Vector3(0f, 180f, -4f)),
            };
            foreach (var (name, pos, euler) in landmarks)
            {
                var m = Models.Spawn(name, transform, lm, pos, 0f, 1.35f);
                if (m == null) continue;
                m.transform.localRotation = Quaternion.Euler(euler);
                foreach (var rend in m.GetComponentsInChildren<Renderer>()) rend.shadowCastingMode = ShadowCastingMode.Off;
                m_Drifters.Add(new Drifter(m.transform, 0f, 0.05f));
            }

            // Three pieces of the home stamp over the causeway, and the seal at the far end (open once all three are held).
            var spots = new[] { new Vector3(-1.2f, 1.5f, 36f), new Vector3(1.2f, 1.5f, 72f), new Vector3(-0.8f, 1.5f, 108f) };
            foreach (var s in spots)
                m_Stamps.Add(StampPortal.Spawn(transform, transform.TransformPoint(s), CountryId.UAE, OnPiece));
            m_Portal = StampPortal.Spawn(transform, transform.TransformPoint(new Vector3(0f, 2.3f, 146f)), CountryId.UAE, _ => Finished = true, true);
            m_Portal.Ready = false;

            // The runner: smoked glass, standing in the glow behind the start.
            m_Ghost = PassengerBody.Create(transform);
            m_Ghost.SetDisguise(Disguise.None);
            m_Ghost.RiseAmount = 1f;
            m_GhostLocal = new Vector3(0f, 0f, -16f);
            m_Ghost.transform.localPosition = m_GhostLocal;
            var behind = new GameObject("RunnerLight").AddComponent<Light>();
            behind.transform.SetParent(m_Ghost.transform, false);
            behind.transform.localPosition = new Vector3(0f, 2.6f, -2.2f);
            behind.type = LightType.Point; behind.color = new Color(1f, 0.22f, 0.12f); behind.range = 7f; behind.intensity = 2.4f; behind.shadows = LightShadows.None;
            MeshFactory.Quad("RunnerGlow", transform, 14f, 9f, glow, new Vector3(0f, 3f, -24f), Quaternion.identity);
            m_ShadowMat = new Material(Mats.Particle(Tex.SoftDot, false, new Color(0f, 0f, 0f, 0.8f), 1f)) { name = "RunnerShadow" };
            m_Shadow = MeshFactory.Quad("RunnerShadow", transform, 1.6f, 8.5f, m_ShadowMat).transform;

            foreach (var rend in GetComponentsInChildren<Renderer>())
                if (rend.GetComponentInParent<PassengerBody>() == null) rend.shadowCastingMode = ShadowCastingMode.Off;
        }

        void OnPiece(StampPortal s)
        {
            Collected++;
            Hud.I?.ShowToast("Home stamp: " + Collected + " / " + Pieces, 2.5f);
            if (Collected >= Pieces && m_Portal != null)
            {
                m_Portal.Ready = true;
                AudioDirector.I?.Play("seal_verify", m_Portal.transform.position, 1f, 0.8f);
                Hud.I?.SetObjective("The seal is open. Reach it.", true);
            }
        }

        /// <summary>Slow tumble and bob for the drifting pieces.</summary>
        class Drifter
        {
            readonly Transform m_T;
            readonly Vector3 m_Base, m_Axis;
            readonly float m_Spin, m_Bob, m_Phase;

            public Drifter(Transform t, float spinDegPerSec, float bob)
            {
                m_T = t; m_Base = t.localPosition; m_Axis = Random.onUnitSphere; m_Spin = spinDegPerSec; m_Bob = bob; m_Phase = Random.Range(0f, 10f);
            }

            public void Step(float dt)
            {
                if (m_T == null) return;
                if (m_Spin > 0f) m_T.Rotate(m_Axis, m_Spin * dt, Space.Self);
                m_T.localPosition = m_Base + Vector3.up * Mathf.Sin(Time.time * 0.4f + m_Phase) * m_Bob * 3f;
            }
        }
    }
}

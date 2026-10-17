using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// The second-hand camera and optics shop in Marrakech. ShopNormal: warm evening prologue with an ordinary,
    /// friendly shopkeeper. ShopCorrupt: the Margin copy for the Morocco encounter — same counter and layout, but the
    /// shelves repeat into fog and the keeper is gone.
    /// </summary>
    public class ShopCell : Cell
    {
        public bool Corrupted;
        public Transform CounterTop, DoorPoint, DisplaySpot, TraySpot, ShopkeeperSpot, DrawerRoot;
        public Shopkeeper Keeper;
        public Light Lamp;
        public readonly List<Transform> CounterMarks = new List<Transform>();
        public TraceMark Trace;

        protected override void Build()
        {
            Lighting = Corrupted ? new LightingPreset
            {
                SunColor = new Color(0.5f, 0.62f, 0.66f), SunIntensity = 0.12f, SunEuler = new Vector3(50f, 20f, 0f), ShadowStrength = 0.4f,
                AmbientSky = new Color(0.1f, 0.14f, 0.15f), AmbientEquator = new Color(0.08f, 0.1f, 0.1f), AmbientGround = new Color(0.04f, 0.05f, 0.05f),
                FogColor = new Color(0.05f, 0.08f, 0.08f), FogDensity = 0.09f,
                SkyZenith = new Color(0.02f, 0.04f, 0.05f), SkyHorizon = new Color(0.08f, 0.12f, 0.12f), SkyGround = new Color(0.03f, 0.04f, 0.04f),
                SunDiscColor = new Color(0, 0, 0, 0),
                PostExposure = 0.3f, Saturation = -30f, Contrast = 14f, ColorFilter = new Color(0.82f, 0.98f, 0.95f), Bloom = 0.6f, Vignette = 0.34f
            } : new LightingPreset
            {
                SunColor = new Color(1f, 0.7f, 0.45f), SunIntensity = 1.1f, SunEuler = new Vector3(18f, 300f, 0f), ShadowStrength = 0.8f,
                AmbientSky = new Color(0.45f, 0.42f, 0.48f), AmbientEquator = new Color(0.42f, 0.3f, 0.22f), AmbientGround = new Color(0.2f, 0.13f, 0.09f),
                FogColor = new Color(0.62f, 0.45f, 0.35f), FogDensity = 0.012f,
                SkyZenith = new Color(0.22f, 0.3f, 0.52f), SkyHorizon = new Color(0.98f, 0.66f, 0.42f), SkyGround = new Color(0.4f, 0.28f, 0.2f),
                SunDiscColor = new Color(1f, 0.7f, 0.45f, 1f), SunSize = 0.006f, CloudColor = new Color(1f, 0.75f, 0.6f, 0.3f),
                PostExposure = 0.05f, Saturation = 8f, Contrast = 8f, ColorFilter = new Color(1f, 0.95f, 0.88f), Bloom = 0.55f, Vignette = 0.2f
            };
            Ambience = Corrupted ? "amb_margin" : "amb_shop";
            AddSun();

            var terracotta = Mats.Lit(Corrupted ? new Color(0.45f, 0.4f, 0.37f) : new Color(0.82f, 0.5f, 0.36f), 0.2f, 0f, "plaster_terracotta");
            var zellige = Mats.Lit(Corrupted ? new Color(0.55f, 0.6f, 0.58f) : Color.white, 0.6f, 0f, "tile_zellige");
            var wood = Mats.Lit(new Color(0.38f, 0.24f, 0.15f), 0.45f, 0f, "wood_dark");
            var brass = Mats.Lit(new Color(0.76f, 0.58f, 0.3f), 0.65f, 0.9f, "metal_brushed");
            var glass = Mats.Glass(new Color(0.75f, 0.85f, 0.85f, 0.18f));
            var paving = Mats.Lit(new Color(0.7f, 0.6f, 0.5f), 0.3f, 0f, "plaza_stone");

            var g = Group("Shop");
            // Street outside (prologue start).
            MeshFactory.Floor("Lane", g, new Vector3(0, 0, -6.5f), new Vector2(3.4f, 7.5f), paving, "stone", 0.2f, 1f);
            MeshFactory.Box("LaneWallL", g, new Vector3(-1.85f, 2.5f, -6.5f), new Vector3(0.3f, 5f, 7.5f), terracotta, true, 1.4f);
            MeshFactory.Box("LaneWallR", g, new Vector3(1.85f, 2.5f, -6.5f), new Vector3(0.3f, 5f, 7.5f), terracotta, true, 1.4f);
            MeshFactory.Box("LaneTileL", g, new Vector3(-1.68f, 0.45f, -6.5f), new Vector3(0.04f, 0.9f, 7.5f), zellige, false, 0.6f);
            MeshFactory.Box("LaneTileR", g, new Vector3(1.68f, 0.45f, -6.5f), new Vector3(0.04f, 0.9f, 7.5f), zellige, false, 0.6f);
            MeshFactory.Box("LaneEnd", g, new Vector3(0, 2.5f, -10.3f), new Vector3(3.8f, 5f, 0.3f), terracotta, true, 1.4f);
            var lantern = MeshFactory.Frustum("StreetLantern", g, new Vector3(0, 3.1f, -6.2f), 0.18f, 0.1f, 0.38f, Mats.Emissive(new Color(0.4f, 0.3f, 0.15f), Corrupted ? new Color(0.3f, 0.5f, 0.45f) : new Color(1.6f, 0.9f, 0.4f)), 8);
            AddPointLight(g, new Vector3(0, 2.9f, -6.2f), Corrupted ? new Color(0.6f, 0.85f, 0.8f) : new Color(1f, 0.7f, 0.4f), 1.6f, 6f);

            // Shop front with door opening.
            MeshFactory.Box("FrontL", g, new Vector3(-1.25f, 2.5f, -3f), new Vector3(1.4f, 5f, 0.3f), terracotta, true, 1.4f);
            MeshFactory.Box("FrontR", g, new Vector3(1.25f, 2.5f, -3f), new Vector3(1.4f, 5f, 0.3f), terracotta, true, 1.4f);
            MeshFactory.Box("FrontTop", g, new Vector3(0, 3.9f, -3f), new Vector3(1.2f, 2.2f, 0.3f), terracotta, true, 1.4f);
            MeshFactory.Box("DoorFrame", g, new Vector3(0, 2.82f, -3.16f), new Vector3(1.3f, 0.1f, 0.06f), wood, false);
            MeshFactory.Box("SignBoard", g, new Vector3(0, 3.3f, -3.18f), new Vector3(1.6f, 0.36f, 0.05f), wood, false);
            var sign = TextFactory.World(g, Corrupted ? "Cameras  &  Optics  &  Cameras  &" : "Cameras  &  Optics", 0.11f, new Color(0.95f, 0.85f, 0.6f), TMPro.TextAlignmentOptions.Center, true, 1.6f);
            sign.transform.localPosition = new Vector3(0, 3.3f, -3.215f);
            DoorPoint = new GameObject("DoorPoint").transform;
            DoorPoint.SetParent(g, false);
            DoorPoint.localPosition = new Vector3(0, 0, -3f);

            // Interior.
            MeshFactory.Floor("ShopFloor", g, new Vector3(0, 0.01f, -1f), new Vector2(5f, 4.2f), zellige, "tile", 0.2f, 0.8f);
            MeshFactory.Box("Ceiling", g, new Vector3(0, 3.0f, -1f), new Vector3(5f, 0.1f, 4.2f), wood, false, 1.5f);
            MeshFactory.Box("BackWall", g, new Vector3(0, 1.5f, 1.15f), new Vector3(5f, 3f, 0.2f), terracotta, true, 1.4f);
            if (!Corrupted)
            {
                MeshFactory.Box("SideWallL", g, new Vector3(-2.55f, 1.5f, -1f), new Vector3(0.2f, 3f, 4.2f), terracotta, true, 1.4f);
                MeshFactory.Box("SideWallR", g, new Vector3(2.55f, 1.5f, -1f), new Vector3(0.2f, 3f, 4.2f), terracotta, true, 1.4f);
                for (int side = -1; side <= 1; side += 2) Shelves(g, new Vector3(side * 2.25f, 0f, -1f), side, wood, brass, 3);
            }
            else
            {
                // The shelves repeat impossibly: aisles stretch away into fog on both sides.
                for (int side = -1; side <= 1; side += 2)
                    for (int k = 0; k < 12; k++)
                        Shelves(g, new Vector3(side * (2.25f + k * 1.7f), 0f, -1f), side, wood, brass, 3);
                MeshFactory.Floor("AisleFloor", g, new Vector3(0, 0.005f, -1f), new Vector2(46f, 4.2f), zellige, "tile", 0.2f, 0.8f);
                MeshFactory.Box("AisleCeiling", g, new Vector3(0, 3.0f, -1f), new Vector3(46f, 0.1f, 4.2f), wood, false, 1.5f);
                MeshFactory.Box("AisleBackL", g, new Vector3(-12f, 1.5f, 1.15f), new Vector3(19f, 3f, 0.2f), terracotta, true, 1.4f);
                MeshFactory.Box("AisleBackR", g, new Vector3(12f, 1.5f, 1.15f), new Vector3(19f, 3f, 0.2f), terracotta, true, 1.4f);
                MeshFactory.Box("AisleFrontL", g, new Vector3(-12f, 1.5f, -3.1f), new Vector3(19f, 3f, 0.2f), terracotta, true, 1.4f);
                MeshFactory.Box("AisleFrontR", g, new Vector3(12f, 1.5f, -3.1f), new Vector3(19f, 3f, 0.2f), terracotta, true, 1.4f);
            }

            // Counter with display case, brass tray and drawer.
            MeshFactory.Box("Counter", g, new Vector3(0, 0.5f, 0.15f), new Vector3(2.6f, 1f, 0.6f), wood, true);
            MeshFactory.Box("CounterTop", g, new Vector3(0, 1.02f, 0.15f), new Vector3(2.7f, 0.04f, 0.66f), wood, false);
            var display = MeshFactory.Box("DisplayGlass", g, new Vector3(-0.6f, 1.16f, 0.2f), new Vector3(0.7f, 0.24f, 0.4f), glass, false);
            display.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            CounterTop = new GameObject("CounterTop").transform;
            CounterTop.SetParent(g, false);
            CounterTop.localPosition = new Vector3(0, 1.04f, 0.05f);
            DisplaySpot = new GameObject("DisplaySpot").transform;
            DisplaySpot.SetParent(g, false);
            DisplaySpot.localPosition = new Vector3(-0.6f, 1.06f, 0.15f);
            TraySpot = new GameObject("TraySpot").transform;
            TraySpot.SetParent(g, false);
            TraySpot.localPosition = new Vector3(0.45f, 1.045f, -0.05f);
            MeshFactory.Cylinder("Tray", g, TraySpot.localPosition - Vector3.up * 0.005f, 0.09f, 0.012f, brass, 24, false);
            DrawerRoot = new GameObject("Drawer").transform;
            DrawerRoot.SetParent(g, false);
            DrawerRoot.localPosition = new Vector3(0.6f, 0.8f, -0.12f);
            MeshFactory.Box("DrawerFront", DrawerRoot, Vector3.zero, new Vector3(0.6f, 0.18f, 0.04f), wood, false);
            MeshFactory.Box("DrawerKnob", DrawerRoot, new Vector3(0, 0, -0.03f), new Vector3(0.08f, 0.03f, 0.03f), brass, false);

            // Hanging lamp and ceiling fan.
            var lampShade = MeshFactory.Frustum("LampShade", g, new Vector3(0, 2.4f, -0.4f), 0.3f, 0.12f, 0.25f, Mats.Emissive(new Color(0.3f, 0.22f, 0.12f), Corrupted ? new Color(0.25f, 0.45f, 0.4f) : new Color(1.2f, 0.7f, 0.35f)), 10);
            lampShade.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Lamp = AddPointLight(g, new Vector3(0, 2.3f, -0.4f), Corrupted ? new Color(0.6f, 0.85f, 0.8f) : new Color(1f, 0.72f, 0.42f), Corrupted ? 1.1f : 2.4f, 6f, true);
            var fan = new GameObject("CeilingFan").transform;
            fan.SetParent(g, false);
            fan.localPosition = new Vector3(0, 2.85f, -1.6f);
            for (int i = 0; i < 4; i++) MeshFactory.Box("Blade", fan, Quaternion.Euler(0, i * 90f, 0) * new Vector3(0, 0, 0.35f), new Vector3(0.12f, 0.01f, 0.6f), wood, false, 1f, Quaternion.Euler(0, i * 90f, 0));
            fan.gameObject.AddComponent<Spin>().DegreesPerSecond = Corrupted ? 6f : 70f;

            ShopkeeperSpot = new GameObject("ShopkeeperSpot").transform;
            ShopkeeperSpot.SetParent(g, false);
            ShopkeeperSpot.localPosition = new Vector3(0.55f, 0f, 0.75f);
            if (!Corrupted)
            {
                Keeper = Shopkeeper.Create(g);
                Keeper.transform.localPosition = ShopkeeperSpot.localPosition;
                Keeper.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            }
            else
            {
                // The counter remembers where things were: faint outlines for the reconstruction puzzle.
                foreach (var p in new[] { new Vector3(-0.85f, 1.045f, -0.02f), new Vector3(0.45f, 1.045f, -0.05f), new Vector3(1.05f, 1.045f, 0.02f) })
                {
                    var m = new GameObject("CounterMark").transform;
                    m.SetParent(g, false);
                    m.localPosition = p;
                    MeshFactory.Quad("Outline", m, 0.2f, 0.16f, Mats.Unlit(new Color(0.55f, 0.95f, 0.85f, 0.25f), Tex.Ring, true), new Vector3(0, 0.002f, 0), Quaternion.Euler(90, 0, 0));
                    CounterMarks.Add(m);
                }
                Trace = TraceMark.Create(transform, CountryId.Morocco, new Vector3(-0.2f, 1.05f, 0.25f), Quaternion.identity, 0.22f, true);
            }

            AddSeam("door", new Vector3(0f, 0f, -2.7f), 0f, 1.3f);
            AddSeam("shelves", new Vector3(-1.9f, 0f, -0.6f), 90f, 1.3f);
            if (!Corrupted) foreach (var s in Seams) s.Revealed = false;
            SetArrival(Corrupted ? new Vector3(0f, 0f, -2.2f) : new Vector3(0f, 0f, -8.6f), 0f);
            AddBounds(new Vector3(0, 0, Corrupted ? -1f : -4.2f), Corrupted ? new Vector3(40f, 3f, 4.2f) : new Vector3(3.6f, 3f, 12.6f));
            if (Corrupted) SetEncounter(gameObject.AddComponent<MoroccoEncounter>());
            else SetEncounter(gameObject.AddComponent<HomeEncounter>());
        }

        static void Shelves(Transform parent, Vector3 pos, int side, Material wood, Material brass, int rows)
        {
            var root = new GameObject("Shelves").transform;
            root.SetParent(parent, false);
            root.localPosition = pos;
            MeshFactory.Box("Back", root, new Vector3(side * 0.22f, 1.2f, 0), new Vector3(0.05f, 2.4f, 3.6f), wood, true);
            var rng = new System.Random((int)(pos.x * 10));
            var dark = Mats.Lit(new Color(0.08f, 0.08f, 0.09f), 0.4f, 0.2f);
            var chrome = Mats.Lit(new Color(0.75f, 0.76f, 0.78f), 0.8f, 1f);
            var boxMat = Mats.Lit(new Color(0.55f, 0.42f, 0.3f), 0.2f, 0f, "paper_ivory");
            for (int r = 0; r < rows; r++)
            {
                float y = 0.5f + r * 0.6f;
                MeshFactory.Box("Board", root, new Vector3(0, y, 0), new Vector3(0.45f, 0.04f, 3.6f), wood, true);
                for (int k = 0; k < 6; k++)
                {
                    float z = -1.5f + k * 0.6f + (float)rng.NextDouble() * 0.15f;
                    int kind = rng.Next(3);
                    if (kind == 0)
                    {
                        MeshFactory.Box("Camera", root, new Vector3(0, y + 0.07f, z), new Vector3(0.12f, 0.1f, 0.16f), dark, false);
                        MeshFactory.Cylinder("Lens", root, new Vector3(-side * 0.06f, y + 0.07f, z), 0.035f, 0.06f, chrome, 12, false).transform.localRotation = Quaternion.Euler(0, 0, side * 90f);
                    }
                    else if (kind == 1) MeshFactory.Box("Box", root, new Vector3(0, y + 0.08f, z), new Vector3(0.2f, 0.14f, 0.24f), boxMat, false);
                    else MeshFactory.Cylinder("LensBarrel", root, new Vector3(0, y + 0.02f, z), 0.04f, 0.12f, brass, 12, false);
                }
            }
        }

        public static ShopCell Make(GameObject go, bool corrupted)
        {
            var c = go.AddComponent<ShopCell>();
            c.Corrupted = corrupted;
            return c;
        }
    }

    public class Spin : MonoBehaviour
    {
        public float DegreesPerSecond = 60f;
        void Update() => transform.Rotate(0, DegreesPerSecond * Time.deltaTime, 0, Space.Self);
    }

    /// <summary>An ordinary, friendly shopkeeper: simple stylised figure with idle and gesture animation.</summary>
    public class Shopkeeper : MonoBehaviour
    {
        Transform m_Head, m_ArmR, m_ArmL, m_Body;
        float m_Gesture;

        public static Shopkeeper Create(Transform parent)
        {
            var go = new GameObject("Shopkeeper");
            go.transform.SetParent(parent, false);
            var s = go.AddComponent<Shopkeeper>();
            s.Build();
            return s;
        }

        void Build()
        {
            var shirt = Mats.Lit(new Color(0.78f, 0.76f, 0.7f), 0.2f, 0f, "fabric_linen");
            var vest = Mats.Lit(new Color(0.22f, 0.24f, 0.28f), 0.3f, 0f, "fabric_linen");
            var trousers = Mats.Lit(new Color(0.25f, 0.22f, 0.2f), 0.25f);
            var skin = Mats.Lit(new Color(0.62f, 0.45f, 0.34f), 0.35f);
            var hair = Mats.Lit(new Color(0.55f, 0.53f, 0.5f), 0.3f);
            var metal = Mats.Lit(new Color(0.6f, 0.6f, 0.62f), 0.7f, 0.9f);
            m_Body = new GameObject("Body").transform;
            m_Body.SetParent(transform, false);
            MeshFactory.Frustum("Legs", m_Body, Vector3.zero, 0.17f, 0.16f, 0.9f, trousers, 12);
            MeshFactory.Frustum("Torso", m_Body, new Vector3(0, 0.88f, 0), 0.19f, 0.21f, 0.55f, shirt, 14);
            MeshFactory.Frustum("Vest", m_Body, new Vector3(0, 0.9f, 0), 0.205f, 0.22f, 0.45f, vest, 14);
            m_Head = new GameObject("Head").transform;
            m_Head.SetParent(m_Body, false);
            m_Head.localPosition = new Vector3(0, 1.55f, 0);
            MeshFactory.Cylinder("Neck", m_Body, new Vector3(0, 1.42f, 0), 0.055f, 0.1f, skin, 10, false);
            var head = MeshFactory.Sphere("HeadShape", m_Head, new Vector3(0, 0.1f, 0), 1f, skin, 16, 12);
            head.transform.localScale = new Vector3(0.1f, 0.125f, 0.11f);
            var cap = MeshFactory.Sphere("Hair", m_Head, new Vector3(0, 0.15f, -0.01f), 1f, hair, 14, 10);
            cap.transform.localScale = new Vector3(0.105f, 0.09f, 0.11f);
            var beard = MeshFactory.Sphere("Beard", m_Head, new Vector3(0, 0.03f, 0.04f), 1f, hair, 12, 8);
            beard.transform.localScale = new Vector3(0.075f, 0.06f, 0.07f);
            // Reading glasses perched on the nose (a nod to his trade).
            var gl = new MeshBuilder();
            gl.AddTorus(new Vector3(-0.035f, 0, 0), 0.02f, 0.003f, 16, 4, Quaternion.Euler(90, 0, 0));
            gl.AddTorus(new Vector3(0.035f, 0, 0), 0.02f, 0.003f, 16, 4, Quaternion.Euler(90, 0, 0));
            MeshFactory.FromBuilder("ReadingGlasses", gl, metal, m_Head).transform.localPosition = new Vector3(0, 0.1f, 0.105f);
            m_ArmL = Arm("ArmL", new Vector3(-0.24f, 1.36f, 0), shirt, skin);
            m_ArmR = Arm("ArmR", new Vector3(0.24f, 1.36f, 0), shirt, skin);
            MeshFactory.NoShadows(gameObject);
            foreach (var r in GetComponentsInChildren<Renderer>()) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        Transform Arm(string name, Vector3 shoulder, Material sleeve, Material skin)
        {
            var t = new GameObject(name).transform;
            t.SetParent(m_Body, false);
            t.localPosition = shoulder;
            var b = new MeshBuilder();
            b.AddFrustum(Vector3.zero, 0.05f, 0.055f, 0.55f, 10);
            MeshFactory.FromBuilder("Sleeve", b, sleeve, t).transform.localPosition = new Vector3(0, -0.55f, 0);
            MeshFactory.Sphere("Hand", t, new Vector3(0, -0.6f, 0), 0.045f, skin);
            return t;
        }

        /// <summary>A short open-handed gesture while speaking.</summary>
        public void Gesture() => m_Gesture = 1f;

        void Update()
        {
            float t = Time.time;
            m_Gesture = Mathf.MoveTowards(m_Gesture, 0f, Time.deltaTime * 0.6f);
            m_Body.localRotation = Quaternion.Euler(Mathf.Sin(t * 1.1f) * 1.2f, 0, Mathf.Sin(t * 0.7f) * 0.8f);
            float g = Mathf.Sin(Mathf.Clamp01(m_Gesture) * Mathf.PI);
            m_ArmR.localRotation = Quaternion.Euler(-g * 55f - 8f, 0, -6f - g * 10f);
            m_ArmL.localRotation = Quaternion.Euler(-12f, 0, 6f);
            var rig = Game.Rig;
            if (rig != null)
            {
                var to = rig.Head.position - m_Head.position;
                var local = transform.InverseTransformDirection(to);
                float yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -55f, 55f);
                m_Head.localRotation = Quaternion.Slerp(m_Head.localRotation, Quaternion.Euler(-4f, yaw, 0), Time.deltaTime * 2.5f);
            }
        }
    }
}

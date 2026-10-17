using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// Distant landmark silhouettes (self-lit backdrop material so they read through fog). One recognisable
    /// silhouette per country; never mixed. Positions are cell-local, typically 120–260 m away. Each uses its Blender
    /// model when the file is present, otherwise the procedural shapes below.
    /// </summary>
    public static class Landmarks
    {
        static GameObject Root(Transform parent, string name, Vector3 pos, float yaw, float scale)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        static void NoShadows(GameObject go) => MeshFactory.NoShadows(go);

        /// <summary>
        /// The Blender landmark (Art/Models/MODELS.md) on a silhouette root. Its slots all take the cell's backdrop materials,
        /// so it keeps the cell's fog and lighting. False when the model is missing: build the procedural silhouette.
        /// </summary>
        static bool Model(GameObject root, string name, System.Func<string, Material> slot)
        {
            if (Models.Spawn(name, root.transform, slot) == null) return false;
            NoShadows(root);
            return true;
        }

        /// <summary>Every slot on <paramref name="mat"/>, except the listed ones on <paramref name="light"/> when there is one.</summary>
        static System.Func<string, Material> Backdrop(Material mat, Material light = null, params string[] lightSlots) =>
            s => light != null && System.Array.IndexOf(lightSlots, Models.SlotOf(s)) >= 0 ? light : mat;

        /// <summary>Lattice tower (Paris). Height ~1 unit * scale * 100.</summary>
        public static GameObject LatticeTower(Transform parent, Vector3 pos, float scale, Material mat, Material lightMat = null)
        {
            var go = Root(parent, "EiffelSilhouette", pos, 0, scale);
            if (Model(go, "Landmark_Eiffel", Backdrop(mat, lightMat, "TS_EmissiveWarm"))) return go;
            var mb = new MeshBuilder();
            float H = 100f;
            // Four curved legs as stacked segments leaning inward.
            for (int leg = 0; leg < 4; leg++)
            {
                float ang = (45f + leg * 90f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
                var pts = new List<Vector3>();
                for (int i = 0; i <= 10; i++)
                {
                    float t = i / 10f;
                    float y = t * H * 0.36f;
                    float r = Mathf.Lerp(17f, 6.5f, Mathf.Pow(t, 0.75f));
                    pts.Add(dir * r + Vector3.up * y);
                }
                mb.AddTube(pts, 1.6f, 4, false);
                // Cross bracing between legs.
            }
            // First and second platforms.
            mb.AddBox(new Vector3(0, H * 0.18f, 0), new Vector3(26f, 1.6f, 26f));
            mb.AddBox(new Vector3(0, H * 0.36f, 0), new Vector3(14f, 1.4f, 14f));
            // Arches between legs at the base.
            for (int side = 0; side < 4; side++)
                mb.AddArch(Quaternion.Euler(0, side * 90f, 0) * new Vector3(0, H * 0.06f, 15.5f), 8f, 9.2f, 0.9f, 10, Quaternion.Euler(0, side * 90f, 0));
            // Upper shaft tapering to the top.
            var shaft = new List<Vector2> { new Vector2(6.5f, H * 0.36f), new Vector2(4.2f, H * 0.55f), new Vector2(2.4f, H * 0.78f), new Vector2(1.1f, H * 0.93f), new Vector2(0.4f, H * 1.0f), new Vector2(0.15f, H * 1.08f) };
            mb.AddLathe(Vector3.zero, shaft, 4, 1f, 45f, 405f);
            // Lattice suggestion: horizontal bands.
            for (int i = 0; i < 9; i++)
            {
                float y = H * (0.4f + i * 0.065f);
                float r = Mathf.Lerp(6.2f, 0.9f, (y - H * 0.36f) / (H * 0.62f));
                mb.AddBox(new Vector3(0, y, 0), new Vector3(r * 2f, 0.45f, r * 2f));
            }
            MeshFactory.FromBuilder("Tower", mb, mat, go.transform);
            if (lightMat != null) MeshFactory.Sphere("Beacon", go.transform, new Vector3(0, H * 1.08f, 0), 0.9f, lightMat);
            NoShadows(go);
            return go;
        }

        public static GameObject Pyramids(Transform parent, Vector3 pos, float yaw, Material mat)
        {
            var go = Root(parent, "Pyramids", pos, yaw, 1f);
            var mb = new MeshBuilder();
            void Pyr(Vector3 c, float half, float h)
            {
                var top = c + Vector3.up * h;
                Vector3 a = c + new Vector3(-half, 0, -half), b = c + new Vector3(half, 0, -half), d = c + new Vector3(half, 0, half), e = c + new Vector3(-half, 0, half);
                mb.AddTriangle(a, top, b, Vector2.zero, new Vector2(0.5f, 1), new Vector2(1, 0));
                mb.AddTriangle(b, top, d, Vector2.zero, new Vector2(0.5f, 1), new Vector2(1, 0));
                mb.AddTriangle(d, top, e, Vector2.zero, new Vector2(0.5f, 1), new Vector2(1, 0));
                mb.AddTriangle(e, top, a, Vector2.zero, new Vector2(0.5f, 1), new Vector2(1, 0));
            }
            Pyr(new Vector3(0, 0, 0), 46f, 62f);
            Pyr(new Vector3(-95f, 0, 40f), 40f, 54f);
            Pyr(new Vector3(-160f, 0, 75f), 22f, 30f);
            MeshFactory.FromBuilder("Pyramids", mb, mat, go.transform);
            NoShadows(go);
            return go;
        }

        public static GameObject Liberty(Transform parent, Vector3 pos, float yaw, Material mat, Material torch)
        {
            var go = Root(parent, "LibertySilhouette", pos, yaw, 1f);
            if (Model(go, "Landmark_Liberty", Backdrop(mat, torch, "TS_EmissiveWarm"))) return go;
            var mb = new MeshBuilder();
            mb.AddBox(new Vector3(0, 10, 0), new Vector3(16, 20, 16));                  // pedestal base
            mb.AddBox(new Vector3(0, 26, 0), new Vector3(11, 12, 11));                  // pedestal top
            mb.AddFrustum(new Vector3(0, 32, 0), 5f, 3.2f, 20f, 10);                    // robe
            mb.AddFrustum(new Vector3(0, 52, 0), 3.2f, 2.2f, 6f, 10);                   // shoulders
            mb.AddSphere(new Vector3(0, 60, 0), 2.1f, 10, 8);                           // head
            for (int i = 0; i < 7; i++)                                                 // crown rays
            {
                float a = (-60f + i * 20f) * Mathf.Deg2Rad;
                mb.AddBox(new Vector3(Mathf.Sin(a) * 2.6f, 62f, -Mathf.Cos(a) * 0.5f), new Vector3(0.35f, 3f, 0.35f), Quaternion.Euler(0, 0, -a * Mathf.Rad2Deg * 0.6f));
            }
            mb.AddBox(new Vector3(2.6f, 62, 0), new Vector3(1.2f, 14, 1.2f), Quaternion.Euler(0, 0, -12));   // raised arm
            mb.AddBox(new Vector3(-2.8f, 47, -1.2f), new Vector3(2.2f, 4f, 0.8f));                            // tablet
            MeshFactory.FromBuilder("Liberty", mb, mat, go.transform);
            var flame = MeshFactory.Sphere("TorchFlame", go.transform, new Vector3(4.1f, 70f, 0), 1.3f, torch);
            NoShadows(go);
            return go;
        }

        public static GameObject Pagoda(Transform parent, Vector3 pos, float yaw, Material mat, Material roof)
        {
            var go = Root(parent, "PagodaSilhouette", pos, yaw, 1f);
            if (Model(go, "Landmark_Pagoda", Backdrop(mat, roof, "TS_MetalDark", "TS_Brass"))) return go; // roofs and finial
            float y = 0;
            for (int i = 0; i < 5; i++)
            {
                float w = 14f - i * 2.2f;
                var body = MeshFactory.Box("Tier", go.transform, new Vector3(0, y + 2.5f, 0), new Vector3(w * 0.7f, 5f, w * 0.7f), mat, false);
                var mb = new MeshBuilder();
                var prof = new List<Vector2> { new Vector2(w * 0.85f, 0f), new Vector2(w * 0.6f, 1.2f), new Vector2(w * 0.35f, 1.8f) };
                mb.AddLathe(Vector3.zero, prof, 4, 1f, 45f, 405f);
                var r = MeshFactory.FromBuilder("Roof", mb, roof, go.transform);
                r.transform.localPosition = new Vector3(0, y + 5f, 0);
                y += 6.6f;
            }
            MeshFactory.Cylinder("Spire", go.transform, new Vector3(0, y, 0), 0.4f, 9f, roof, 8, false);
            NoShadows(go);
            return go;
        }

        /// <summary>Tall tower with a parabolic opening and sky bridge near the top (Riyadh skyline icon, abstracted).</summary>
        public static GameObject ArchTower(Transform parent, Vector3 pos, float yaw, Material mat, Material lights)
        {
            var go = Root(parent, "ArchTowerSilhouette", pos, yaw, 1f);
            if (Model(go, "Landmark_ArchTower", Backdrop(mat, lights, "TS_Glass", "TS_EmissiveWarm"))) return go; // the lit sky bridge
            var outline = new List<Vector2>();
            // Outer profile (half), then the inner parabolic cut-out as a separate prism pair.
            float H = 150f, W = 30f;
            var mb = new MeshBuilder();
            var lower = new List<Vector2> { new Vector2(-W / 2, 0), new Vector2(W / 2, 0), new Vector2(W / 2 * 0.95f, H * 0.62f), new Vector2(-W / 2 * 0.95f, H * 0.62f) };
            mb.AddExtrudedXY(lower, 18f, Vector3.zero, Quaternion.identity, 4f);
            // Two horns with the parabolic gap between them.
            for (int side = -1; side <= 1; side += 2)
            {
                var horn = new List<Vector2>();
                for (int i = 0; i <= 12; i++)
                {
                    float t = i / 12f;
                    float y = H * 0.62f + t * H * 0.38f;
                    float outer = side * Mathf.Lerp(W / 2 * 0.95f, W * 0.18f, t * t);
                    horn.Add(new Vector2(outer, y));
                }
                for (int i = 12; i >= 0; i--)
                {
                    float t = i / 12f;
                    float y = H * 0.62f + t * H * 0.38f;
                    float inner = side * Mathf.Lerp(W * 0.12f, W * 0.17f, t) * Mathf.Sqrt(Mathf.Max(0.05f, t));
                    horn.Add(new Vector2(inner, y));
                }
                if (side > 0) horn.Reverse();
                mb.AddExtrudedXY(horn, 14f, Vector3.zero, Quaternion.identity, 4f);
            }
            mb.AddBox(new Vector3(0, H * 0.86f, 0), new Vector3(W * 0.42f, 3f, 6f)); // sky bridge
            MeshFactory.FromBuilder("ArchTower", mb, mat, go.transform);
            MeshFactory.Box("BridgeGlow", go.transform, new Vector3(0, H * 0.86f, -3.1f), new Vector3(W * 0.4f, 1.2f, 0.2f), lights, false);
            NoShadows(go);
            return go;
        }

        /// <summary>Domed mausoleum with four minarets on a platform (Agra), reflected in a long pool by the cell.</summary>
        public static GameObject DomedPavilion(Transform parent, Vector3 pos, float yaw, Material mat)
        {
            var go = Root(parent, "DomedPavilion", pos, yaw, 1f);
            if (Model(go, "Landmark_DomedPavilion", Backdrop(mat))) return go;
            var mb = new MeshBuilder();
            mb.AddBox(new Vector3(0, 3, 0), new Vector3(70, 6, 70));                  // plinth
            mb.AddBox(new Vector3(0, 19, 0), new Vector3(40, 26, 40));                // main hall
            var dome = new List<Vector2>();
            for (int i = 0; i <= 14; i++)
            {
                float t = i / 14f;
                float a = t * Mathf.PI * 0.62f;
                float r = Mathf.Sin(a + 0.45f) * 15.5f * (1f - t * 0.25f);
                dome.Add(new Vector2(Mathf.Max(0.2f, r * (1f - Mathf.Pow(t, 6f))), 32f + t * 30f));
            }
            dome.Add(new Vector2(0.05f, 63.5f));
            mb.AddLathe(Vector3.zero, dome, 24);
            mb.AddCylinder(new Vector3(0, 32, 0), 12f, 4f, 24);                       // drum
            foreach (var c in new[] { new Vector3(32, 6, 32), new Vector3(-32, 6, 32), new Vector3(32, 6, -32), new Vector3(-32, 6, -32) })
            {
                mb.AddFrustum(c, 2.3f, 1.8f, 40f, 12);
                mb.AddSphere(c + Vector3.up * 42f, 2.4f, 10, 6, 1f, 0f, 90f);
            }
            foreach (var c in new[] { new Vector3(13, 32, 13), new Vector3(-13, 32, 13), new Vector3(13, 32, -13), new Vector3(-13, 32, -13) })
                mb.AddSphere(c + Vector3.up * 2f, 4f, 12, 6, 1f, 0f, 90f);
            MeshFactory.FromBuilder("Pavilion", mb, mat, go.transform);
            NoShadows(go);
            return go;
        }

        public static GameObject ClockTower(Transform parent, Vector3 pos, float yaw, Material mat, Material face)
        {
            var go = Root(parent, "ClockTowerSilhouette", pos, yaw, 1f);
            if (Model(go, "Landmark_ClockTower", Backdrop(mat, face, "TS_EmissiveWarm"))) return go; // the four clock faces
            var mb = new MeshBuilder();
            mb.AddBox(new Vector3(0, 30, 0), new Vector3(12, 60, 12));
            mb.AddBox(new Vector3(0, 66, 0), new Vector3(14, 12, 14));
            mb.AddBox(new Vector3(0, 74, 0), new Vector3(11, 4, 11));
            var spire = new List<Vector2> { new Vector2(7.5f, 76f), new Vector2(5.5f, 84f), new Vector2(2f, 96f), new Vector2(0.3f, 104f) };
            mb.AddLathe(Vector3.zero, spire, 4, 1f, 45f, 405f);
            MeshFactory.FromBuilder("Tower", mb, mat, go.transform);
            foreach (var r in new[] { 0f, 90f, 180f, 270f })
            {
                var q = Quaternion.Euler(0, r, 0);
                MeshFactory.Make("Face", MeshFactory.QuadMesh(9f, 9f), face, go.transform, q * new Vector3(0, 66, -7.05f), q);
            }
            NoShadows(go);
            return go;
        }

        /// <summary>Six-column gate with entablature and a block for the quadriga (Berlin framing).</summary>
        public static GameObject ColumnGate(Transform parent, Vector3 pos, float yaw, Material mat, float scale = 1f)
        {
            var go = Root(parent, "ColumnGate", pos, yaw, scale);
            if (Model(go, "Landmark_ColumnGate", Backdrop(mat))) return go;
            var mb = new MeshBuilder();
            for (int i = 0; i < 6; i++)
            {
                float x = -12.5f + i * 5f;
                mb.AddBox(new Vector3(x, 0.6f, 0), new Vector3(2.6f, 1.2f, 2.6f));
                mb.AddCylinder(new Vector3(x, 1.2f, 0), 0.95f, 13f, 16);
                mb.AddBox(new Vector3(x, 14.4f, 0), new Vector3(2.4f, 0.6f, 2.4f));
            }
            mb.AddBox(new Vector3(0, 15.8f, 0), new Vector3(30f, 2.2f, 5f));
            mb.AddBox(new Vector3(0, 17.9f, 0), new Vector3(28f, 2f, 4.5f));
            mb.AddBox(new Vector3(0, 19.8f, 0), new Vector3(9f, 1.8f, 3f));
            mb.AddBox(new Vector3(0, 21.5f, 0), new Vector3(5f, 1.6f, 2f));
            // Side wings.
            mb.AddBox(new Vector3(-19f, 6f, 0), new Vector3(6f, 12f, 4f));
            mb.AddBox(new Vector3(19f, 6f, 0), new Vector3(6f, 12f, 4f));
            MeshFactory.FromBuilder("Gate", mb, mat, go.transform, ColliderKind.None);
            NoShadows(go);
            return go;
        }

        public static GameObject Skyline(Transform parent, Vector3 center, float radius, float arcDegrees, float yaw, int count, Material mat, Material windowLights, int seed)
        {
            var go = Root(parent, "Skyline", center, yaw, 1f);
            var rnd = new System.Random(seed);
            for (int i = 0; i < count; i++)
            {
                float a = (-arcDegrees / 2 + arcDegrees * i / Mathf.Max(1, count - 1)) * Mathf.Deg2Rad;
                float r = radius + (float)rnd.NextDouble() * 40f;
                var p = new Vector3(Mathf.Sin(a) * r, 0, Mathf.Cos(a) * r);
                float h = 25f + (float)rnd.NextDouble() * 85f;
                float w = 10f + (float)rnd.NextDouble() * 16f;
                var t = MeshFactory.Box("Tower", go.transform, p + Vector3.up * h / 2, new Vector3(w, h, w), mat, false, 4f, Quaternion.Euler(0, (float)rnd.NextDouble() * 90f, 0));
                if (windowLights != null && rnd.NextDouble() < 0.7)
                {
                    int bands = 2 + rnd.Next(5);
                    for (int b = 0; b < bands; b++)
                        MeshFactory.Box("Lights", t.transform, new Vector3(0, -h / 2 + h * (0.18f + b * 0.14f), 0), new Vector3(w * 1.01f, 0.45f, w * 1.01f), windowLights, false);
                }
                if (rnd.NextDouble() < 0.25)
                    MeshFactory.Frustum("Spire", go.transform, p + Vector3.up * h, w * 0.3f, 0.2f, h * 0.25f, mat, 6);
            }
            NoShadows(go);
            return go;
        }
    }
}

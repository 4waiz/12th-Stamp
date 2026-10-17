using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwelfthStamp
{
    /// <summary>
    /// The horror "dimension shift" between places. The world around the player comes apart: debris lifts and circles,
    /// dark tears split the walls, the light sinks into a slow-pulsing red, whispers swell; on desktop a short streak
    /// tunnel opens. At the peak a dark-red fade hides the swap the caller makes (activate the next cell, place the
    /// player), then the new place fades in.
    /// Comfort (the brief's rules): the camera and the rig are never moved, shaken or turned; nothing flashes faster than
    /// about once a second; the tunnel lasts at most 1.5 s, never runs in VR or with reduced effects, and keeps the room
    /// visible through it; Gentle scare intensity gets a shorter, softer version. Skippable once it has been seen.
    /// </summary>
    public static class DimensionShift
    {
        const string SeenKey = "ts_shift_seen";
        public static bool Running { get; private set; }

        /// <summary>The player has seen a full shift before (then the skip input jumps straight to the swap).</summary>
        public static bool Seen
        {
            get { try { return PlayerPrefs.GetInt(SeenKey, 0) == 1; } catch { return false; } }
            set { try { PlayerPrefs.SetInt(SeenKey, value ? 1 : 0); PlayerPrefs.Save(); } catch { } }
        }

        public static bool Gentle => Game.Settings != null && Game.Settings.scareIntensity == 0;
        static bool Reduced => Game.Settings != null && Game.Settings.reducedEffects;

        /// <summary>The skip input (desktop Skip key, either VR trigger).</summary>
        public static bool SkipPressed()
        {
            var i = Game.Input;
            if (i == null) return false;
            if (i.Skip != null && i.Skip.WasPressedThisFrame()) return true;
            return Game.Mode == GameMode.VR && ((i.RTrigger != null && i.RTrigger.WasPressedThisFrame()) || (i.LTrigger != null && i.LTrigger.WasPressedThisFrame()));
        }

        /// <summary>
        /// Plays the shift around the player. <paramref name="swap"/> runs once at the peak, behind the fade.
        /// <paramref name="buildUp"/> is the chaos before the peak (Gentle halves it). Input is locked throughout.
        /// </summary>
        public static IEnumerator Run(System.Action swap, float buildUp = 4f)
        {
            Running = true;
            Game.InputLocks++;
            bool gentle = Gentle, reduced = Reduced, vr = Game.Mode == GameMode.VR;
            float build = gentle ? buildUp * 0.5f : buildUp;
            bool canSkip = Seen;
            var head = Game.Rig.Head;
            var root = new GameObject("DimensionShift").transform;
            root.position = new Vector3(head.position.x, Game.Rig.transform.position.y, head.position.z);

            var cell = Game.Cells.Active;
            var lights = cell != null ? cell.GetComponentsInChildren<Light>() : new Light[0];
            var baseIntensity = new float[lights.Length];
            for (int i = 0; i < lights.Length; i++) baseIntensity[i] = lights[i].intensity;

            var red = new GameObject("ShiftLight").AddComponent<Light>();
            red.transform.SetParent(root, false);
            red.transform.localPosition = Vector3.up * 2.2f;
            red.type = LightType.Point;
            red.color = new Color(1f, 0.16f, 0.1f);
            red.range = 9f;
            red.intensity = 0f;
            red.shadows = LightShadows.None;

            var guard = root.gameObject.AddComponent<Guard>();
            guard.Lights = lights;
            guard.Base = baseIntensity;
            guard.Beat = Time.time;
            var debris = Debris(root, head, gentle ? 22 : 56);
            var tears = Tears(root, head, gentle ? 3 : 7);
            Tunnel tunnel = null;
            if (!vr && !reduced && !gentle) tunnel = new Tunnel(root, head);

            var stretch = AudioDirector.I?.Loop("tear_stretch_loop", head, 0f, Bus.SFX, 0f);
            var heart = AudioDirector.I?.Loop("heartbeat_loop", head, 0f, Bus.SFX, 0f);
            AudioDirector.I?.Play("pass_sting", null, gentle ? 0.4f : 0.75f, 0.8f, Bus.SFX, true);

            float t = 0f;
            int whispers = 0;
            bool skipped = false;
            while (t < build)
            {
                if (canSkip && SkipPressed()) { skipped = true; break; }
                guard.Beat = Time.time;
                float dt = Time.deltaTime;
                t += dt;
                float k = Mathf.Clamp01(t / build);              // 0..1 build-up
                float pulse = 0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f * 0.7f); // 0.7 Hz: well under the flashing limit
                PostFx.I?.SetShift(k * (0.75f + 0.25f * pulse) * (gentle ? 0.6f : 1f));
                red.intensity = Mathf.Lerp(0f, gentle ? 1.1f : 2.2f, k) * (0.65f + 0.35f * pulse);
                for (int i = 0; i < lights.Length; i++)
                    if (lights[i] != null && lights[i] != red) lights[i].intensity = baseIntensity[i] * Mathf.Lerp(1f, 0.25f, k);
                if (stretch != null) { AudioDirector.I.SetBaseVolume(stretch, k * 0.7f); stretch.pitch = 0.7f + k * 0.6f; }
                if (heart != null) { AudioDirector.I.SetBaseVolume(heart, 0.3f + k * 0.5f); heart.pitch = 1f + k * 0.5f; }
                if (!gentle && whispers < 4 && t > build * (0.25f + whispers * 0.18f))
                {
                    var a = Random.Range(0f, Mathf.PI * 2f);
                    AudioDirector.I?.Play("pass_whisper", head.position + new Vector3(Mathf.Cos(a), 0.2f, Mathf.Sin(a)) * 2.5f, 0.6f, Random.Range(0.8f, 1.1f));
                    whispers++;
                }
                foreach (var d in debris) d.Step(root.position, k, dt);
                foreach (var tr in tears) tr.Step(k);
                if (tunnel != null) tunnel.Step(k, build, t, dt, head);
                yield return null;
            }

            // The peak: one dark-red fade (no flashing), the swap, then the new place fades in.
            AudioDirector.I?.Play("tear_breach", null, gentle ? 0.6f : 1f, 0.85f, Bus.SFX, true);
            Fader.I.SetColor(new Color(0.16f, 0.01f, 0.02f));
            Fader.I.FadeTo(1f, skipped ? 0.15f : 0.35f);
            yield return new WaitForSeconds(skipped ? 0.18f : 0.4f);
            guard.Restore();
            Object.Destroy(root.gameObject);
            if (stretch != null) AudioDirector.I.FadeOutAndStop(stretch, 0.4f);
            if (heart != null) AudioDirector.I.FadeOutAndStop(heart, 1.5f);
            try { swap?.Invoke(); }
            catch (System.Exception e) { Debug.LogException(e); }
            yield return null;
            Fader.I.FadeTo(0f, 0.9f);
            for (float f = 0f; f < 1f; f += Time.deltaTime / 1.2f) { PostFx.I?.SetShift((1f - f) * 0.6f); yield return null; }
            PostFx.I?.SetShift(0f);
            Fader.I.SetColor(Color.black);
            if (!skipped) Seen = true;
            Game.InputLocks--;
            Running = false;
        }

        /// <summary>
        /// Lives on the shift's root. If the coroutine stops beating (the player quits to the menu mid-shift), it puts the
        /// lights, the grade and the fade back and removes the debris, so nothing is left behind in the room.
        /// </summary>
        class Guard : MonoBehaviour
        {
            public Light[] Lights;
            public float[] Base;
            public float Beat;
            bool m_Restored;

            public void Restore()
            {
                if (m_Restored) return;
                m_Restored = true;
                for (int i = 0; i < Lights.Length; i++) if (Lights[i] != null) Lights[i].intensity = Base[i];
            }

            void Update()
            {
                // 2.5 s without a beat: the coroutine is gone (a hitch is never that long). The input lock is left alone:
                // returning to the menu resets it.
                if (Time.time - Beat < 2.5f || m_Restored) return;
                Restore();
                PostFx.I?.SetShift(0f);
                if (Fader.I != null) Fader.I.SetColor(Color.black);
                Running = false;
                Destroy(gameObject);
            }
        }

        // ------------------------------------------------------------------ debris: shards that lift and circle
        class Shard
        {
            public Transform T;
            float m_Angle, m_Radius, m_Height, m_Speed, m_Lift, m_Wobble;
            Vector3 m_Spin;

            public Shard(Transform t, float angle, float radius, float height)
            {
                T = t; m_Angle = angle; m_Radius = radius; m_Height = height;
                m_Speed = Random.Range(0.35f, 1.1f) * (Random.value < 0.5f ? -1f : 1f);
                m_Lift = Random.Range(0.6f, 1.8f);
                m_Wobble = Random.Range(0f, 10f);
                m_Spin = Random.onUnitSphere * Random.Range(40f, 160f);
            }

            public void Step(Vector3 center, float k, float dt)
            {
                m_Angle += m_Speed * k * k * dt * 1.6f;
                float r = m_Radius + Mathf.Sin(Time.time * 1.3f + m_Wobble) * 0.25f * k;
                float y = m_Height + m_Lift * k + Mathf.Sin(Time.time * 2f + m_Wobble) * 0.08f;
                T.position = center + new Vector3(Mathf.Cos(m_Angle) * r, y, Mathf.Sin(m_Angle) * r);
                T.Rotate(m_Spin * k * dt, Space.Self);
            }
        }

        static List<Shard> Debris(Transform root, Transform head, int count)
        {
            var list = new List<Shard>();
            var dark = Mats.Lit(new Color(0.07f, 0.06f, 0.06f), 0.6f, 0.2f, null, false, new Color(0.09f, 0.008f, 0.008f));
            var pale = Mats.Lit(new Color(0.55f, 0.52f, 0.48f), 0.3f, 0f, "plaster_warm");
            for (int i = 0; i < count; i++)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                float r = Random.Range(1.3f, 4.5f);   // never inside arm's reach of the face
                float h = Random.Range(0.1f, 2.2f);
                var size = new Vector3(Random.Range(0.08f, 0.5f), Random.Range(0.015f, 0.05f), Random.Range(0.06f, 0.4f));
                var go = MeshFactory.Box("Shard", root, Vector3.zero, size, Random.value < 0.7f ? dark : pale, false, 0.5f, Random.rotation);
                go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var s = new Shard(go.transform, a, r, h);
                s.Step(root.position, 0f, 0f);
                list.Add(s);
            }
            return list;
        }

        // ------------------------------------------------------------------ tears: dark splits on the walls
        class WallTear
        {
            public Transform T;
            Vector3 m_Full;
            public WallTear(Transform t, Vector3 full) { T = t; m_Full = full; t.localScale = Vector3.zero; }
            public void Step(float k)
            {
                float g = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((k - 0.15f) / 0.6f));
                T.localScale = new Vector3(m_Full.x * (0.3f + 0.7f * g), m_Full.y * g, 1f);
            }
        }

        static List<WallTear> Tears(Transform root, Transform head, int count)
        {
            var list = new List<WallTear>();
            var black = Mats.UnlitInstance(new Color(0.01f, 0f, 0.005f, 1f), Tex.TearMask, true, 1f);
            var glow = Mats.Particle(Tex.SoftDot, true, new Color(1f, 0.15f, 0.08f), 1.2f);
            int tries = 0;
            while (list.Count < count && tries++ < count * 6)
            {
                float a = Random.Range(0f, Mathf.PI * 2f);
                var dir = new Vector3(Mathf.Cos(a), Random.Range(-0.15f, 0.25f), Mathf.Sin(a)).normalized;
                if (!Physics.Raycast(head.position, dir, out var hit, 7f, ~0, QueryTriggerInteraction.Ignore)) continue;
                if (hit.distance < 1.2f || Mathf.Abs(hit.normal.y) > 0.4f) continue;
                var t = new GameObject("WallTear").transform;
                t.SetParent(root, true);
                t.position = hit.point + hit.normal * 0.02f;
                t.rotation = Quaternion.LookRotation(-hit.normal, Vector3.up) * Quaternion.Euler(0f, 0f, Random.Range(-20f, 20f));
                var w = Random.Range(0.25f, 0.6f);
                var h = Random.Range(1.1f, 2.2f);
                MeshFactory.Quad("Split", t, 1f, 1f, black, Vector3.zero, Quaternion.identity).GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                MeshFactory.Quad("Glow", t, 2.4f, 1.5f, glow, new Vector3(0f, 0f, 0.01f), Quaternion.identity).GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                list.Add(new WallTear(t, new Vector3(w, h, 1f)));
            }
            return list;
        }

        static Texture2D s_Dashes;

        /// <summary>Soft dashes that repeat along v (the tunnel streaks scroll through them).</summary>
        static Texture2D Dashes
        {
            get
            {
                if (s_Dashes != null) return s_Dashes;
                const int w = 16, h = 64;
                s_Dashes = new Texture2D(w, h, TextureFormat.RGBA32, true) { name = "TS_ShiftDashes", wrapMode = TextureWrapMode.Repeat };
                for (int y = 0; y < h; y++)
                    for (int x = 0; x < w; x++)
                    {
                        float dx = Mathf.Abs((x + 0.5f) / w * 2f - 1f);
                        float fy = (y + 0.5f) / h;
                        float dash = Mathf.Clamp01(Mathf.Sin(fy * Mathf.PI * 2f) * 1.6f);
                        s_Dashes.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - dx * 1.8f) * dash));
                    }
                s_Dashes.Apply();
                return s_Dashes;
            }
        }

        // ------------------------------------------------------------------ tunnel (desktop only, at most 1.5 s)
        class Tunnel
        {
            readonly GameObject m_Go;
            readonly Material m_Mat;
            float m_Scroll;

            public Tunnel(Transform root, Transform head)
            {
                // A ring of streaks along the view, fixed in the world (it never follows the head), opening around the room
                // instead of replacing it: the floor and walls stay visible between the streaks as a stable frame.
                m_Mat = new Material(Mats.Particle(Dashes, true, new Color(1f, 0.32f, 0.22f), 1.8f)) { name = "ShiftTunnel" };
                m_Mat.mainTextureScale = new Vector2(1f, 2f);
                var b = new MeshBuilder();
                const int Streaks = 40;
                var fwd = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                var right = Vector3.Cross(Vector3.up, fwd);
                var origin = head.position + fwd * 0.9f;
                for (int i = 0; i < Streaks; i++)
                {
                    float a = i / (float)Streaks * Mathf.PI * 2f + Random.Range(-0.05f, 0.05f);
                    float r = Random.Range(1.2f, 2f);
                    var o = (right * Mathf.Cos(a) + Vector3.up * Mathf.Sin(a)) * r;
                    var w = (right * -Mathf.Sin(a) + Vector3.up * Mathf.Cos(a)) * Random.Range(0.05f, 0.14f);
                    float len = Random.Range(4f, 9f);
                    var p = origin + o;
                    b.AddQuad(p - w, p - w + fwd * len, p + w + fwd * len, p + w, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f));
                }
                m_Go = MeshFactory.FromBuilder("ShiftTunnel", b, m_Mat, root);
                m_Go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                m_Go.SetActive(false);
            }

            public void Step(float k, float build, float t, float dt, Transform head)
            {
                float open = build - 1.5f;
                bool on = t >= open;
                if (m_Go.activeSelf != on) m_Go.SetActive(on);
                if (!on) return;
                float f = Mathf.Clamp01((t - open) / 1.5f);
                m_Scroll += dt * (1.5f + f * 2.5f); // dashes rush toward the viewer
                m_Mat.mainTextureOffset = new Vector2(0f, m_Scroll);
                m_Mat.SetColor("_TintColor", new Color(1f, 0.32f, 0.22f, 1f) * Mathf.Sin(f * Mathf.PI));
            }
        }
    }
}

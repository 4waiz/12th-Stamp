using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    public enum Bus { Music, Ambience, SFX, Voice, UI }

    [Serializable]
    public class AudioClipInfo
    {
        public string id;
        public string file;
        public string bus;
        public float duration;
        public bool loop;
        public string source;
        public string license;
        public string transcript;
        public string speaker;
    }

    [Serializable]
    class AudioManifest
    {
        public int version;
        public List<AudioClipInfo> clips = new List<AudioClipInfo>();
    }

    /// <summary>
    /// Software audio buses (Master, Music, Ambience, SFX, Voice, UI). Every source the game creates is registered
    /// with a bus; volume sliders change bus gains that are applied to all registered sources each frame. Voice
    /// ducks music and ambience. "Reduced dynamic range" lowers marked scare peaks and lifts quiet beds.
    /// Spatialisation is Unity's built-in 3D panning (no HRTF plug-in is installed).
    /// </summary>
    public class AudioDirector : MonoBehaviour
    {
        public static AudioDirector I { get; private set; }

        class Tracked
        {
            public AudioSource Source;
            public Bus Bus;
            public float BaseVolume;
            public bool Peak;
            public float Fade = 1f;
            public float FadeTarget = 1f;
            public float FadeSpeed;
            public bool DestroyWhenFaded;
        }

        readonly List<Tracked> m_Tracked = new List<Tracked>();
        readonly Dictionary<string, AudioClipInfo> m_Info = new Dictionary<string, AudioClipInfo>();
        readonly Dictionary<string, AudioClip> m_Clips = new Dictionary<string, AudioClip>();
        readonly HashSet<string> m_Missing = new HashSet<string>();
        readonly List<AudioSource> m_OneShotPool = new List<AudioSource>();

        Tracked m_AmbA, m_AmbB, m_MusA, m_MusB;
        string m_AmbId, m_MusId;
        float m_Duck = 1f;
        int m_VoiceActive;

        public static void Create(Transform parent)
        {
            if (I != null) return;
            var go = new GameObject("AudioDirector");
            go.transform.SetParent(parent, false);
            I = go.AddComponent<AudioDirector>();
            I.LoadManifest();
        }

        void LoadManifest()
        {
            var json = Resources.Load<TextAsset>("Audio/audio_manifest");
            if (json == null) { Debug.LogWarning("[TS-Audio] audio_manifest.json not found; clips load by convention"); return; }
            try
            {
                var m = JsonUtility.FromJson<AudioManifest>(json.text);
                foreach (var c in m.clips) if (!string.IsNullOrEmpty(c.id)) m_Info[c.id] = c;
                Debug.Log("[TS-Audio] Manifest clips: " + m_Info.Count);
            }
            catch (Exception e) { Debug.LogWarning("[TS-Audio] Bad manifest: " + e.Message); }
        }

        public AudioClipInfo Info(string id) => id != null && m_Info.TryGetValue(id, out var i) ? i : null;

        public AudioClip Clip(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (m_Clips.TryGetValue(id, out var c)) return c;
            if (m_Missing.Contains(id)) return null;
            string path = Info(id)?.file;
            if (string.IsNullOrEmpty(path))
            {
                foreach (var folder in new[] { "SFX", "Voice", "Ambience", "Music" })
                {
                    c = Resources.Load<AudioClip>("Audio/" + folder + "/" + id);
                    if (c != null) break;
                }
            }
            else c = Resources.Load<AudioClip>(path);
            if (c == null)
            {
                m_Missing.Add(id);
                Debug.LogWarning("[TS-Audio] Missing clip: " + id);
                return null;
            }
            m_Clips[id] = c;
            return c;
        }

        public bool Has(string id) => Clip(id) != null;

        // ------------------------------------------------------------------ gains
        public float BusGain(Bus bus)
        {
            var s = Game.Settings ?? new GameSettings();
            float g = s.master;
            switch (bus)
            {
                case Bus.Music: g *= s.music * m_Duck * (s.reducedDynamicRange ? 0.9f : 1f); break;
                case Bus.Ambience: g *= s.ambience * Mathf.Lerp(1f, m_Duck, 0.6f) * (s.reducedDynamicRange ? 1.25f : 1f); break;
                case Bus.SFX: g *= s.sfx; break;
                case Bus.Voice: g *= s.voice; break;
                case Bus.UI: g *= s.ui; break;
            }
            return g;
        }

        void Update()
        {
            float duckTarget = m_VoiceActive > 0 ? 0.45f : 1f;
            m_Duck = Mathf.MoveTowards(m_Duck, duckTarget, Time.unscaledDeltaTime * 1.6f);
            bool rdr = Game.Settings != null && Game.Settings.reducedDynamicRange;
            for (int i = m_Tracked.Count - 1; i >= 0; i--)
            {
                var t = m_Tracked[i];
                if (t.Source == null) { m_Tracked.RemoveAt(i); continue; }
                if (t.FadeSpeed > 0f)
                {
                    t.Fade = Mathf.MoveTowards(t.Fade, t.FadeTarget, t.FadeSpeed * Time.unscaledDeltaTime);
                    if (Mathf.Approximately(t.Fade, t.FadeTarget)) t.FadeSpeed = 0f;
                    if (t.DestroyWhenFaded && t.Fade <= 0.0001f)
                    {
                        t.Source.Stop();
                        if (!m_OneShotPool.Contains(t.Source)) Destroy(t.Source.gameObject);
                        m_Tracked.RemoveAt(i);
                        continue;
                    }
                }
                float peak = t.Peak && rdr ? 0.6f : 1f;
                t.Source.volume = t.BaseVolume * t.Fade * peak * BusGain(t.Bus);
            }
        }

        Tracked Track(AudioSource src, Bus bus, float vol, bool peak)
        {
            var t = new Tracked { Source = src, Bus = bus, BaseVolume = vol, Peak = peak };
            m_Tracked.Add(t);
            src.volume = vol * BusGain(bus);
            src.ignoreListenerPause = bus == Bus.Music || bus == Bus.UI;
            return t;
        }

        public void SetBaseVolume(AudioSource src, float vol)
        {
            foreach (var t in m_Tracked) if (t.Source == src) { t.BaseVolume = vol; return; }
        }

        public void FadeOutAndStop(AudioSource src, float seconds)
        {
            foreach (var t in m_Tracked)
                if (t.Source == src)
                {
                    t.FadeTarget = 0f; t.FadeSpeed = 1f / Mathf.Max(0.01f, seconds); t.DestroyWhenFaded = true;
                    return;
                }
            if (src != null) Destroy(src.gameObject);
        }

        // ------------------------------------------------------------------ playback
        AudioSource NewSource(string name, Transform parent, Vector3? pos, float spatial)
        {
            var go = new GameObject("Audio_" + name);
            if (parent != null) { go.transform.SetParent(parent, false); go.transform.localPosition = Vector3.zero; }
            else go.transform.SetParent(transform, false);
            if (pos.HasValue) go.transform.position = pos.Value;
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = spatial;
            src.rolloffMode = AudioRolloffMode.Logarithmic;
            src.minDistance = 1.2f;
            src.maxDistance = 30f;
            src.dopplerLevel = 0f;
            src.spread = 30f;
            return src;
        }

        /// <summary>Fire-and-forget sound. Returns the source (or null if the clip is missing).</summary>
        public AudioSource Play(string id, Vector3? position = null, float volume = 1f, float pitch = 1f, Bus bus = Bus.SFX, bool peak = false, Transform follow = null, float minDistance = 1.2f)
        {
            var clip = Clip(id);
            if (clip == null) return null;
            bool spatial = position.HasValue || follow != null;
            var src = NewSource(id, follow, position, spatial ? 1f : 0f);
            src.minDistance = minDistance;
            src.clip = clip;
            src.pitch = pitch;
            Track(src, bus, volume, peak);
            src.Play();
            Destroy(src.gameObject, clip.length / Mathf.Max(0.1f, Mathf.Abs(pitch)) + 0.2f);
            return src;
        }

        public AudioSource PlayUI(string id, float volume = 1f) => Play(id, null, volume, 1f, Bus.UI);

        /// <summary>Looping source that the caller controls (volume via SetBaseVolume, stop via FadeOutAndStop).</summary>
        public AudioSource Loop(string id, Transform parent, float volume, Bus bus = Bus.SFX, float spatial = 1f, float minDistance = 1.5f, float maxDistance = 25f)
        {
            var clip = Clip(id);
            if (clip == null) return null;
            var src = NewSource(id, parent, null, spatial);
            src.clip = clip;
            src.loop = true;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            Track(src, bus, volume, false);
            src.time = UnityEngine.Random.Range(0f, clip.length * 0.9f);
            src.Play();
            return src;
        }

        public void SetAmbience(string id, float fadeSeconds = 2.5f, float volume = 0.9f)
        {
            if (id == m_AmbId) return;
            m_AmbId = id;
            if (m_AmbA != null && m_AmbA.Source != null) FadeOutAndStop(m_AmbA.Source, fadeSeconds);
            m_AmbA = null;
            if (string.IsNullOrEmpty(id)) return;
            var src = Loop(id, transform, volume, Bus.Ambience, 0f);
            if (src == null) return;
            foreach (var t in m_Tracked) if (t.Source == src) { t.Fade = 0f; t.FadeTarget = 1f; t.FadeSpeed = 1f / fadeSeconds; m_AmbA = t; }
        }

        public void SetMusic(string id, float fadeSeconds = 3f, float volume = 0.8f, bool loop = true)
        {
            if (id == m_MusId) return;
            m_MusId = id;
            if (m_MusA != null && m_MusA.Source != null) FadeOutAndStop(m_MusA.Source, fadeSeconds);
            m_MusA = null;
            if (string.IsNullOrEmpty(id)) return;
            var src = Loop(id, transform, volume, Bus.Music, 0f);
            if (src == null) return;
            src.loop = loop;
            src.time = 0f;
            foreach (var t in m_Tracked) if (t.Source == src) { t.Fade = 0f; t.FadeTarget = 1f; t.FadeSpeed = 1f / fadeSeconds; m_MusA = t; }
        }

        public string CurrentAmbience => m_AmbId;

        /// <summary>Voice playback used by Narration (keeps ducking accurate).</summary>
        public AudioSource PlayVoice(AudioClip clip, Transform speaker)
        {
            var src = NewSource("voice", speaker, null, speaker != null ? 0.85f : 0f);
            src.clip = clip;
            src.minDistance = 2.5f;
            Track(src, Bus.Voice, 1f, false);
            src.Play();
            m_VoiceActive++;
            StartCoroutine(VoiceDone(src, clip.length));
            return src;
        }

        System.Collections.IEnumerator VoiceDone(AudioSource src, float length)
        {
            float t = 0f;
            while (src != null && (src.isPlaying || AudioListener.pause) && t < length + 30f)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
            m_VoiceActive = Mathf.Max(0, m_VoiceActive - 1);
            if (src != null) Destroy(src.gameObject);
        }

        public void StopVoice(AudioSource src)
        {
            if (src != null) src.Stop();
        }

        public static void Haptic(HandSide hand, float amplitude, float duration)
        {
            if (Game.Mode != GameMode.VR || hand == HandSide.Desktop) return;
            float a = Mathf.Clamp01(amplitude * (Game.Settings?.haptics ?? 1f));
            if (a <= 0f) return;
            var node = hand == HandSide.Left ? UnityEngine.XR.XRNode.LeftHand : UnityEngine.XR.XRNode.RightHand;
            var dev = UnityEngine.XR.InputDevices.GetDeviceAtXRNode(node);
            if (dev.isValid) dev.SendHapticImpulse(0, a, duration);
        }
    }
}

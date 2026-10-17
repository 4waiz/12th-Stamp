using System.Collections;
using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// The Passenger's arm pushing through an open tear from the other side ("open tears work both ways"). Rendered
    /// as ordinary world geometry: the part behind the tear plane is hidden by the opening's stencil clear, so the
    /// fingers appear to emerge from the hole. Sealing in time catches it (near miss); otherwise it breaches.
    /// </summary>
    public class PassengerHand : MonoBehaviour
    {
        Transform m_Arm;
        Transform[] m_Fingers = new Transform[4];
        Tear m_Tear;
        float m_Progress;
        bool m_Recoiling;
        AudioSource m_Tap;

        public float Progress => m_Progress;
        public bool Active => m_Tear != null && !m_Recoiling;

        public static PassengerHand Create(Transform parent)
        {
            var go = new GameObject("PassengerHand");
            go.transform.SetParent(parent, false);
            var h = go.AddComponent<PassengerHand>();
            h.Build();
            go.SetActive(false);
            return h;
        }

        void Build()
        {
            var glass = Mats.Lit(new Color(0.055f, 0.06f, 0.068f), 0.94f, 0.35f);
            var edge = Mats.Emissive(new Color(0.1f, 0.14f, 0.13f), new Color(0.16f, 0.42f, 0.36f), 0.9f);
            m_Arm = new GameObject("Arm").transform;
            m_Arm.SetParent(transform, false);
            var fore = new MeshBuilder();
            fore.AddFrustum(Vector3.zero, 0.04f, 0.032f, 0.75f, 7, true, true, 1f, Quaternion.Euler(90, 0, 0));
            MeshFactory.FromBuilder("Forearm", fore, glass, m_Arm).transform.localPosition = new Vector3(0, 0, -0.75f);
            MeshFactory.Box("Palm", m_Arm, new Vector3(0, 0, 0.04f), new Vector3(0.08f, 0.025f, 0.09f), glass, false);
            MeshFactory.Box("Edge", m_Arm, new Vector3(0.04f, 0.012f, -0.3f), new Vector3(0.003f, 0.003f, 0.6f), edge, false);
            for (int i = 0; i < 4; i++)
            {
                var f = new GameObject("Finger" + i).transform;
                f.SetParent(m_Arm, false);
                f.localPosition = new Vector3(-0.03f + i * 0.02f, 0, 0.085f);
                var tag = new MeshBuilder();
                tag.AddRoundedPlate(new Vector3(0, 0, 0), 0.014f, 0.15f, 0.006f, 0.004f, Quaternion.Euler(90, 0, 0), 3);
                var g = MeshFactory.FromBuilder("Tag", tag, glass, f);
                g.transform.localPosition = new Vector3(0, 0, 0.075f);
                MeshFactory.Box("Hole", f, new Vector3(0, 0, 0.135f), new Vector3(0.006f, 0.006f, 0.006f), edge, false);
                m_Fingers[i] = f;
            }
            MeshFactory.NoShadows(gameObject);
        }

        public void Attach(Tear tear)
        {
            m_Tear = tear;
            m_Recoiling = false;
            m_Progress = 0f;
            gameObject.SetActive(true);
            m_Tap = AudioDirector.I?.Loop("pass_tap", transform, 0.7f, Bus.SFX, 1f, 1.2f, 12f);
            Hud.I?.ShowCaption("glass fingertips tap at the edge of the tear", 3f);
        }

        /// <summary>0 = just behind the opening, 1 = fully through and gripping the edge.</summary>
        public void SetProgress(float p) => m_Progress = Mathf.Clamp01(p);

        void LateUpdate()
        {
            if (m_Tear == null) return;
            var a = m_Tear.Anchor;
            float w = m_Tear.Width;
            // Enter low on one side of the opening, reaching outwards along the seam's forward.
            var basePos = m_Tear.OpeningCenter + a.transform.right * Mathf.Min(0.12f, w * 0.15f) + Vector3.down * 0.15f;
            float depth = Mathf.Lerp(-0.35f, 0.55f, Mathf.SmoothStep(0f, 1f, m_Progress));
            transform.position = basePos + a.Forward * depth;
            transform.rotation = Quaternion.LookRotation(a.Forward, Vector3.up) * Quaternion.Euler(-12f + Mathf.Sin(Time.time * 7f) * 2f, Mathf.Sin(Time.time * 2.3f) * 6f, 0);
            float curl = Mathf.Lerp(-10f, 70f, Mathf.InverseLerp(0.6f, 1f, m_Progress)) + Mathf.Sin(Time.time * 9f) * 6f;
            for (int i = 0; i < 4; i++) m_Fingers[i].localRotation = Quaternion.Euler(curl + i * 4f, (i - 1.5f) * 6f, 0);
        }

        public void Recoil()
        {
            if (m_Tear == null || m_Recoiling) return;
            m_Recoiling = true;
            StartCoroutine(RecoilRoutine());
        }

        IEnumerator RecoilRoutine()
        {
            AudioDirector.I?.Play("pass_recoil", transform.position, 1f, 1.1f, Bus.SFX, true);
            if (m_Tap != null) { AudioDirector.I.FadeOutAndStop(m_Tap, 0.2f); m_Tap = null; }
            float start = m_Progress;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.3f;
                m_Progress = Mathf.Lerp(start, 0f, t);
                yield return null;
            }
            Detach();
        }

        public void Detach()
        {
            if (m_Tap != null) { AudioDirector.I?.FadeOutAndStop(m_Tap, 0.2f); m_Tap = null; }
            m_Tear = null;
            m_Recoiling = false;
            gameObject.SetActive(false);
        }
    }
}

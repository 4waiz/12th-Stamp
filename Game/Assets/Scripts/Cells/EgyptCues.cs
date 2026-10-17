using UnityEngine;

namespace TwelfthStamp
{
    /// <summary>
    /// World cues for Egypt's light puzzle (playtest: "when I am in Egypt what am I even supposed to do ... I put the light
    /// at the enemy and I'm just stuck"). The bronze reflector glints until it is first turned. The false shadow smoulders
    /// as the reflected beam swings near it, more the closer it gets (warmer, colder), and throws sparks while the beam is
    /// on it. The first time the beam drives the risen figure back, one line points at the real problem: the shadow.
    /// </summary>
    public class EgyptCues : MonoBehaviour
    {
        /// <summary>The guide can switch off the one-line nudge if it says the same thing.</summary>
        public static bool Nudges = true;

        const float WarmAngle = 32f, OnTarget = 8.5f;

        EgyptCell m_Cell;
        EgyptEncounter m_Enc;
        ParticleSystem m_Sparks;
        Transform m_Glint;
        Material m_GlintMat;
        bool m_Nudged, m_Subscribed;

        /// <summary>Degrees between the reflected beam and the false shadow (0 = on it).</summary>
        public float AimError { get; private set; } = 180f;
        /// <summary>The reflector has been turned at least once this visit.</summary>
        public bool ReflectorTurned { get; private set; }

        public void Setup(EgyptCell cell, EgyptEncounter enc)
        {
            m_Cell = cell;
            m_Enc = enc;
            ReflectorTurned = false;
            m_Nudged = false;
            if (m_Sparks == null) m_Sparks = MakeSparks();
            if (m_Glint == null) MakeGlint();
            m_Cell.Reflector.Changed -= OnTurned;
            m_Cell.Reflector.Changed += OnTurned;
            if (!m_Subscribed && Game.Passenger != null) { Game.Passenger.Repelled += OnRepelled; m_Subscribed = true; }
        }

        void OnDestroy()
        {
            if (m_Cell != null && m_Cell.Reflector != null) m_Cell.Reflector.Changed -= OnTurned;
            if (m_Subscribed && Game.Passenger != null) Game.Passenger.Repelled -= OnRepelled;
        }

        void OnTurned(StepRotator r, int index) => ReflectorTurned = true;

        void OnRepelled()
        {
            if (!Nudges || m_Nudged || m_Enc == null || !m_Enc.IsLive || m_Enc.Revealed || !m_Enc.MummyRisen) return;
            if (Game.Campaign.IsVerified(CountryId.Egypt)) return;
            m_Nudged = true;
            Hud.I?.ShowToast("The light only holds it back. The real wrong thing here is the shadow by the pillar.", 4.5f);
        }

        void Update()
        {
            if (m_Cell == null || m_Enc == null) return;
            bool live = m_Enc.IsLive && !m_Enc.Revealed && !Game.Campaign.IsVerified(CountryId.Egypt);
            var beam = m_Cell.ReflectorBeam != null ? m_Cell.ReflectorBeam.transform : null;
            if (beam != null)
            {
                var to = m_Cell.transform.TransformPoint(m_Cell.ShadowTargetLocal) - beam.position;
                to.y = 0f;
                var f = beam.forward; f.y = 0f;
                AimError = to.sqrMagnitude > 1e-4f && f.sqrMagnitude > 1e-4f ? Vector3.Angle(f, to) : 180f;
            }

            // Warmer, colder: the shadow's edges smoulder as the beam swings near, and spark while it is on target.
            var em = m_Sparks.emission;
            if (live && AimError < WarmAngle)
            {
                float k = 1f - AimError / WarmAngle;
                em.rateOverTime = AimError < OnTarget ? 46f : Mathf.Lerp(1.5f, 18f, k * k);
            }
            else em.rateOverTime = 0f;

            // The reflector glints until it has been turned once (it is the thing to use here).
            bool glint = live && !ReflectorTurned && beam != null;
            if (m_Glint.gameObject.activeSelf != glint) m_Glint.gameObject.SetActive(glint);
            if (glint)
            {
                var cam = Game.Rig != null ? Game.Rig.Cam : null;
                m_Glint.position = beam.position + Vector3.up * 0.05f;
                if (cam != null) m_Glint.rotation = Quaternion.LookRotation(m_Glint.position - cam.transform.position, Vector3.up);
                float pulse = Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Time.time * 2.4f)), 6f);
                m_Glint.localScale = Vector3.one * Mathf.Lerp(0.18f, 0.62f, pulse);
                m_GlintMat.SetColor("_Color", new Color(1f, 0.86f, 0.55f, Mathf.Lerp(0.15f, 0.95f, pulse)));
            }
        }

        ParticleSystem MakeSparks()
        {
            var go = new GameObject("ShadowSmoulder");
            go.transform.SetParent(m_Cell.transform, false);
            go.transform.localPosition = m_Cell.ShadowTargetLocal + Vector3.up * 0.04f;
            // Same footprint as the shadow quad (1 x 3.2 m, turned 212 degrees).
            go.transform.localRotation = Quaternion.Euler(0f, m_Cell.WrongShadow.localEulerAngles.y, 0f);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.3f);
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.62f, 0.25f, 1f), new Color(1f, 0.85f, 0.5f, 1f));
            main.maxParticles = 300;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var em = ps.emission; em.rateOverTime = 0f;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Box; shape.scale = new Vector3(0.9f, 0.02f, 3f);
            var vel = ps.velocityOverLifetime; vel.enabled = true; vel.space = ParticleSystemSimulationSpace.World;
            vel.x = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            vel.y = new ParticleSystem.MinMaxCurve(0.35f, 1.1f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.12f, 0.12f);
            var col = ps.colorOverLifetime; col.enabled = true;
            var g = new Gradient();
            g.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.4f, 0.15f), 1f) },
                      new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.1f), new GradientAlphaKey(0f, 1f) });
            col.color = g;
            var r = go.GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.sharedMaterial = Mats.Particle(Tex.SoftDot, true, Color.white, 1.8f, Mats.StencilNotEqual);
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            ps.Play();
            return ps;
        }

        void MakeGlint()
        {
            m_GlintMat = Mats.UnlitInstance(new Color(1f, 0.86f, 0.55f, 0f), Tex.SoftDot, true, 2.2f);
            m_GlintMat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            m_GlintMat.SetFloat("_ZWrite", 0f);
            var q = MeshFactory.Quad("ReflectorGlint", m_Cell.transform, 1f, 1f, m_GlintMat, Vector3.zero, Quaternion.identity);
            q.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            m_Glint = q.transform;
            m_Glint.gameObject.SetActive(false);
        }
    }
}

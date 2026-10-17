using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    public struct PointerState
    {
        public Ray Ray;
        public bool Down;
        public bool Held;
        public bool Up;
        public HandSide Hand;
    }

    /// <summary>Common surface of the desktop and VR rigs. Exactly one rig (and one AudioListener) exists.</summary>
    public abstract class PlayerRig : MonoBehaviour
    {
        public Camera Cam { get; protected set; }
        public Transform Head => Cam.transform;

        public abstract IReadOnlyList<Interactor> Hands { get; }
        public abstract Interactor MainHand { get; }
        /// <summary>Where the open passport sits (camera-relative on desktop, passport hand in VR).</summary>
        public abstract Transform PassportMount { get; }
        /// <summary>Ray used to pick seam anchors for tears.</summary>
        public abstract Ray TearAimRay { get; }

        public Vector3 Feet => new Vector3(Head.position.x, transform.position.y, Head.position.z);
        public Vector3 Forward
        {
            get
            {
                var f = Head.forward; f.y = 0f;
                return f.sqrMagnitude < 1e-4f ? transform.forward : f.normalized;
            }
        }

        /// <summary>Move so the player's feet are at <paramref name="feet"/> facing <paramref name="yaw"/>.</summary>
        public abstract void PlaceAt(Vector3 feet, float yaw);

        public virtual void ApplySettings() { }

        public abstract void GetPointers(List<PointerState> pointers);

        /// <summary>Shows the cursor (desktop) or laser pointers (VR) for 3D menus.</summary>
        public abstract void SetPointerMode(bool on);

        public float HoldRemoveGlasses { get; protected set; }

        protected void ConfigureCamera(Camera cam)
        {
            cam.nearClipPlane = 0.04f;
            cam.farClipPlane = 600f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.06f);
            cam.allowMSAA = true;
            cam.allowHDR = true;
            var data = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            if (data == null) data = cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.None;
            data.renderShadows = true;
            data.requiresDepthOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
            data.requiresColorOption = UnityEngine.Rendering.Universal.CameraOverrideOption.Off;
        }

        /// <summary>Footstep sounds keyed by the floor's surface tag (set by cell builders via name prefix).</summary>
        protected float m_StepDistance;
        protected Vector3 m_LastFeet;

        protected void UpdateFootsteps(float stride = 0.62f)
        {
            var feet = Feet;
            var delta = feet - m_LastFeet; delta.y = 0;
            m_LastFeet = feet;
            if (delta.magnitude > 1.5f) return; // teleported
            m_StepDistance += delta.magnitude;
            if (m_StepDistance < stride) return;
            m_StepDistance = 0f;
            string surface = "stone";
            if (Physics.Raycast(feet + Vector3.up * 0.4f, Vector3.down, out var hit, 1.2f, Layers.FloorMask | 1, QueryTriggerInteraction.Ignore))
                surface = Surface.Of(hit.collider);
            AudioDirector.I?.Play("foot_" + surface + "_" + Random.Range(1, 5), feet, 0.35f, Random.Range(0.94f, 1.06f));
        }
    }

    /// <summary>Surface type for footsteps, read from a tag component on floor colliders.</summary>
    public class Surface : MonoBehaviour
    {
        public string Kind = "stone";
        public static string Of(Collider c)
        {
            var s = c != null ? c.GetComponentInParent<Surface>() : null;
            return s != null ? s.Kind : "stone";
        }
    }
}

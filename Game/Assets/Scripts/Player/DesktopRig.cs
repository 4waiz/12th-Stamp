using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TwelfthStamp
{
    /// <summary>Keyboard + mouse first-person rig. Never touches XR.</summary>
    public class DesktopRig : PlayerRig
    {
        CharacterController m_Controller;
        Interactor m_Hand;
        Transform m_PassportMount;
        float m_Pitch;
        float m_Yaw;
        float m_VerticalSpeed;
        bool m_Pointer;
        readonly List<Interactor> m_Hands = new List<Interactor>();

        public const float EyeHeight = 1.62f;
        public float WalkSpeed = 2.3f;

        public override IReadOnlyList<Interactor> Hands => m_Hands;
        public override Interactor MainHand => m_Hand;
        public override Transform PassportMount => m_PassportMount;
        public override Ray TearAimRay => new Ray(Head.position, Head.forward);

        public static DesktopRig Create(Transform parent)
        {
            var go = new GameObject("DesktopRig");
            go.transform.SetParent(parent, false);
            go.layer = Layers.PlayerBody;
            var rig = go.AddComponent<DesktopRig>();
            rig.Build();
            return rig;
        }

        void Build()
        {
            m_Controller = gameObject.AddComponent<CharacterController>();
            m_Controller.height = 1.75f;
            m_Controller.radius = 0.28f;
            m_Controller.center = new Vector3(0, 0.875f, 0);
            m_Controller.stepOffset = 0.32f;
            m_Controller.slopeLimit = 50f;
            m_Controller.skinWidth = 0.03f;

            var head = new GameObject("Head");
            head.transform.SetParent(transform, false);
            head.transform.localPosition = new Vector3(0, EyeHeight, 0);
            Cam = head.AddComponent<Camera>();
            Cam.fieldOfView = 72f;
            ConfigureCamera(Cam);
            head.AddComponent<AudioListener>();
            head.tag = "MainCamera";

            var hold = new GameObject("HoldPoint").transform;
            hold.SetParent(head.transform, false);
            hold.localPosition = new Vector3(0.2f, -0.2f, 0.48f);

            m_Hand = head.AddComponent<Interactor>();
            m_Hand.Side = HandSide.Desktop;
            m_Hand.HoldPoint = hold;
            m_Hand.RayOrigin = head.transform;
            m_Hands.Add(m_Hand);

            m_PassportMount = new GameObject("PassportMount").transform;
            m_PassportMount.SetParent(head.transform, false);
            m_PassportMount.localPosition = new Vector3(-0.19f, -0.155f, 0.37f); // lower left: keeps the reticle and the right-hand card clear
            m_PassportMount.localRotation = Quaternion.Euler(21f, -26f, 0f); // page faces the eyes
            m_LastFeet = transform.position;
            SetPointerMode(false);
        }

        public override void PlaceAt(Vector3 feet, float yaw)
        {
            m_Controller.enabled = false;
            transform.position = feet;
            m_Yaw = yaw;
            m_Pitch = 0f;
            transform.rotation = Quaternion.Euler(0, yaw, 0);
            Head.localRotation = Quaternion.identity;
            m_Controller.enabled = true;
            m_VerticalSpeed = 0f;
            m_LastFeet = Feet;
        }

        /// <summary>Tilts the view up or down (story shots frame their subject while input is locked).</summary>
        public void SetPitch(float pitch)
        {
            m_Pitch = Mathf.Clamp(pitch, -82f, 82f);
            Head.localRotation = Quaternion.Euler(m_Pitch, 0, 0);
        }

        public override void SetPointerMode(bool on)
        {
            m_Pointer = on;
            Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = on;
        }

        public override void GetPointers(List<PointerState> pointers)
        {
            var input = Game.Input;
            var mouse = Mouse.current;
            Vector2 pos = mouse != null ? mouse.position.ReadValue() : new Vector2(Screen.width / 2f, Screen.height / 2f);
            var ray = m_Pointer ? Cam.ScreenPointToRay(pos) : new Ray(Head.position, Head.forward);
            pointers.Add(new PointerState
            {
                Ray = ray,
                Down = input.Click.WasPressedThisFrame(),
                Held = input.Click.IsPressed(),
                Up = input.Click.WasReleasedThisFrame(),
                Hand = HandSide.Desktop
            });
        }

        void Update()
        {
            var input = Game.Input;
            if (input == null) return;

            // Focus loss: free the cursor so the player can leave the window; the pause menu handles the rest.
            if (!Application.isFocused && !m_Pointer) Cursor.lockState = CursorLockMode.None;
            else if (Application.isFocused && !m_Pointer && Cursor.lockState != CursorLockMode.Locked) Cursor.lockState = CursorLockMode.Locked;

            bool canAct = !Game.InputLocked && !Game.InMenu;

            if (canAct)
            {
                float sens = 0.075f * (Game.Settings?.mouseSensitivity ?? 1f);
                var look = input.Look.ReadValue<Vector2>();
                m_Yaw += look.x * sens;
                m_Pitch -= look.y * sens * ((Game.Settings?.invertY ?? false) ? -1f : 1f);
                m_Pitch = Mathf.Clamp(m_Pitch, -82f, 82f);
                transform.rotation = Quaternion.Euler(0, m_Yaw, 0);
                Head.localRotation = Quaternion.Euler(m_Pitch, 0, 0);
            }

            var move = canAct ? input.Move.ReadValue<Vector2>() : Vector2.zero;
            var dir = transform.right * move.x + transform.forward * move.y;
            if (dir.sqrMagnitude > 1f) dir.Normalize();
            if (m_Controller.isGrounded) m_VerticalSpeed = -1f; else m_VerticalSpeed -= 9.81f * Time.deltaTime;
            if (m_Controller.enabled && Time.deltaTime > 0f)
                m_Controller.Move((dir * WalkSpeed + Vector3.up * m_VerticalSpeed) * Time.deltaTime);
            UpdateFootsteps(0.66f);

            m_Hand.UpdateFocus(canAct);
            if (!canAct) { HoldRemoveGlasses = 0f; return; }

            if (input.Interact.WasPressedThisFrame()) m_Hand.PressUse();
            if (input.PassportToggle.WasPressedThisFrame()) Game.Passport?.Toggle(HandSide.Desktop);

            var passportOpen = Game.Passport != null && Game.Passport.IsOpen;
            if (passportOpen)
            {
                float scroll = input.DestScroll.ReadValue<float>();
                if (scroll > 0.1f) Game.Passport.CycleDestination(-1);
                else if (scroll < -0.1f) Game.Passport.CycleDestination(1);
                if (input.DestPrev.WasPressedThisFrame()) Game.Passport.CycleDestination(-1);
                if (input.DestNext.WasPressedThisFrame()) Game.Passport.CycleDestination(1);
            }

            if (Game.Tears != null)
            {
                if (passportOpen && input.Tear.IsPressed()) Game.Tears.HoldWiden(Time.deltaTime, HandSide.Desktop);
                else Game.Tears.ReleaseWiden();
                if (input.Seal.IsPressed()) Game.Tears.HoldSeal(Time.deltaTime, HandSide.Desktop);
                else Game.Tears.ReleaseSeal();
            }

            if (!passportOpen && input.Tear.WasPressedThisFrame() && m_Hand.Held != null && m_Hand.Held.OnActivate != null)
                m_Hand.Held.OnActivate(m_Hand.Held, m_Hand);

            if (input.Recall.WasPressedThisFrame()) Game.Story?.RecallEssential(m_Hand);
            if (input.Objectives.WasPressedThisFrame()) Hud.I?.ToggleObjectives();

            if (input.RemoveGlasses.IsPressed()) HoldRemoveGlasses += Time.deltaTime;
            else HoldRemoveGlasses = 0f;
            if (HoldRemoveGlasses >= 2f) { HoldRemoveGlasses = 0f; Game.Story?.RequestEarlyGlassesRemoval(); }
        }

        public override void ApplySettings()
        {
            if (Cam != null) Cam.fieldOfView = 72f;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace TwelfthStamp
{
    /// <summary>
    /// Quest 3 (PC VR over Link, OpenXR) rig: tracked head and controllers, teleport + snap turn by default,
    /// optional smooth locomotion/turn, left-handed swap, seated offset, head-in-wall fade, controller callouts.
    /// Scenery moves, the player's viewpoint is never driven by scripts (only by teleport/turn the player chose).
    /// </summary>
    public class VRRig : PlayerRig
    {
        Transform m_Offset;
        Transform m_LeftGrip, m_RightGrip, m_LeftAim, m_RightAim;
        Interactor m_Left, m_Right;
        ControllerModel m_LeftModel, m_RightModel;
        Transform m_PassportMountL, m_PassportMountR;
        LineRenderer m_LeftLaser, m_RightLaser, m_Arc;
        Transform m_Reticle;
        CharacterController m_Body;
        readonly List<Interactor> m_Hands = new List<Interactor>();
        readonly List<Vector3> m_ArcPoints = new List<Vector3>();
        bool m_Aiming, m_ArcValid, m_TurnArmed = true, m_Pointer;
        float m_RunHold; // seconds the move stick has been pushed fully forward (VR run)
        Vector3 m_ArcTarget;
        float m_DestinationRepeat;
        bool m_HeadBlocked;

        public override IReadOnlyList<Interactor> Hands => m_Hands;
        public override Interactor MainHand => Game.Input.DominantHand == HandSide.Left ? m_Left : m_Right;
        public Interactor PassportHandInteractor => Game.Input.PassportHand == HandSide.Left ? m_Left : m_Right;
        public override Transform PassportMount => Game.Input.PassportHand == HandSide.Left ? m_PassportMountL : m_PassportMountR;
        public Transform GripOf(HandSide h) => h == HandSide.Left ? m_LeftGrip : m_RightGrip;
        public ControllerModel ModelOf(HandSide h) => h == HandSide.Left ? m_LeftModel : m_RightModel;

        public override Ray TearAimRay
        {
            get
            {
                // Aim tears with the dominant hand's pointer (the passport hand is busy holding the book).
                var aim = Game.Input.DominantHand == HandSide.Left ? m_LeftAim : m_RightAim;
                return new Ray(aim.position, aim.forward);
            }
        }

        public static VRRig Create(Transform parent)
        {
            var go = new GameObject("VRRig");
            go.transform.SetParent(parent, false);
            go.layer = Layers.PlayerBody;
            var rig = go.AddComponent<VRRig>();
            rig.Build();
            return rig;
        }

        void Build()
        {
            var input = Game.Input;
            m_Body = gameObject.AddComponent<CharacterController>();
            m_Body.height = 1.6f; m_Body.radius = 0.22f; m_Body.center = new Vector3(0, 0.8f, 0);
            m_Body.stepOffset = 0.3f; m_Body.skinWidth = 0.02f;
            m_Body.minMoveDistance = 0f; // a gentle stick push at 90-120 Hz must still move (default 1 mm minimum)

            m_Offset = new GameObject("CameraOffset").transform;
            m_Offset.SetParent(transform, false);

            var head = new GameObject("Head");
            head.transform.SetParent(m_Offset, false);
            head.transform.localPosition = new Vector3(0, 1.6f, 0);
            Cam = head.AddComponent<Camera>();
            ConfigureCamera(Cam);
            head.AddComponent<AudioListener>();
            head.tag = "MainCamera";
            var tpd = head.AddComponent<TrackedPoseDriver>();
            tpd.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            tpd.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            tpd.positionInput = new InputActionProperty(input.HeadPos);
            tpd.rotationInput = new InputActionProperty(input.HeadRot);

            m_LeftGrip = TrackedChild("LeftGrip", input.LPos, input.LRot);
            m_RightGrip = TrackedChild("RightGrip", input.RPos, input.RRot);
            m_LeftAim = TrackedChild("LeftAim", input.LAimPos, input.LAimRot);
            m_RightAim = TrackedChild("RightAim", input.RAimPos, input.RAimRot);

            m_Left = MakeHand(HandSide.Left, m_LeftGrip, m_LeftAim);
            m_Right = MakeHand(HandSide.Right, m_RightGrip, m_RightAim);
            m_LeftModel = ControllerModel.Create(m_LeftGrip, HandSide.Left);
            m_RightModel = ControllerModel.Create(m_RightGrip, HandSide.Right);
            // Hands are drawn at the grips; the controller models sit in the same hand frame and show (ghosted) only with their labels.
            m_LeftModel.transform.localRotation = VRHands.GripToModel;
            m_RightModel.transform.localRotation = VRHands.GripToModel;
            VRHands.Create(this);

            m_PassportMountL = Mount(m_LeftGrip, false);
            m_PassportMountR = Mount(m_RightGrip, true);

            m_LeftLaser = Laser("LeftLaser");
            m_RightLaser = Laser("RightLaser");
            m_Arc = Laser("TeleportArc");
            m_Arc.widthMultiplier = 0.012f;

            var ret = MeshFactory.Disc("TeleportReticle", 0.22f, Mats.Unlit(new Color(0.55f, 0.9f, 0.8f, 0.8f), Tex.Ring, true));
            ret.transform.SetParent(transform, false);
            m_Reticle = ret.transform;
            m_Reticle.gameObject.SetActive(false);
            m_LastFeet = Feet;
            ApplySettings();
        }

        Transform TrackedChild(string name, InputAction pos, InputAction rot)
        {
            var t = new GameObject(name).transform;
            t.SetParent(m_Offset, false);
            var d = t.gameObject.AddComponent<TrackedPoseDriver>();
            d.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            d.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            d.positionInput = new InputActionProperty(pos);
            d.rotationInput = new InputActionProperty(rot);
            return t;
        }

        Interactor MakeHand(HandSide side, Transform grip, Transform aim)
        {
            var hold = new GameObject("HoldPoint").transform;
            hold.SetParent(grip, false);
            // Authored in the controller model's frame (y up, z forward as held); same alignment as the model and hands.
            hold.localPosition = VRHands.GripToModel * new Vector3(0, -0.01f, 0.05f);
            hold.localRotation = VRHands.GripToModel;
            var it = grip.gameObject.AddComponent<Interactor>();
            it.Side = side;
            it.HoldPoint = hold;
            it.RayOrigin = aim;
            m_Hands.Add(it);
            return it;
        }

        static Transform Mount(Transform grip, bool mirrored)
        {
            var t = new GameObject("PassportMount").transform;
            t.SetParent(grip, false);
            // Authored in the controller model's frame: in front of and above the controller as held, tilted to the eyes.
            t.localPosition = VRHands.GripToModel * new Vector3(mirrored ? -0.02f : 0.02f, 0.07f, 0.07f);
            t.localRotation = VRHands.GripToModel * Quaternion.Euler(48f, mirrored ? -12f : 12f, 0f);
            return t;
        }

        LineRenderer Laser(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.widthMultiplier = 0.004f;
            lr.positionCount = 2;
            lr.material = Mats.Unlit(new Color(0.75f, 0.95f, 0.88f, 0.55f), null, true);
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.enabled = false;
            return lr;
        }

        public override void ApplySettings()
        {
            if (m_Offset == null) return;
            var s = Game.Settings;
            m_Offset.localPosition = new Vector3(0, s.seated ? s.seatedHeight : 0f, 0);
        }

        public override void PlaceAt(Vector3 feet, float yaw)
        {
            float headYaw = Head.eulerAngles.y;
            transform.RotateAround(new Vector3(Head.position.x, transform.position.y, Head.position.z), Vector3.up, yaw - headYaw);
            var f = Feet;
            transform.position += new Vector3(feet.x - f.x, feet.y - transform.position.y, feet.z - f.z);
            m_LastFeet = Feet;
        }

        public override void SetPointerMode(bool on) => m_Pointer = on;

        public override void GetPointers(List<PointerState> pointers)
        {
            var input = Game.Input;
            pointers.Add(new PointerState { Ray = new Ray(m_LeftAim.position, m_LeftAim.forward), Down = input.LTrigger.WasPressedThisFrame(), Held = input.LTrigger.IsPressed(), Up = input.LTrigger.WasReleasedThisFrame(), Hand = HandSide.Left });
            pointers.Add(new PointerState { Ray = new Ray(m_RightAim.position, m_RightAim.forward), Down = input.RTrigger.WasPressedThisFrame(), Held = input.RTrigger.IsPressed(), Up = input.RTrigger.WasReleasedThisFrame(), Hand = HandSide.Right });
            // Spectator keyboard/mouse can also drive menus on the desktop mirror.
            if (input.Click.WasPressedThisFrame() || input.Click.IsPressed() || input.Click.WasReleasedThisFrame())
            {
                var mouse = Mouse.current;
                if (mouse != null)
                    pointers.Add(new PointerState { Ray = Cam.ScreenPointToRay(mouse.position.ReadValue()), Down = input.Click.WasPressedThisFrame(), Held = input.Click.IsPressed(), Up = input.Click.WasReleasedThisFrame(), Hand = HandSide.Desktop });
            }
        }

        void Update()
        {
            var input = Game.Input;
            if (input == null) return;
            bool canAct = !Game.InputLocked && !Game.InMenu;

            UpdateHeadBlock();
            UpdateLocomotion(canAct);
            // Longer strides as the stick run builds (2.0 m/s walk, 3.3 m/s run), so running steps don't patter.
            UpdateFootsteps(Mathf.Lerp(0.75f, 1.15f, Mathf.InverseLerp(2f, 3.3f, MoveSpeed)));

            foreach (var h in m_Hands) h.UpdateFocus(canAct && !(Game.Passport != null && Game.Passport.IsHeldBy(h.Side)));
            UpdateLasers(canAct);

            if (input.Menu.WasPressedThisFrame() || input.Pause.WasPressedThisFrame()) MenuSystem.I?.TogglePause();
            if (!canAct) { HoldRemoveGlasses = 0f; return; }

            // Hands: grip = grab, trigger = use.
            HandButtons(m_Left, input.LGrip, input.LTrigger);
            HandButtons(m_Right, input.RGrip, input.RTrigger);

            if (input.PassportButton.WasPressedThisFrame()) Game.Passport?.Toggle(input.PassportHand);
            bool passportOpen = Game.Passport != null && Game.Passport.IsOpen;
            if (passportOpen)
            {
                if (input.DestinationButton.WasPressedThisFrame()) Game.Passport.CycleDestination(1);
                float v = input.TurnStick.ReadValue<Vector2>().y;
                m_DestinationRepeat -= Time.deltaTime;
                if (Mathf.Abs(v) > 0.7f && m_DestinationRepeat <= 0f)
                {
                    Game.Passport.CycleDestination(v > 0 ? -1 : 1);
                    m_DestinationRepeat = 0.35f;
                }
                else if (Mathf.Abs(v) < 0.3f) m_DestinationRepeat = 0f;
            }

            if (Game.Tears != null)
            {
                // One-handed alternative: hold the dominant trigger while aiming at a seam (passport open).
                var dominantTrigger = input.TriggerOf(input.DominantHand);
                if (passportOpen && dominantTrigger.IsPressed() && Game.Tears.HasTarget) Game.Tears.HoldWiden(Time.deltaTime, input.DominantHand);
                else if (!Game.Tears.IsPulling) Game.Tears.ReleaseWiden();
                if (input.SealButton.IsPressed()) Game.Tears.HoldSeal(Time.deltaTime, input.DominantHand);
                else Game.Tears.ReleaseSeal();
            }

            if (input.RecallButton.WasPressedThisFrame()) Game.Story?.RecallEssential(MainHand);
        }

        void HandButtons(Interactor hand, InputAction grip, InputAction trigger)
        {
            bool passportHolder = Game.Passport != null && Game.Passport.IsHeldBy(hand.Side);
            if (grip.WasPressedThisFrame())
            {
                if (Game.Passport != null && Game.Passport.TryBeginSeamPull(hand)) { }
                else if (!passportHolder) hand.PressGrab();
            }
            if (grip.WasReleasedThisFrame())
            {
                if (Game.Passport != null && Game.Passport.IsPulling(hand)) Game.Passport.EndSeamPull(hand);
                else hand.ReleaseGrab();
            }
            if (trigger.WasPressedThisFrame() && !passportHolder)
            {
                bool tearUse = Game.Passport != null && Game.Passport.IsOpen && Game.Tears != null && Game.Tears.HasTarget && hand.Side == Game.Input.DominantHand;
                if (!tearUse) hand.PressUse();
            }
        }

        void UpdateLasers(bool canAct)
        {
            DrawLaser(m_LeftLaser, m_Left, m_LeftAim, canAct);
            DrawLaser(m_RightLaser, m_Right, m_RightAim, canAct);
        }

        void DrawLaser(LineRenderer lr, Interactor hand, Transform aim, bool canAct)
        {
            bool show = m_Pointer || (canAct && hand.Focused != null && !hand.FocusIsNear);
            bool tearAim = canAct && Game.Passport != null && Game.Passport.IsOpen && hand.Side == Game.Input.DominantHand;
            if (tearAim) show = true;
            lr.enabled = show;
            if (!show) return;
            var start = aim.position;
            Vector3 end;
            if (m_Pointer && Physics.Raycast(aim.position, aim.forward, out var hit, 8f, Layers.UIMask, QueryTriggerInteraction.Collide)) end = hit.point;
            else if (tearAim && Game.Tears != null && Game.Tears.HasTarget) end = Game.Tears.TargetPoint;
            else end = hand.RayHitSomething ? hand.LastRayEnd : aim.position + aim.forward * (m_Pointer ? 3f : 1.5f);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        // ------------------------------------------------------------------ locomotion
        void UpdateLocomotion(bool canAct)
        {
            var input = Game.Input;
            var s = Game.Settings;
            Vector2 move = canAct ? input.MoveStick.ReadValue<Vector2>() : Vector2.zero;
            Vector2 turn = canAct ? input.TurnStick.ReadValue<Vector2>() : Vector2.zero;
            bool passportOpen = Game.Passport != null && Game.Passport.IsOpen;

            if (s.smoothLocomotion)
            {
                m_Arc.enabled = false; m_Reticle.gameObject.SetActive(false);
                if (move.sqrMagnitude > 0.04f)
                {
                    // Push the stick all the way forward to run: the Passenger chases at 2.8 m/s in VR, a walk is 2.0.
                    m_RunHold = move.y > 0.92f ? m_RunHold + Time.deltaTime : 0f;
                    float speed = m_RunHold > 0.35f ? 3.3f : 2.0f;
                    var dir = Forward * move.y + Vector3.Cross(Vector3.up, Forward) * move.x;
                    var local = transform.InverseTransformPoint(Head.position);
                    m_Body.center = new Vector3(local.x, 0.8f, local.z);
                    m_Body.Move(dir * speed * Time.deltaTime + Vector3.down * 2f * Time.deltaTime);
                }
                else m_RunHold = 0f;
                Sprinting = move.sqrMagnitude > 0.04f && m_RunHold > 0.35f; // breathing, heartbeat and the sleeves read this
            }
            else
            {
                Sprinting = false;
                if (move.y > 0.6f) m_Aiming = true;
                if (m_Aiming)
                {
                    var aimT = input.MoveStick == input.LStick ? m_LeftAim : m_RightAim;
                    ComputeArc(aimT.position, aimT.forward);
                    if (move.magnitude < 0.25f)
                    {
                        m_Aiming = false;
                        m_Arc.enabled = false;
                        m_Reticle.gameObject.SetActive(false);
                        if (m_ArcValid) StartCoroutine(TeleportRoutine(m_ArcTarget));
                    }
                }
            }

            // Turning (the destination selection uses the same stick's up/down while the passport is open).
            if (s.smoothTurn)
            {
                if (Mathf.Abs(turn.x) > 0.2f) transform.RotateAround(new Vector3(Head.position.x, 0, Head.position.z), Vector3.up, turn.x * 75f * Time.deltaTime);
            }
            else
            {
                if (m_TurnArmed && Mathf.Abs(turn.x) > 0.75f && !(passportOpen && Mathf.Abs(turn.y) > Mathf.Abs(turn.x)))
                {
                    m_TurnArmed = false;
                    transform.RotateAround(new Vector3(Head.position.x, 0, Head.position.z), Vector3.up, Mathf.Sign(turn.x) * s.snapAngle);
                    m_LastFeet = Feet;
                }
                else if (Mathf.Abs(turn.x) < 0.3f) m_TurnArmed = true;
            }
        }

        void ComputeArc(Vector3 origin, Vector3 dir)
        {
            m_ArcPoints.Clear();
            var v = dir.normalized * 7.5f;
            var p = origin;
            m_ArcValid = false;
            const float dt = 0.03f;
            for (int i = 0; i < 90; i++)
            {
                var next = p + v * dt;
                v += Vector3.down * 9.81f * dt;
                m_ArcPoints.Add(p);
                if (Physics.Linecast(p, next, out var hit, Layers.WorldMask, QueryTriggerInteraction.Ignore))
                {
                    m_ArcPoints.Add(hit.point);
                    bool floor = hit.collider.gameObject.layer == Layers.Floor && hit.normal.y > 0.7f;
                    var flat = hit.point - Feet; flat.y = 0;
                    bool clear = !Physics.CheckCapsule(hit.point + Vector3.up * 0.35f, hit.point + Vector3.up * 1.5f, 0.2f, 1 << Layers.Default, QueryTriggerInteraction.Ignore);
                    m_ArcValid = floor && clear && flat.magnitude < 7f;
                    m_ArcTarget = hit.point;
                    break;
                }
                p = next;
            }
            m_Arc.enabled = true;
            m_Arc.positionCount = m_ArcPoints.Count;
            m_Arc.SetPositions(m_ArcPoints.ToArray());
            var col = m_ArcValid ? new Color(0.55f, 0.95f, 0.8f, 0.85f) : new Color(0.9f, 0.4f, 0.35f, 0.6f);
            m_Arc.startColor = m_Arc.endColor = col;
            m_Reticle.gameObject.SetActive(m_ArcValid);
            if (m_ArcValid) { m_Reticle.position = m_ArcTarget + Vector3.up * 0.02f; m_Reticle.rotation = Quaternion.identity; }
        }

        System.Collections.IEnumerator TeleportRoutine(Vector3 target)
        {
            Fader.I?.Flash(0.08f, 0.1f);
            yield return new WaitForSecondsRealtime(0.08f);
            var f = Feet;
            transform.position += new Vector3(target.x - f.x, target.y - transform.position.y, target.z - f.z);
            m_LastFeet = Feet;
            AudioDirector.I?.Play("foot_stone_1", target, 0.25f);
        }

        void UpdateHeadBlock()
        {
            bool blocked = Physics.CheckSphere(Head.position, 0.1f, 1 << Layers.Default, QueryTriggerInteraction.Ignore);
            if (blocked != m_HeadBlocked)
            {
                m_HeadBlocked = blocked;
                Fader.I?.SetBlocker(blocked);
            }
        }
    }
}

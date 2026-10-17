using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwelfthStamp
{
    public enum PassengerState { Dormant, Observe, Stalk, Guard, Investigate, PressSeam, Reach, Repelled, Retreat, Drain, Rise, Mimic }

    /// <summary>
    /// The single enemy. A small, explicit state machine (no learning AI): dormant, observe from a distance, stalk,
    /// guard an objective, investigate a noise, press against an open seam, reach through it, recoil from light,
    /// retreat into shadow, drain when close. Never attacks while paused, during loading or onboarding (Threatening
    /// = false), and always telegraphs with sound before draining.
    /// </summary>
    public class PassengerController : MonoBehaviour
    {
        public static PassengerController I { get; private set; }

        public PassengerState State { get; private set; } = PassengerState.Dormant;
        public PassengerBody Body { get; private set; }
        public bool Threatening;                 // false during onboarding / transitions / cutscenes
        public float WalkSpeed = 0.85f;
        public float DrainRange = 1.35f;
        public float DrainPerSecond = 0.12f;
        public float RepelThreshold = 0.28f;
        public float RetreatThreshold = 0.6f;
        public Vector3 GuardPoint;
        public Vector3 RetreatPoint;
        public float RetreatSeconds = 20f;
        public bool CanBeRepelled = true;

        public event Action<PassengerState> StateChanged;
        public event Action Repelled;
        public event Action Drained;

        Vector3 m_Target;
        float m_StateTime;
        float m_Exposure, m_ExposureTime;
        float m_DrainTelegraph;
        float m_StepTimer, m_BurstTimer;
        bool m_Moving;
        PassengerState m_ReturnState = PassengerState.Stalk;
        AudioSource m_Drone;
        Cell m_PocketCell;
        readonly Dictionary<Renderer, Material[]> m_LitMats = new Dictionary<Renderer, Material[]>();

        public Vector3 ChestPosition => Body != null ? Body.Chest.position + Vector3.up * 0.2f : transform.position + Vector3.up * 1.5f;
        public bool Visible => gameObject.activeSelf && State != PassengerState.Dormant;

        public static PassengerController Create(Transform parent)
        {
            var go = new GameObject("Passenger");
            go.transform.SetParent(parent, false);
            go.layer = Layers.Passenger;
            I = go.AddComponent<PassengerController>();
            I.Body = PassengerBody.Create(go.transform);
            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 1.1f, 0); col.height = 2.2f; col.radius = 0.25f; col.isTrigger = true;
            MeshFactory.SetLayer(go, Layers.Passenger);
            go.SetActive(false);
            return I;
        }

        // ------------------------------------------------------------------ placement / control API
        public void Spawn(Cell cell, Vector3 localPos, float yaw, PassengerState state, Disguise? disguise = null)
        {
            ExitPocket();
            transform.SetParent(cell.transform, false);
            transform.localPosition = localPos;
            transform.localRotation = Quaternion.Euler(0, yaw, 0);
            gameObject.SetActive(true);
            Body.SetDisguise(disguise ?? DisguiseKit.ForCell(cell.Id));
            Body.RiseAmount = state == PassengerState.Rise ? 0f : 1f;
            Body.ShowImitationPassport(false);
            m_Exposure = 0f; m_ExposureTime = 0f;
            SetState(state);
            if (m_Drone == null) m_Drone = AudioDirector.I?.Loop("pass_drone_loop", Body.Chest, 0.0f, Bus.SFX, 1f, 1.5f, 14f);
        }

        public void Despawn()
        {
            ExitPocket();
            SetState(PassengerState.Dormant);
            gameObject.SetActive(false);
            if (m_Drone != null) { AudioDirector.I?.FadeOutAndStop(m_Drone, 0.6f); m_Drone = null; }
            Game.Story?.SetDrainFeedback(0f);
        }

        public void SetState(PassengerState s)
        {
            if (State == s && m_StateTime > 0f) return;
            State = s;
            m_StateTime = 0f;
            m_Moving = false;
            switch (s)
            {
                case PassengerState.Guard: m_Target = GuardPoint; break;
                case PassengerState.Retreat:
                    m_Target = RetreatPoint;
                    AudioDirector.I?.Play("pass_recoil", ChestPosition, 0.9f, UnityEngine.Random.Range(0.9f, 1.05f), Bus.SFX, true);
                    break;
                case PassengerState.Repelled:
                    AudioDirector.I?.Play("sun_sizzle", ChestPosition, 0.8f);
                    Repelled?.Invoke();
                    break;
                case PassengerState.Rise:
                    AudioDirector.I?.Play("pass_breath", transform.position + Vector3.up, 0.8f);
                    break;
            }
            StateChanged?.Invoke(s);
        }

        public void Investigate(Vector3 worldPoint, float seconds = 8f)
        {
            if (State == PassengerState.Dormant || State == PassengerState.Retreat) return;
            m_ReturnState = State == PassengerState.Investigate ? m_ReturnState : State;
            m_Target = worldPoint;
            SetState(PassengerState.Investigate);
            m_InvestigateSeconds = seconds;
        }
        float m_InvestigateSeconds = 8f;

        public void GoTo(Vector3 worldPoint, PassengerState state)
        {
            m_Target = worldPoint;
            SetState(state);
        }

        /// <summary>Show the Passenger standing inside a destination pocket (seen only through the tear).</summary>
        public void EnterPocket(Cell pocket, Vector3 localPos, float localYaw)
        {
            ExitPocket();
            m_PocketCell = pocket;
            transform.SetParent(pocket.transform, false);
            transform.localPosition = localPos;
            transform.localRotation = Quaternion.Euler(0, localYaw, 0);
            gameObject.SetActive(true);
            foreach (var r in Body.Renderers)
            {
                if (r == null) continue;
                m_LitMats[r] = r.sharedMaterials;
                var src = r.sharedMaterials;
                var dst = new Material[src.Length];
                bool ok = true;
                for (int i = 0; i < src.Length; i++) { dst[i] = Mats.PocketVariant(src[i], pocket.Lighting); if (dst[i] == null) ok = false; }
                if (ok) r.sharedMaterials = dst; else r.enabled = false;
            }
            MeshFactory.SetLayer(gameObject, Layers.Pocket);
        }

        public void ExitPocket()
        {
            if (m_PocketCell == null) return;
            foreach (var kv in m_LitMats) if (kv.Key != null) { kv.Key.sharedMaterials = kv.Value; kv.Key.enabled = true; }
            m_LitMats.Clear();
            m_PocketCell = null;
            MeshFactory.SetLayer(gameObject, Layers.Passenger);
        }

        public bool InPocket => m_PocketCell != null;

        // ------------------------------------------------------------------ update
        void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f || Body == null) return;
            m_StateTime += dt;
            var rig = Game.Rig;
            Vector3 player = rig != null ? rig.Feet : transform.position + Vector3.forward * 10f;
            float distToPlayer = Vector3.Distance(Flat(transform.position), Flat(player));

            // Light exposure (deterministic gameplay volumes).
            m_Exposure = InPocket ? 0f : LightVolumes.Exposure(ChestPosition);
            if (m_Exposure > RepelThreshold && CanBeRepelled) m_ExposureTime += dt; else m_ExposureTime = Mathf.Max(0f, m_ExposureTime - dt * 2f);

            if (m_Drone != null) AudioDirector.I.SetBaseVolume(m_Drone, State == PassengerState.Dormant ? 0f : Mathf.Lerp(0.5f, 0.08f, Mathf.InverseLerp(1.5f, 12f, distToPlayer)));

            switch (State)
            {
                case PassengerState.Dormant:
                    return;
                case PassengerState.Observe:
                    Body.Pose = Pose.Idle; Body.Speed01 = 0f;
                    FaceTowards(player, dt, 60f);
                    break;
                case PassengerState.Rise:
                    Body.Pose = Pose.Crouch;
                    Body.RiseAmount = Mathf.MoveTowards(Body.RiseAmount, 1f, dt / 2.4f);
                    FaceTowards(player, dt, 40f);
                    if (Body.RiseAmount >= 1f) SetState(PassengerState.Stalk);
                    break;
                case PassengerState.Stalk:
                    Body.Pose = Body.CurrentDisguise == Disguise.ExhibitLinen ? Pose.Shuffle : Pose.Walk;
                    MoveTowards(player, dt, distToPlayer > DrainRange * 0.8f);
                    break;
                case PassengerState.Guard:
                    if (Vector3.Distance(Flat(transform.position), Flat(GuardPoint)) > 0.3f) { Body.Pose = Pose.Walk; MoveTowards(GuardPoint, dt, true); }
                    else { Body.Pose = Pose.Idle; Body.Speed01 = 0f; FaceTowards(player, dt, 50f); }
                    break;
                case PassengerState.Investigate:
                    Body.Pose = Pose.Walk;
                    bool arrived = !MoveTowards(m_Target, dt, Vector3.Distance(Flat(transform.position), Flat(m_Target)) > 0.6f);
                    if (arrived)
                    {
                        Body.Pose = Pose.Crouch;
                        if (m_StateTime > m_InvestigateSeconds) SetState(m_ReturnState == PassengerState.Investigate ? PassengerState.Stalk : m_ReturnState);
                    }
                    break;
                case PassengerState.PressSeam:
                    Body.Pose = Pose.Reach;
                    Body.ReachAmount = Mathf.MoveTowards(Body.ReachAmount, 0.6f, dt);
                    MoveTowards(m_Target, dt, Vector3.Distance(Flat(transform.position), Flat(m_Target)) > 0.5f);
                    break;
                case PassengerState.Reach:
                    Body.Pose = Pose.Reach;
                    Body.ReachAmount = Mathf.MoveTowards(Body.ReachAmount, 1f, dt * 0.8f);
                    break;
                case PassengerState.Repelled:
                    Body.Pose = Pose.Shield; Body.Speed01 = 0f;
                    // Backs away from the light source a little, holding position if the light is weak.
                    if (m_Exposure >= RetreatThreshold && m_ExposureTime > 0.9f) SetState(PassengerState.Retreat);
                    else if (m_Exposure < RepelThreshold * 0.7f && m_StateTime > 1.2f) SetState(m_ReturnState);
                    break;
                case PassengerState.Retreat:
                    Body.Pose = Pose.Walk;
                    bool there = !MoveTowards(RetreatPoint, dt, Vector3.Distance(Flat(transform.position), Flat(RetreatPoint)) > 0.4f, 1.8f);
                    if (there)
                    {
                        Body.Pose = Pose.Shield;
                        FaceTowards(player, dt, 30f);
                        if (m_StateTime > RetreatSeconds) SetState(PassengerState.Stalk);
                    }
                    break;
                case PassengerState.Drain:
                    Body.Pose = Pose.Reach;
                    Body.ReachAmount = Mathf.MoveTowards(Body.ReachAmount, 1f, dt * 2f);
                    FaceTowards(player, dt, 120f);
                    if (distToPlayer > DrainRange * 1.4f) SetState(PassengerState.Stalk);
                    break;
                case PassengerState.Mimic:
                    Body.Pose = Pose.HoldPassport;
                    Body.ShowImitationPassport(true);
                    FaceTowards(player, dt, 30f);
                    break;
            }

            // Light reaction overrides (except while already retreating or hidden in a pocket).
            if (Threatening && CanBeRepelled && !InPocket && m_Exposure > RepelThreshold
                && State != PassengerState.Repelled && State != PassengerState.Retreat && State != PassengerState.Dormant && State != PassengerState.Rise)
            {
                if (State != PassengerState.Investigate) m_ReturnState = State == PassengerState.Drain ? PassengerState.Stalk : State;
                SetState(PassengerState.Repelled);
            }

            UpdateDrain(distToPlayer, dt);
        }

        void UpdateDrain(float distToPlayer, float dt)
        {
            bool close = Threatening && !InPocket && distToPlayer < DrainRange && State != PassengerState.Retreat && State != PassengerState.Repelled && State != PassengerState.Dormant && State != PassengerState.Rise;
            if (close)
            {
                if (m_DrainTelegraph <= 0f) AudioDirector.I?.Play("pass_breath", ChestPosition, 0.9f, 1f, Bus.SFX, true);
                m_DrainTelegraph += dt;
                if (m_DrainTelegraph > 0.9f)
                {
                    if (State != PassengerState.Drain && State != PassengerState.Mimic) SetState(PassengerState.Drain);
                    float scale = Game.Settings != null && Game.Settings.scareIntensity == 0 ? 0.7f : 1f;
                    Game.Story?.DrainVitality(DrainPerSecond * scale * dt);
                    Drained?.Invoke();
                }
                Game.Story?.SetDrainFeedback(Mathf.Clamp01(m_DrainTelegraph));
            }
            else
            {
                if (m_DrainTelegraph > 0f) Game.Story?.SetDrainFeedback(0f);
                m_DrainTelegraph = 0f;
            }
        }

        static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0, v.z);

        /// <summary>Glass-stutter movement: short bursts with tiny pauses. Returns true while still moving.</summary>
        bool MoveTowards(Vector3 target, float dt, bool shouldMove, float speedScale = 1f)
        {
            FaceTowards(target, dt, 140f);
            if (!shouldMove) { Body.Speed01 = Mathf.MoveTowards(Body.Speed01, 0f, dt * 3f); return false; }
            m_BurstTimer -= dt;
            if (m_BurstTimer <= 0f) { m_Moving = !m_Moving; m_BurstTimer = m_Moving ? UnityEngine.Random.Range(0.5f, 0.9f) : UnityEngine.Random.Range(0.08f, 0.2f); }
            float speed = (Body.CurrentDisguise == Disguise.ExhibitLinen ? 0.55f : WalkSpeed) * speedScale;
            if (m_Moving)
            {
                var dir = Flat(target) - Flat(transform.position);
                if (dir.magnitude > 0.01f)
                {
                    var step = dir.normalized * Mathf.Min(dir.magnitude, speed * dt);
                    var next = transform.position + step;
                    if (Physics.Raycast(next + Vector3.up * 1.2f, Vector3.down, out var hit, 3f, Layers.FloorMask | (1 << Layers.Default), QueryTriggerInteraction.Ignore))
                        next.y = hit.point.y;
                    transform.position = next;
                }
                Body.Speed01 = Mathf.MoveTowards(Body.Speed01, 1f, dt * 4f);
                m_StepTimer -= dt;
                if (m_StepTimer <= 0f)
                {
                    m_StepTimer = Body.CurrentDisguise == Disguise.ExhibitLinen ? 0.9f : 0.55f;
                    AudioDirector.I?.Play("pass_step_" + UnityEngine.Random.Range(1, 4), transform.position, 0.55f, UnityEngine.Random.Range(0.9f, 1.1f));
                    if (UnityEngine.Random.value < 0.25f) AudioDirector.I?.Play("pass_creak_" + UnityEngine.Random.Range(1, 4), ChestPosition, 0.45f);
                }
            }
            else Body.Speed01 = Mathf.MoveTowards(Body.Speed01, 0.2f, dt * 6f);
            return true;
        }

        void FaceTowards(Vector3 target, float dt, float degPerSec)
        {
            var dir = Flat(target) - Flat(transform.position);
            if (dir.sqrMagnitude < 0.0001f) return;
            var want = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, want, degPerSec * dt);
        }
    }
}

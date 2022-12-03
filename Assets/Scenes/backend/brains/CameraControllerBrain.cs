using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Brains {
    public class CameraControllerBrain : ICameraControllerBrain {
        readonly IGameSystem system;
        readonly Transform target;
        readonly Transform camera;
        CameraCfgSO cfg = null;
        CinemachineCameraOffset camOffset = null;

        PlayerAttackMode attackMode;
        bool isMoving;
        bool isScoping;

        public CameraControllerBrain(IGameSystem system, Transform camera) {
            this.system = system;
            this.camera = camera;
            target = system.Send<ITagRegistry, Transform>(registry =>
                registry.GetUnique(IdTag.PLAYER).transform);
            system.Send<IPubSub>(pubsub => {
                pubsub.Subscribe<AttackModeChanged>(UpdateState);
                pubsub.Subscribe<MovementChanged>(UpdateState);
                pubsub.Subscribe<ScopingChanged>(UpdateState);
            });
        }

        public void Init(CameraCfgSO cfg) {
            this.cfg = cfg;
            camOffset = camera.GetComponent<CinemachineCameraOffset>();
        }

        public void Update() {
            if (cfg != null && camOffset != null)
                SetOffset();
        }

        void SetOffset() {
            Vector3 rotFactor = LookAheadOffset();
            camOffset.m_Offset = Vector3.Lerp(
                camOffset.m_Offset,
                target.rotation * rotFactor,
                cfg.orbitSpeed * Time.deltaTime)
                + ShakeOffset();
        }

        void UpdateState(AttackModeChanged ev) =>
            attackMode = ev.mode;

        void UpdateState(MovementChanged ev) =>
            isMoving = ev.isMoving;

        void UpdateState(ScopingChanged ev) =>
            isScoping = ev.isScoping;

        Vector3 LookAheadOffset() {
            float lookAhead = 0f;

            if (isScoping) lookAhead += cfg.scopeOffset;
            else if (isMoving) lookAhead += cfg.moveOffset;
            if (IsAiming()) lookAhead += cfg.aimOffset;

            return new Vector3(0f, Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead), 0f);
        }

        Vector3 ShakeOffset() {
            float offset = attackMode == PlayerAttackMode.FIRING
                ? cfg.fireOffset : cfg.punchOffset;

            if (!IsAttacking()) return Vector3.zero;
            return new Vector3(
                Random.Range(-offset, offset),
                Random.Range(-offset, offset),
                Random.Range(-offset, offset));
        }

        bool IsAiming() =>
            attackMode == PlayerAttackMode.WEAPON
                || attackMode == PlayerAttackMode.FIRING;

        bool IsAttacking() =>
            attackMode == PlayerAttackMode.PUNCHING
                || attackMode == PlayerAttackMode.FIRING;
    }
}

using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraController : ICameraController {
        readonly IGameSystem system;
        readonly Transform target;
        readonly Transform camera;
        CameraCfgSO cfg = null;
        CinemachineCameraOffset camOffset = null;

        AttackMode attackMode;
        bool isMoving;
        bool isScoping;

        public CameraController(IGameSystem system, Transform camera) {
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

            if (IsAiming()) lookAhead += cfg.aimOffset;
            else if (isScoping) lookAhead = 0f;
            if (isMoving) lookAhead += cfg.moveOffset;

            return new Vector3(0f, Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead), 0f);
        }

        Vector3 ShakeOffset() {
            float offset = attackMode == AttackMode.FIRING
                ? cfg.fireOffset : cfg.punchOffset;

            if (!IsAttacking()) return Vector3.zero;
            return Vectors.Range(-offset, offset);
        }

        bool IsAiming() =>
            attackMode == AttackMode.WEAPON
                || attackMode == AttackMode.FIRING;

        bool IsAttacking() =>
            attackMode == AttackMode.MELEE
                || attackMode == AttackMode.FIRING;
    }
}

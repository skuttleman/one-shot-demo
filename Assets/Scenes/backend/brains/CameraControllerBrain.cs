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

        bool isMoving;
        bool isScoping;
        bool isAiming;

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
            float lookAhead = 0f;
            if (isScoping) lookAhead += cfg.scopeOffset;
            else if (isMoving) lookAhead += cfg.moveOffset;
            if (isAiming) lookAhead += cfg.aimOffset;

            lookAhead = Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead);

            camOffset.m_Offset = Vector3.Lerp(
                camOffset.m_Offset,
                target.rotation * new Vector3(0, lookAhead, 0),
                cfg.orbitSpeed * Time.deltaTime);
        }

        void UpdateState(AttackModeChanged ev) =>
            isAiming = ev.mode == PlayerAttackMode.WEAPON
                || ev.mode == PlayerAttackMode.FIRING;

        void UpdateState(MovementChanged ev) =>
            isMoving = ev.isMoving;

        void UpdateState(ScopingChanged ev) =>
            isScoping = ev.isScoping;
    }
}

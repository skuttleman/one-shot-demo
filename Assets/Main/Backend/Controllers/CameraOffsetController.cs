using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraOffsetController : ASystemInitializer<AttackModeChanged, MovementChanged, ScopingChanged> {
        [SerializeField] private CameraCfgSO cfg;
        [SerializeField] private Vector3 foo;

        private CinemachineCameraOffset camOffset = null;
        private Transform player;
        private AttackMode attackMode;
        private bool isMoving;
        private bool isScoping;

        protected override void OnEvent(AttackModeChanged ev) {
            attackMode = ev.mode;
        }

        protected override void OnEvent(MovementChanged ev) {
            isMoving = Maths.NonZero(ev.speed);
        }

        protected override void OnEvent(ScopingChanged ev) {
            isScoping = ev.isScoping;
        }

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player().transform;
        }

        private void Start() {
            camOffset = GetComponent<CinemachineCameraOffset>();
        }

        private void Update() {
            if (cfg != null && camOffset != null)
                SetOffset();
        }

        private void SetOffset() {
            float rotFactor = LookAheadOffset();
            Vector3 angle = rotFactor * Vectors.ToVector3(player.rotation.eulerAngles.y).normalized;

            camOffset.m_Offset = Vector3.Lerp(
                camOffset.m_Offset,
                new(angle.x, angle.z, 0f),
                cfg.orbitSpeed * Time.deltaTime);
        }

        private float LookAheadOffset() {
            float lookAhead = 0.5f;

            if (IsAiming()) lookAhead += cfg.aimOffset;
            else if (isScoping) lookAhead = 0f;
            if (isMoving) lookAhead += cfg.moveOffset;

            return Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead);
        }

        private bool IsAiming() =>
            attackMode == AttackMode.WEAPON
                || attackMode == AttackMode.FIRING;
    }
}

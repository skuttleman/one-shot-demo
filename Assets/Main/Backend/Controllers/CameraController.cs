using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class CameraController : ASystemInitializer<AttackModeChanged, MovementChanged, ScopingChanged> {
        [SerializeField] private CameraCfgSO cfg;

        private CinemachineCameraOffset camOffset = null;
        private AttackMode attackMode;
        private bool isMoving;
        private bool isScoping;

        protected override void OnEvent(AttackModeChanged ev) =>
            attackMode = ev.mode;

        protected override void OnEvent(MovementChanged ev) =>
            isMoving = Maths.NonZero(ev.speed);

        protected override void OnEvent(ScopingChanged ev) =>
            isScoping = ev.isScoping;

        private void Start() {
            camOffset = GetComponent<CinemachineCameraOffset>();
        }

        private void Update() {
            if (cfg != null && camOffset != null)
                SetOffset();
        }

        private void SetOffset() {
            Vector3 rotFactor = LookAheadOffset();

            camOffset.m_Offset = Vector3.Lerp(
                camOffset.m_Offset,
                transform.rotation * rotFactor,
                cfg.orbitSpeed * Time.deltaTime)
                + ShakeOffset();
        }

        private Vector3 LookAheadOffset() {
            float lookAhead = 0f;

            if (IsAiming()) lookAhead += cfg.aimOffset;
            else if (isScoping) lookAhead = 0f;
            if (isMoving) lookAhead += cfg.moveOffset;

            return new(0f, Mathf.Clamp(lookAhead, 0f, cfg.maxLookAhead), 0f);
        }

        private Vector3 ShakeOffset() {
            float offset = attackMode == AttackMode.FIRING
                ? cfg.fireOffset : cfg.punchOffset;

            if (IsAttacking())
                return transform.rotation * new Vector3(0, -offset, 0f) * Time.deltaTime;
            return Vector3.zero;
        }

        private bool IsAiming() =>
            attackMode == AttackMode.WEAPON
                || attackMode == AttackMode.FIRING;

        private bool IsAttacking() =>
            attackMode == AttackMode.MELEE
                || attackMode == AttackMode.FIRING;
    }
}

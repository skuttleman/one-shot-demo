using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;
using OSCore;

namespace OSBE.Controllers {
    public class CameraController : MonoBehaviour {
        [SerializeField] private CameraCfgSO cfg;

        private IGameSystem system;
        private CinemachineCameraOffset camOffset = null;

        private AttackMode attackMode;
        private bool isMoving;
        private bool isScoping;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            camOffset = GetComponent<CinemachineCameraOffset>();
            system.Send<IPubSub>(pubsub => {
                pubsub.Subscribe<AttackModeChanged>(UpdateState);
                pubsub.Subscribe<MovementChanged>(UpdateState);
                pubsub.Subscribe<ScopingChanged>(UpdateState);
            });
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

        private void UpdateState(AttackModeChanged ev) =>
            attackMode = ev.mode;

        private void UpdateState(MovementChanged ev) =>
            isMoving = ev.isMoving;

        private void UpdateState(ScopingChanged ev) {
            isScoping = ev.isScoping;
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

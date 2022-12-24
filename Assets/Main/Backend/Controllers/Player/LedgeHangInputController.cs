using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.Utils;
using System;
using UnityEngine;
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSBE.Controllers.Player {
    public class LedgeHangInputController : IPlayerInputController {
        private readonly IPlayerMainController controller;
        private readonly PlayerCfgSO cfg;
        private readonly Transform transform;
        private readonly PlayerAnimator anim;
        private readonly Rigidbody rb;

        private CapsuleCollider playerCollider;

        private PlayerLedgeHangingInputState state = new() {
            hangingPoint = Vector3.zero,
            ledge = default,
        };

        public LedgeHangInputController(IPlayerMainController controller, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.cfg = cfg;
            this.transform = transform;

            anim = transform.GetComponentInChildren<PlayerAnimator>();
            rb = transform.GetComponent<Rigidbody>();
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case LedgeTransition ev:
                    UpdateState(state => state with {
                        ledge = ev.ledge,
                        hangingPoint = ev.pt,
                    });
                    break;
                case ClimbInput ev:
                    anim.transform.localPosition = anim.transform.localPosition
                        .WithZ(anim.transform.localPosition.z + 0.6f);
                    if (ev.direction == ClimbDirection.UP) OnClimbUp();
                    else OnClimbDown();
                    break;
                case MovementInput ev: OnMovementInput(ev.direction); break;
            }
        }

        public void OnUpdate(PlayerSharedInputState common) { }

        public void OnActivate(PlayerSharedInputState common) {
            controller.Notify(new Facing(Vector2.zero));

            float angleToPoint = Mathf.Round(Vectors.AngleTo(transform.position - state.hangingPoint));
            transform.rotation = Quaternion.Euler(0f, 0f, angleToPoint);
            playerCollider = transform.GetComponentInChildren<CapsuleCollider>();

            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            transform.position = transform.position.WithZ(state.hangingPoint.z + 0.6f);
        }

        public void OnStateEnter(PlayerAnim anim) {
            if (anim == PlayerAnim.hang_move)
                MovePlayer();
            state = ControllerUtils.TransitionState(state, anim);
        }

        private void OnClimbUp() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_CLIMB;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                Vector3 diff = (state.hangingPoint - transform.position) * 1.2f;
                transform.position += diff;
                controller.Notify(new Facing(Vector2.zero));
            }
        }

        private void OnClimbDown() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_DROP;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                float pointZ = state.hangingPoint.z;
                transform.position = transform.position.WithZ(pointZ + 0.55f);
                controller.Notify(new Facing(Vector2.zero));
            }
        }

        private PlayerLedgeHangingInputState UpdateState(Func<PlayerLedgeHangingInputState, PlayerLedgeHangingInputState> updateFn) =>
            state = updateFn(state);

        private void OnMovementInput(Vector2 direction) {
            Vector2 dir = direction.Directionify().normalized;
            Vector2 move = dir * cfg.hangMoveAmount;
            Vector3 nextHangingPoint = state.hangingPoint + move.Upgrade();
            Vector3 nextPlayerPos = transform.position + move.Upgrade();

            float heightAxis = (playerCollider.height + 0.1f) / 2f;
            Vector3 height = new(
                playerCollider.direction == 0 ? heightAxis : 0,
                playerCollider.direction == 1 ? heightAxis : 0,
                playerCollider.direction == 2 ? heightAxis : 0);

            if (dir.magnitude > 0.5f
                && anim.CanTransition(PlayerAnimSignal.MOVE_ON)
                && Mathf.Abs(transform.rotation.eulerAngles.z - Vectors.AngleTo(dir)) % 180f == 90f
                && state.ledge.ClosestPoint(nextHangingPoint) == nextHangingPoint
                && state.ledge.ClosestPoint(nextPlayerPos) != nextPlayerPos
                && !Physics.CapsuleCast(
                    playerCollider.center + transform.position + height,
                    playerCollider.center + transform.position - height,
                    playerCollider.radius,
                    move,
                    cfg.hangMoveAmount)) {

                anim.Send(PlayerAnimSignal.MOVE_ON);
                UpdateState(state => state with {
                    movement = move,
                });
            }
        }

        private void MovePlayer() {
            transform.position += state.movement.Upgrade();
            UpdateState(state => state with {
                hangingPoint = state.hangingPoint + state.movement.Upgrade(),
            });
        }
    }
}

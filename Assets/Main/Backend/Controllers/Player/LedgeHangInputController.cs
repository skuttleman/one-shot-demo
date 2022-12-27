﻿using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.Utils;
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
        private PlayerControllerState state;

        public LedgeHangInputController(IPlayerMainController controller, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.cfg = cfg;
            this.transform = transform;

            anim = transform.GetComponentInChildren<PlayerAnimator>();
            rb = transform.GetComponent<Rigidbody>();
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case ClimbInput ev:
                    anim.transform.localPosition = anim.transform.localPosition
                        .WithZ(anim.transform.localPosition.z + 0.6f);
                    if (ev.direction == ClimbDirection.UP) OnClimbUp();
                    else OnClimbDown();
                    break;
                case MovementInput ev: OnMovementInput(ev.direction); break;
            }
        }

        public void OnUpdate(PlayerControllerState state) {
            this.state = state;
        }

        public void OnActivate(PlayerControllerState state) {
            controller.UpdateState(state => state with { facing = Vector2.zero });

            float angleToPoint = Mathf.Round(Vectors.AngleTo(transform.position - state.hangingPoint));
            transform.rotation = Quaternion.Euler(0f, 0f, angleToPoint);
            playerCollider = transform.GetComponentInChildren<CapsuleCollider>();

            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            transform.position = transform.position.WithZ(state.hangingPoint.z + 0.6f);
            this.state = state;
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            if (curr == PlayerAnim.hang_move)
                MovePlayer();
        }

        private void OnClimbUp() {
            anim.UpdateState(state => state with { climb = true });

            Vector3 diff = (state.hangingPoint - transform.position) * 1.2f;
            transform.position += diff;
        }

        private void OnClimbDown() {
            anim.UpdateState(state => state with { fall = true, hang = false });

            float pointZ = state.hangingPoint.z;
            transform.position = transform.position.WithZ(pointZ + 0.55f);
        }

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
                && Mathf.Abs(transform.rotation.eulerAngles.z - Vectors.AngleTo(dir)) % 180f == 90f
                && state.ledge.ClosestPoint(nextHangingPoint) == nextHangingPoint
                && state.ledge.ClosestPoint(nextPlayerPos) != nextPlayerPos
                && !Physics.CapsuleCast(
                    playerCollider.center + transform.position + height,
                    playerCollider.center + transform.position - height,
                    playerCollider.radius,
                    move,
                    cfg.hangMoveAmount)) {

                controller.UpdateState(state => state with {
                    movement = move,
                });

                if (anim.state == PlayerAnim.hang_idle)
                    anim.UpdateState(state => state with { move = true });
            }
        }

        private void MovePlayer() {
            transform.position += state.movement.Upgrade();
            controller.UpdateState(state => state with {
                hangingPoint = state.hangingPoint + state.movement.Upgrade(),
            });
        }
    }
}

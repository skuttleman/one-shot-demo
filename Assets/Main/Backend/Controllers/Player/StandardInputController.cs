using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers.Player {
    public class StandardInputController : IPlayerInputController {
        private readonly IPlayerMainController controller;
        private readonly PlayerCfgSO cfg;
        private readonly Transform transform;
        private readonly Rigidbody rb;
        private readonly PlayerAnimator anim;

        public StandardInputController(IPlayerMainController controller, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.cfg = cfg;
            this.transform = transform;

            rb = transform.GetComponent<Rigidbody>();
            anim = transform.GetComponentInChildren<PlayerAnimator>();
        }

        public void On(PlayerControllerInput e) { }

        public void OnUpdate(PlayerState state) {
            if (state.std.mouseLookTimer > 0f)
                controller.UpdateState(stte => state with {
                    std = state.std with {
                        mouseLookTimer = state.std.mouseLookTimer - Time.deltaTime
                    }
                });

            RotatePlayer(state, PlayerControllerUtils.MoveCfg(cfg, state));
        }

        public void OnFixedUpdate(PlayerState state) {
            bool prevGrounded = state.std.isGrounded;
            bool isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out RaycastHit ground,
                cfg.groundedDist);
            controller.UpdateState(state => state with {
                std = state.std with {
                    isGrounded = isGrounded,
                    ground = ground,
                },
            });

            if (prevGrounded && !isGrounded) {
                anim.Send(PlayerAnimSignal.FALLING);
            } else if (!prevGrounded && isGrounded) {
                if (state.std.isSprinting && Vectors.NonZero(state.std.movement))
                    anim.Send(PlayerAnimSignal.LAND_SPRINT);
                else if (Vectors.NonZero(state.std.movement))
                    anim.Send(PlayerAnimSignal.LAND_MOVE);
                else
                    anim.Send(PlayerAnimSignal.LAND_IDLE);
            }

            MovePlayer(state, PlayerControllerUtils.MoveCfg(cfg, state));
        }

        private void RotatePlayer(PlayerState state, MoveConfig moveCfg) {
            Vector2 direction;

            if (Vectors.NonZero(state.std.facing)
                && (state.std.stance != PlayerStance.CRAWLING || !Vectors.NonZero(state.std.movement)))
                direction = state.std.facing;
            else if (state.std.mouseLookTimer <= 0f && Vectors.NonZero(state.std.movement))
                direction = state.std.movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    moveCfg.rotationSpeed * Time.deltaTime);
        }

        private void MovePlayer(PlayerState state, MoveConfig moveCfg) {
            if (state.std.isGrounded
                && state.std.ground.collider != null
                && state.std.isMoving
                && PlayerControllerUtils.IsMovable(state.std.stance, state)) {
                float speed = moveCfg.moveSpeed;
                float forceZ = state.std.ground.transform.rotation != Quaternion.identity && Vectors.NonZero(state.std.movement)
                    ? (Vector3.Angle(state.std.ground.normal, state.std.movement) - 90f) / 90f
                    : 0f;

                if (PlayerControllerUtils.IsAiming(state.std.attackMode)) speed *= cfg.aimFactor;
                else if (state.std.isScoping) speed *= cfg.scopeFactor;

                float movementSpeed = Mathf.Max(
                    Mathf.Abs(state.std.movement.x),
                    Mathf.Abs(state.std.movement.y));
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;

                if (Vectors.NonZero(state.std.facing)) {
                    float mov = (360f + Vectors.AngleTo(transform.position, transform.position - state.std.movement.Upgrade())) % 360;
                    float fac = (360f + Vectors.AngleTo(transform.position, transform.position - state.std.facing.Upgrade())) % 360;
                    float diff = Mathf.Abs(mov - fac);

                    if (diff > 180f) diff = Mathf.Abs(diff - 360f);

                    speed *= Mathf.Lerp(moveCfg.lookSpeedInhibiter, 1f, 1f - diff / 180f);
                }

                Vector3 dir = speed * state.std.movement.Upgrade(-forceZ);

                float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;
                if (velocityDiff < moveCfg.maxVelocitydamper)
                    dir *= velocityDiff / moveCfg.maxVelocitydamper;

                float currSpeed = state.common.animSpeed;
                controller.UpdateState(state => state with {
                    common = state.common with {
                        animSpeed = state.std.isMoving ? movementSpeed * speed * moveCfg.animFactor : 0
                    }
                });
                PublishChanged(currSpeed, state.common.animSpeed, new MovementChanged(state.common.animSpeed));
                if (isForceable && Vectors.NonZero(state.std.movement)) {
                    anim.SetSpeed(state.common.animSpeed * Time.fixedDeltaTime);

                    if (state.std.stance == PlayerStance.STANDING)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue)) controller.Publish(e);
        }
    }
}

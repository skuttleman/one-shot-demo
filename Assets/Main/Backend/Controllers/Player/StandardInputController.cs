using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
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

        public void OnUpdate(PlayerState state) {
            if (state.input.mouseLookTimer > 0f)
                controller.UpdateState(stte => state with {
                    input = state.input with {
                        mouseLookTimer = state.input.mouseLookTimer - Time.deltaTime
                    }
                });

            RotatePlayer(state, PlayerControllerUtils.MoveCfg(cfg, state));
        }

        public void OnFixedUpdate(PlayerState state) {
            bool prevGrounded = state.isGrounded;
            bool isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out RaycastHit ground,
                cfg.groundedDist);
            controller.UpdateState(state => state with {
                isGrounded = isGrounded,
                ground = ground,
            });

            if (prevGrounded && !isGrounded) {
                anim.Send(PlayerAnimSignal.FALLING);
            } else if (!prevGrounded && isGrounded) {
                if (state.isSprinting && state.isMoving) anim.Send(PlayerAnimSignal.LAND_SPRINT);
                else if (state.isMoving) anim.Send(PlayerAnimSignal.LAND_MOVE);
                else anim.Send(PlayerAnimSignal.LAND_IDLE);
            }

            MovePlayer(state, PlayerControllerUtils.MoveCfg(cfg, state));
        }

        private void RotatePlayer(PlayerState state, MoveConfig moveCfg) {
            Vector2 direction;

            if (Vectors.NonZero(state.input.facing)
                && (state.stance != PlayerStance.CRAWLING || !Vectors.NonZero(state.input.movement)))
                direction = state.input.facing;
            else if (state.input.mouseLookTimer <= 0f && Vectors.NonZero(state.input.movement))
                direction = state.input.movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    moveCfg.rotationSpeed * Time.deltaTime);
        }

        private void MovePlayer(PlayerState state, MoveConfig moveCfg) {
            if (state.isGrounded
                && state.ground.collider != null
                && state.isMoving
                && PlayerControllerUtils.IsMovable(state.stance, state)) {
                float speed = moveCfg.moveSpeed;
                float forceZ = state.ground.transform.rotation != Quaternion.identity && Vectors.NonZero(state.input.movement)
                    ? (Vector3.Angle(state.ground.normal, state.input.movement) - 90f) / 90f
                    : 0f;

                if (PlayerControllerUtils.IsAiming(state.attackMode)) speed *= cfg.aimFactor;
                else if (state.isScoping) speed *= cfg.scopeFactor;

                float movementSpeed = Mathf.Max(
                    Mathf.Abs(state.input.movement.x),
                    Mathf.Abs(state.input.movement.y));
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;

                if (Vectors.NonZero(state.input.facing)) {
                    float mov = (360f + Vectors.AngleTo(transform.position, transform.position - state.input.movement.Upgrade())) % 360;
                    float fac = (360f + Vectors.AngleTo(transform.position, transform.position - state.input.facing.Upgrade())) % 360;
                    float diff = Mathf.Abs(mov - fac);

                    if (diff > 180f) diff = Mathf.Abs(diff - 360f);

                    speed *= Mathf.Lerp(moveCfg.lookSpeedInhibiter, 1f, 1f - diff / 180f);
                }

                Vector3 dir = speed * state.input.movement.Upgrade(-forceZ);

                float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;
                if (velocityDiff < moveCfg.maxVelocitydamper)
                    dir *= velocityDiff / moveCfg.maxVelocitydamper;

                float currSpeed = state.animSpeed;
                controller.UpdateState(state => state with {
                    animSpeed = state.isMoving ? movementSpeed * speed * moveCfg.animFactor : 0
                });
                PublishChanged(currSpeed, state.animSpeed, new MovementChanged(state.animSpeed));
                if (isForceable && Vectors.NonZero(state.input.movement)) {
                    anim.SetSpeed(state.animSpeed * Time.fixedDeltaTime);

                    if (state.stance == PlayerStance.STANDING)
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

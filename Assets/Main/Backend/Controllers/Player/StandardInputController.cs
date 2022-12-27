using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;
using static OSCore.Data.Controllers.PlayerControllerInput;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers.Player {
    public class StandardInputController : IPlayerInputController {
        private readonly IPlayerMainController controller;
        private readonly IGameSystem system;
        private readonly PlayerCfgSO cfg;
        private readonly Transform transform;
        private readonly Rigidbody rb;
        private readonly PlayerAnimator anim;

        private PlayerControllerState state;

        public StandardInputController(
            IPlayerMainController controller,
            IGameSystem system,
            PlayerCfgSO cfg,
            Transform transform,
            PlayerControllerState state) {
            this.controller = controller;
            this.system = system;
            this.cfg = cfg;
            this.transform = transform;
            this.state = state;

            rb = transform.GetComponent<Rigidbody>();
            anim = transform.GetComponentInChildren<PlayerAnimator>();
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case MovementInput ev: OnMovementInput(ev.direction); break;
                case SprintInput ev: OnSprintInput(ev.isSprinting); break;
                case LookInput ev: OnLookInput(ev.direction, ev.isMouse); break;
                case StanceInput: OnStanceInput(); break;
                case ScopeInput ev: OnScopeInput(ev.isScoping); break;
                case AimInput ev: OnAimInput(ev.isAiming); break;
                case AttackInput ev: OnAttackInput(ev.isAttacking); break;
            }
        }

        public void OnUpdate(PlayerControllerState state) {
            if (this.state.mouseLookTimer > 0f)
                controller.UpdateState(mainState => mainState with {
                    mouseLookTimer = state.mouseLookTimer - Time.deltaTime
                });

            this.state = state;
            RotatePlayer(ControllerUtils.MoveCfg(cfg, this.state));
        }

        public void OnFixedUpdate(PlayerControllerState state) {
            bool prevGrounded = this.state.isGrounded;
            Collider ledge = this.state.ground.collider;
            bool wasCrouching = this.state.stance == PlayerStance.CROUCHING;



            bool isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out RaycastHit ground,
                cfg.groundedDist,
                ~0,
                QueryTriggerInteraction.Ignore);
            controller.UpdateState(state => state with {
                isGrounded = isGrounded,
                ground = ground,
            });

            if (prevGrounded && !isGrounded)
                anim.UpdateState(state => state with { fall = true });
            else if (!prevGrounded && isGrounded)
                anim.UpdateState(state => state with { fall = false });


            this.state = state;
            MovePlayer(ControllerUtils.MoveCfg(cfg, this.state));



            bool isFallStart = prevGrounded && !isGrounded;
            bool isCatchable = ledge != null
                && system.Send<ITagRegistry, ISet<GameObject>>(tags =>
                    tags.Get(IdTag.PLATFORM_CATCHABLE))
                    .Contains(ledge.gameObject);

            if (isFallStart && isCatchable && wasCrouching)
                TransitionToLedgeHang(ledge);
        }

        private void RotatePlayer(MoveConfig moveCfg) {
            if (!state.isGrounded) return;
            Vector2 direction;

            if (Vectors.NonZero(state.facing)
                && (state.stance != PlayerStance.CRAWLING || !Vectors.NonZero(state.movement)))
                direction = state.facing;
            else if (state.mouseLookTimer <= 0f && Vectors.NonZero(state.movement))
                direction = state.movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, rotationZ),
                moveCfg.rotationSpeed * Time.deltaTime);
        }

        private void MovePlayer(MoveConfig moveCfg) {
            if (state.isGrounded
                && state.ground.collider != null
                && state.isMoving
                && ControllerUtils.IsMovable(state.stance, state)) {
                float speed = moveCfg.moveSpeed;
                float forceZ = state.ground.transform.rotation != Quaternion.identity && Vectors.NonZero(state.movement)
                    ? (Vector3.Angle(state.ground.normal, state.movement) - 90f) / 90f
                    : 0f;

                if (ControllerUtils.IsAiming(state.attackMode)) speed *= cfg.aimFactor;
                else if (state.isScoping) speed *= cfg.scopeFactor;

                float movementSpeed = Mathf.Max(
                    Mathf.Abs(state.movement.x),
                    Mathf.Abs(state.movement.y));
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;

                if (Vectors.NonZero(state.facing)) {
                    float mov = (360f + Vectors.AngleTo(transform.position, transform.position - state.movement.Upgrade())) % 360;
                    float fac = (360f + Vectors.AngleTo(transform.position, transform.position - state.facing.Upgrade())) % 360;
                    float diff = Mathf.Abs(mov - fac);

                    if (diff > 180f) diff = Mathf.Abs(diff - 360f);

                    speed *= Mathf.Lerp(moveCfg.lookSpeedInhibiter, 1f, 1f - diff / 180f);
                }

                Vector3 dir = speed * state.movement.Upgrade(-forceZ);

                float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;
                if (velocityDiff < moveCfg.maxVelocitydamper)
                    dir *= velocityDiff / moveCfg.maxVelocitydamper;

                float currSpeed = anim.animSpeed;
                float animSpeed = state.isMoving ? movementSpeed * speed * moveCfg.animFactor : 1f;

                PublishChanged(currSpeed, animSpeed, new MovementChanged(animSpeed));
                if (isForceable && Vectors.NonZero(state.movement)) {
                    anim.SetSpeed(animSpeed * Time.fixedDeltaTime);

                    if (state.stance == PlayerStance.STANDING)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue))
                system.Send<IPubSub>(pubsub => pubsub.Publish(e));
        }

        private void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            anim.UpdateState(state => state with {
                move = isMoving,
                sprint = state.sprint && isMoving,
            });
            controller.UpdateState(state => state with {
                movement = direction,
            });
        }

        private void OnSprintInput(bool isSprinting) {
            if (isSprinting && ControllerUtils.CanSprint(state))
                anim.UpdateState(state => state with { sprint = isSprinting });
        }

        private void OnLookInput(Vector2 direction, bool isMouse) {
            controller.UpdateState(state => state with {
                facing = direction,
                mouseLookTimer = isMouse && Vectors.NonZero(direction)
                    ? cfg.mouseLookReset
                    : state.mouseLookTimer
            });
        }

        private void OnStanceInput() {
            PlayerStance nextStance = ControllerUtils.NextStance(state.stance);
            if (!state.isMoving || ControllerUtils.IsMovable(nextStance, state))
                anim.UpdateState(state => state with { stance = nextStance });
        }

        private void OnScopeInput(bool isScoping) {
            anim.UpdateState(state => state with { scope = isScoping });
        }

        private void OnAimInput(bool isAiming) {
            anim.UpdateState(state => state with { aim = isAiming });
        }

        private void OnAttackInput(bool isAttacking) {
            if (isAttacking && ControllerUtils.CanAttack(state.attackMode))
                anim.UpdateState(state => state with { attack = isAttacking });
        }

        private void TransitionToLedgeHang(Collider ledge) {
            float distanceToGround = float.PositiveInfinity;

            Vector3 pt = ledge.ClosestPoint(transform.position);
            Vector2 direction = (transform.position - pt).Downgrade().Directionify().normalized;
            Vector3 nextPlayerPos = pt + (direction * 0.275f).Upgrade();

            if (Physics.Raycast(
                nextPlayerPos,
                Vectors.DOWN,
                out RaycastHit ground,
                1000f))
                distanceToGround = ground.distance;

            if (distanceToGround >= 0.6f) {
                anim.transform.localPosition = anim.transform.localPosition
                    .WithZ(anim.transform.localPosition.z - 0.6f);
                anim.UpdateState(state => state with { hang = true });

                Vector2 facing = pt - nextPlayerPos;
                rb.velocity = Vector3.zero;
                transform.position = nextPlayerPos;

                controller.UpdateState(state => state with {
                    movement = Vector3.zero,
                    isMoving = false,
                    controls = PlayerInputControlMap.None,
                    facing = facing,
                    ledge = ledge,
                    hangingPoint = pt,
                });
            }
        }
    }
}

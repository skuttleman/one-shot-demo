using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
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

        public StandardInputController(
            IPlayerMainController controller,
            IGameSystem system,
            PlayerCfgSO cfg,
            Transform transform) {
            this.controller = controller;
            this.system = system;
            this.cfg = cfg;
            this.transform = transform;

            rb = transform.GetComponent<Rigidbody>();
            anim = transform.GetComponentInChildren<PlayerAnimator>();
        }

        public void Handle(PlayerControllerInput e) {
            switch (e) {
                case MovementInput ev: OnMovementInput(ev.direction); break;
                case SprintInput ev: OnSprintInput(ev.isSprinting); break;
                case LookInput ev: OnLookInput(ev.direction, ev.isMouse); break;
                case StanceInput: OnStanceInput(); break;
                case DiveInput: OnDiveInput(); break;
                case ScopeInput ev: OnScopeInput(ev.isScoping); break;
                case AimInput ev: OnAimInput(ev.isAiming); break;
                case AttackInput ev: OnAttackInput(ev.isAttacking); break;
                case TBDInput: OnTBDInput(); break;
            }
        }

        public void OnUpdate() {
            controller.UpdateState(mainState => {
                if (mainState.mouseLookTimer > 0f) {
                    mainState = mainState with { mouseLookTimer = mainState.mouseLookTimer - Time.deltaTime };
                }
                if (mainState.tbdTimer > 0f) {
                    mainState = mainState with { tbdTimer = mainState.tbdTimer - Time.deltaTime };
                }
                return mainState;
            });

            RotatePlayer(ControllerUtils.MoveCfg(cfg, controller.state));
        }

        public void OnFixedUpdate() {
            Collider ledge = controller.state.ground.collider;
            bool wasCrouching = controller.state.stance == PlayerStance.CROUCHING;
            bool prevGrounded = controller.state.isGrounded;
            bool isGrounded = GroundPlayer();

            if (prevGrounded && !isGrounded) {
                anim.Transition(state => state with { fall = true });
            } else if (!prevGrounded && isGrounded) {
                anim.Transition(state => state with { fall = false });
            }

            MovePlayer(ControllerUtils.MoveCfg(cfg, controller.state));

            bool isFallStart = prevGrounded && !isGrounded;
            bool isCatchable = ledge != null
                && system.Send<ITagRegistry, ISet<GameObject>>(tags =>
                    tags.Get(IdTag.PLATFORM_CATCHABLE))
                    .Contains(ledge.gameObject);

            if (isFallStart && isCatchable && wasCrouching) {
                TransitionToLedgeHang(ledge);
            }
        }

        public bool GroundPlayer() {
            bool isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, -0.01f, 0f),
                Vector3.down,
                out RaycastHit ground,
                cfg.groundedDist,
                LayerMask.GetMask("Geometry"),
                QueryTriggerInteraction.Ignore);
            controller.UpdateState(state => state with {
                isGrounded = isGrounded,
                ground = ground,
            });

            return isGrounded;
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            switch ((prev, curr)) {
                case (_, PlayerAnim.crawl_dive):
                    rb.AddRelativeForce((Vector3.forward + Vector3.up) * cfg.diveForce);
                    break;
            }
        }

        private void RotatePlayer(MoveConfig moveCfg) {
            if (!controller.state.isGrounded) return;
            Vector2 direction;
            bool isMoving = Vectors.NonZero(controller.state.movement);

            if (Vectors.NonZero(controller.state.facing)
                && (controller.state.stance != PlayerStance.CRAWLING || !isMoving)) {
                direction = controller.state.facing;
            } else if (controller.state.mouseLookTimer <= 0f && isMoving) {
                direction = controller.state.movement;
            } else {
                return;
            }

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(new(direction.x, 0f, direction.y)),
                moveCfg.rotationSpeed * Time.deltaTime);
        }

        private void MovePlayer(MoveConfig moveCfg) {
            if (CanMove()) {
                float speed = MoveSpeed(moveCfg);
                float movementSpeed = Mathf.Max(
                    Mathf.Abs(controller.state.movement.x),
                    Mathf.Abs(controller.state.movement.y));
                float currSpeed = anim.animSpeed;
                float animSpeed = controller.state.isMoving ? movementSpeed * speed * moveCfg.animFactor : 1f;

                PublishChanged(currSpeed, animSpeed, MoveChangedEvent(movementSpeed));

                Vector3 dir = MoveDirection(moveCfg, speed, 0f);
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;
                if (isForceable && Vectors.NonZero(controller.state.movement)) {
                    anim.SetSpeed(animSpeed * Time.fixedDeltaTime);

                    if (controller.state.stance == PlayerStance.STANDING)
                        rb.AddRelativeForce(Vector3.forward * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private MovementChanged MoveChangedEvent(float movementSpeed) {
            PlayerSpeed playerSpeed = PlayerSpeed.FAST;
            if (!controller.state.isMoving || movementSpeed < 0.1f) playerSpeed = PlayerSpeed.STOPPED;
            else if (movementSpeed < 0.725f) playerSpeed = PlayerSpeed.SLOW;
            return new(playerSpeed);
        }

        private bool CanMove() =>
            controller.state.isGrounded
                && controller.state.ground.collider != null
                && controller.state.isMoving
                && ControllerUtils.IsMovable(controller.state.stance, controller.state);

        private float MoveSpeed(MoveConfig moveCfg) {
            float speed = moveCfg.moveSpeed;
            if (ControllerUtils.IsAiming(controller.state.attackMode)) speed *= cfg.aimFactor;
            else if (controller.state.isScoping) speed *= cfg.scopeFactor;

            if (Vectors.NonZero(controller.state.facing)) {
                float mov = AngleTo(controller.state.movement);
                float fac = AngleTo(controller.state.facing);
                float diff = Maths.AngleDiff(mov, fac);

                speed *= Mathf.Lerp(moveCfg.lookSpeedInhibiter, 1f, 1f - diff / 180f);
            }

            return speed;
        }

        private Vector3 MoveDirection(MoveConfig moveCfg, float speed, float forceY) {
            Vector3 dir = speed * new Vector3(
                controller.state.movement.x,
                forceY,
                controller.state.movement.y);
            float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;

            if (velocityDiff < moveCfg.maxVelocitydamper) {
                dir *= velocityDiff / moveCfg.maxVelocitydamper;
            }

            return dir;
        }

        private float AngleTo(Vector2 position) {
            var angle = Vectors.AngleTo(
                transform.position,
                transform.position - position.Upgrade());
            return (360f + angle) % 360;
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue)) {
                system.Send<IPubSub>(pubsub => pubsub.Publish(e));
            }
        }

        private void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            anim.Transition(state => state with {
                move = isMoving,
                sprint = state.sprint && isMoving,
            });
            controller.UpdateState(state => state with {
                movement = direction,
            });

            if (!isMoving) {
                system.Send<IPubSub>(pubsub =>
                    pubsub.Publish(MoveChangedEvent(0f)));
            }
        }

        private void OnSprintInput(bool isSprinting) {
            if (isSprinting && ControllerUtils.CanSprint(controller.state)) {
                anim.Transition(state => state with {
                    sprint = isSprinting,
                    stance = PlayerStance.STANDING
                });
            }
        }

        private void OnLookInput(Vector2 direction, bool isMouse) {
            controller.UpdateState(state => state with {
                facing = direction,
                mouseLookTimer = isMouse && Vectors.NonZero(direction)
                    ? cfg.mouseLookReset
                    : state.mouseLookTimer
            });
            anim.Transition(state => state with { sprint = false });
        }

        private void OnStanceInput() {
            PlayerStance nextStance = ControllerUtils.NextStance(controller.state.stance);
            if (!controller.state.isMoving || ControllerUtils.IsMovable(nextStance, controller.state)) {
                anim.Transition(state => state with { stance = nextStance });
            }
        }

        private void OnDiveInput() {
            anim.Transition(state => state with { dive = true });
        }

        private void OnScopeInput(bool isScoping) {
            anim.Transition(state => state with { scope = isScoping });
        }

        private void OnAimInput(bool isAiming) {
            anim.Transition(state => state with { aim = isAiming });
        }

        private void OnAttackInput(bool isAttacking) {
            if (isAttacking && ControllerUtils.CanAttack(controller.state.attackMode)) {
                anim.Transition(state => state with { attack = isAttacking });
            }
        }

        private void OnTBDInput() {
            if (controller.state.tbdTimer <= 0f) {
                controller.UpdateState(state => state with { tbdTimer = cfg.tbdCooldown });
            }
        }

        private void TransitionToLedgeHang(Collider ledge) {
            const float hardCodedFactor = 0.275f;

            Vector3 pt = ledge.ClosestPoint(transform.position);
            Vector3 direction = hardCodedFactor * (transform.position - pt).normalized;
            Vector3 nextPlayerPos = pt + direction;

            if (CanTransition(nextPlayerPos)) {
                anim.transform.localPosition = anim.transform.localPosition
                    .WithY(anim.transform.localPosition.y + 0.6f);
                anim.Transition(state => state with { hang = true });

                TransitionLedgeHangState(ledge, pt, nextPlayerPos);
            }
        }

        private bool CanTransition(Vector3 nextPlayerPos) =>
            anim.state != PlayerAnim.crawl_dive
                && Vector3.Distance(nextPlayerPos, transform.position) <= 0.35f
                && (!Physics.Raycast(nextPlayerPos, Vector3.down, out RaycastHit ground, 1000f)
                    || ground.distance >= 0.6f);

        private void TransitionLedgeHangState(Collider ledge, Vector3 pt, Vector3 nextPlayerPos) {
            Vector3 rawFacing = pt - nextPlayerPos;
            Vector2 facing = new(rawFacing.x, -rawFacing.z);
            transform.position = nextPlayerPos;

            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            controller.UpdateState(state => state with {
                movement = Vector3.zero,
                isMoving = false,
                controls = PlayerInputControlMap.None,
                facing = facing.Directionify(),
                ledge = ledge,
                hangingPoint = pt,
            });
        }
    }
}

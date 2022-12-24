using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using System;
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

        private readonly GameObject stand;
        private readonly GameObject crouch;
        private readonly GameObject crawl;

        private PlayerStandardInputState state = new() {
            mouseLookTimer = 0f,
            stance = PlayerStance.STANDING,
            attackMode = AttackMode.HAND,
            isMoving = false,
            isSprinting = false,
            isScoping = false,
            isGrounded = true,
            movement = Vector2.zero,
            facing = Vector2.zero,
        };

        public StandardInputController(IPlayerMainController controller, IGameSystem system, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.system = system;
            this.cfg = cfg;
            this.transform = transform;

            rb = transform.GetComponent<Rigidbody>();
            anim = transform.GetComponentInChildren<PlayerAnimator>();

            stand = FindStance("stand");
            crouch = FindStance("crouch");
            crawl = FindStance("crawl");
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case Facing ev: UpdateState(state => state with { facing = ev.direction }); break;
                case MovementInput ev: OnMovementInput(ev.direction); break;
                case SprintInput ev: OnSprintInput(ev.isSprinting); break;
                case LookInput ev: OnLookInput(ev.direction, ev.isMouse); break;
                case StanceInput: OnStanceInput(); break;
                case ScopeInput ev: OnScopeInput(ev.isScoping); break;
                case AimInput ev: OnAimInput(ev.isAiming); break;
                case AttackInput ev: OnAttackInput(ev.isAttacking); break;
            }
        }

        public void OnUpdate(PlayerSharedInputState common) {
            if (state.mouseLookTimer > 0f)
                UpdateState(state => state with {
                    mouseLookTimer = state.mouseLookTimer - Time.deltaTime
                });

            RotatePlayer(common, ControllerUtils.MoveCfg(cfg, state));
        }

        public void OnFixedUpdate(PlayerSharedInputState common) {
            bool prevGrounded = state.isGrounded;
            Collider ledge = state.ground.collider;
            bool wasCrouching = state.stance == PlayerStance.CROUCHING;



            bool isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out RaycastHit ground,
                cfg.groundedDist,
                ~0,
                QueryTriggerInteraction.Ignore);
            UpdateState(state => state with {
                isGrounded = isGrounded,
                ground = ground,
            });

            if (prevGrounded && !isGrounded) {
                anim.Send(PlayerAnimSignal.FALLING);
            } else if (!prevGrounded && isGrounded) {
                if (state.isSprinting && Vectors.NonZero(state.movement))
                    anim.Send(PlayerAnimSignal.LAND_SPRINT);
                else if (Vectors.NonZero(state.movement))
                    anim.Send(PlayerAnimSignal.LAND_MOVE);
                else
                    anim.Send(PlayerAnimSignal.LAND_IDLE);
            }

            MovePlayer(common, ControllerUtils.MoveCfg(cfg, state));



            bool isFallStart = prevGrounded && !state.isGrounded;
            bool isCatchable = ledge != null
                && system.Send<ITagRegistry, ISet<GameObject>>(tags =>
                    tags.Get(IdTag.PLATFORM_CATCHABLE))
                    .Contains(ledge.gameObject);

            if (isFallStart && isCatchable && wasCrouching)
                TransitionToLedgeHang(ledge);
        }

        public void OnStateEnter(PlayerAnim anim) {
            PlayerStandardInputState prevState = state;
            state = ControllerUtils.TransitionState(state, anim);

            ActivateStance();
            PublishChanged(prevState.stance, state.stance, new StanceChanged(state.stance));
            PublishChanged(prevState.attackMode, state.attackMode, new AttackModeChanged(state.attackMode));
            PublishChanged(prevState.isScoping, state.isScoping, new ScopingChanged(state.isScoping));
        }

        private void RotatePlayer(PlayerSharedInputState common, MoveConfig moveCfg) {
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

        private void MovePlayer(PlayerSharedInputState common, MoveConfig moveCfg) {
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

        private PlayerStandardInputState UpdateState(Func<PlayerStandardInputState, PlayerStandardInputState> updateFn) =>
            state = updateFn(state);

        private void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            PlayerAnimSignal signal = isMoving ? PlayerAnimSignal.MOVE_ON : PlayerAnimSignal.MOVE_OFF;
            if (anim.CanTransition(signal))
                anim.Send(signal);

            UpdateState(state => state with {
                movement = direction
            });
        }

        private void OnSprintInput(bool isSprinting) {
            PlayerAnimSignal signal = PlayerAnimSignal.SPRINT;
            if (isSprinting && ControllerUtils.CanSprint(state) && anim.CanTransition(signal))
                anim.Send(signal);
        }

        private void OnLookInput(Vector2 direction, bool isMouse) {
            PlayerAnimSignal signal = PlayerAnimSignal.LOOK;
            if (anim.CanTransition(signal))
                anim.Send(signal);

            UpdateState(state => state with {
                facing = direction,
                mouseLookTimer = isMouse && Vectors.NonZero(direction)
                    ? cfg.mouseLookReset
                    : state.mouseLookTimer
            });
        }

        private void OnStanceInput() {
            PlayerAnimSignal signal = PlayerAnimSignal.STANCE;
            PlayerStance nextStance = ControllerUtils.NextStance(state.stance);
            if (anim.CanTransition(signal)
                && (!state.isMoving || ControllerUtils.IsMovable(nextStance, state)))
                anim.Send(signal);
        }

        private void OnScopeInput(bool isScoping) {
            PlayerAnimSignal signal = isScoping ? PlayerAnimSignal.SCOPE_ON : PlayerAnimSignal.SCOPE_OFF;
            if (anim.CanTransition(signal))
                anim.Send(signal);
        }

        private void OnAimInput(bool isAiming) {
            PlayerAnimSignal signal = isAiming ? PlayerAnimSignal.AIM_ON : PlayerAnimSignal.AIM_OFF;
            if (anim.CanTransition(signal))
                anim.Send(signal);
        }

        private void OnAttackInput(bool isAttacking) {
            PlayerAnimSignal signal = PlayerAnimSignal.ATTACK;
            if (isAttacking
                && anim.CanTransition(signal)
                && ControllerUtils.CanAttack(state.attackMode))
                anim.Send(signal);
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
                anim.Send(PlayerAnimSignal.FALLING_LUNGE);

                Vector2 facing = pt - nextPlayerPos;
                rb.velocity = Vector3.zero;
                transform.position = nextPlayerPos;

                UpdateState(state => state with {
                    movement = Vector3.zero,
                    facing = facing,
                    isMoving = false,
                });
                controller
                    .Notify(new LedgeTransition(ledge, pt))
                    .Notify(new Facing(facing));
            }
        }

        private GameObject FindStance(string name) =>
            Transforms
                .FindInChildren(transform, xform => xform.name == name)
                .First()
                .gameObject;

        private void ActivateStance() {
            stand.SetActive(state.stance == PlayerStance.STANDING);
            crouch.SetActive(state.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.stance == PlayerStance.CRAWLING);
        }
    }
}

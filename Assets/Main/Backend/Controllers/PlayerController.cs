using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer, IPlayerController, IStateReceiver<PlayerAnim> {
        [SerializeField] private PlayerCfgSO cfg;

        private Rigidbody rb;
        private Animator anim;
        private PlayerState state;
        private bool isGrounded = true;
        private RaycastHit ground;

        private PlayerAnimator animController;
        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;

        public void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);

            animController.Send(isMoving ? PlayerAnimSignal.MOVE_ON : PlayerAnimSignal.MOVE_OFF);
            state = state with {
                movement = direction,
                isMoving = isMoving
            };
        }

        public void OnSprintInput(bool isSprinting) {
            if (isSprinting && ShouldTransitionToSprint()) {
                animController.Send(PlayerAnimSignal.SPRINT);
                state = state with {
                    stance = PlayerStance.STANDING,
                    isMoving = true,
                };
            }
        }

        public void OnLookInput(Vector2 direction, bool isMouse) {
            state = state with {
                facing = direction,
                stance = state.stance == PlayerStance.STANDING ? PlayerStance.CROUCHING : state.stance,
                mouseLookTimer = isMouse && Vectors.NonZero(direction) ? cfg.mouseLookReset : state.mouseLookTimer
            };
        }

        public void OnStanceInput() {
            PlayerStance nextStance = PCUtils.NextStance(state.stance);
            if (!state.isMoving || PCUtils.IsMovable(nextStance, state)) {
                animController.Send(PlayerAnimSignal.STANCE);
            }
        }

        public void OnAimInput(bool isAiming) {
            animController.Send(isAiming ? PlayerAnimSignal.AIM_ON : PlayerAnimSignal.AIM_OFF);
        }

        public void OnAttackInput(bool isAttacking) {
            if (isAttacking && PCUtils.CanAttack(state.attackMode))
                animController.Send(PlayerAnimSignal.ATTACK);
        }

        public void OnScopeInput(bool isScoping) {
            animController.Send(isScoping ? PlayerAnimSignal.SCOPE_ON : PlayerAnimSignal.SCOPE_OFF);
        }

        public void OnPlayerStep() { }

        private void Start() {
            rb = GetComponent<Rigidbody>();
            anim = GetComponentInChildren<Animator>();
            animController = GetComponentInChildren<PlayerAnimator>();

            stand = Transforms
                .FindInChildren(transform, xform => xform.name == "stand")
                .First()
                .gameObject;
            crouch = Transforms
                .FindInChildren(transform, xform => xform.name == "crouch")
                .First()
                .gameObject;
            crawl = Transforms
                .FindInChildren(transform, xform => xform.name == "crawl")
                .First()
                .gameObject;

            state = new PlayerState {
                movement = Vector2.zero,
                facing = Vector2.zero,
                stance = PlayerStance.STANDING,
                attackMode = AttackMode.HAND,
                mouseLookTimer = 0f,
                isMoving = false,
                isScoping = false
            };
            ActivateStance();
            animController.Init(this);
        }

        private void Update() {
            if (state.mouseLookTimer > 0f)
                state = state with { mouseLookTimer = state.mouseLookTimer - Time.deltaTime };
            RotatePlayer(MoveCfg());
        }

        private void FixedUpdate() {
            bool prevGrounded = isGrounded;
            isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out ground,
                cfg.groundedDist);

            if (prevGrounded && !isGrounded) {
                animController.Send(PlayerAnimSignal.FALLING);
            } else if (!prevGrounded && isGrounded) {
                if (state.isMoving) animController.Send(PlayerAnimSignal.LAND_MOVING);
                else animController.Send(PlayerAnimSignal.LAND_STILL);
            }

            MovePlayer(MoveCfg());
        }

        private void RotatePlayer(MoveConfig moveCfg) {
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
            if (isGrounded && PCUtils.IsMovable(state.stance, state)) {
                float speed = moveCfg.moveSpeed;
                float forceZ = ground.transform.rotation != Quaternion.identity && Vectors.NonZero(state.movement)
                    ? (Vector3.Angle(ground.normal, state.movement) - 90f) / 90f
                    : 0f;

                if (PCUtils.IsAiming(state.attackMode)) speed *= cfg.aimFactor;
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

                if (isForceable && Vectors.NonZero(state.movement)) {
                    anim.speed = movementSpeed * speed * Time.fixedDeltaTime * moveCfg.animFactor;

                    if (state.stance == PlayerStance.STANDING)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private MoveConfig MoveCfg() =>
            state.stance switch {
                PlayerStance.CROUCHING => cfg.crouching,
                PlayerStance.CRAWLING => cfg.crawling,
                _ => cfg.sprinting
            };

        private void ActivateStance() {
            stand.SetActive(state.stance == PlayerStance.STANDING);
            crouch.SetActive(state.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.stance == PlayerStance.CRAWLING);
        }

        private bool ShouldTransitionToSprint() =>
            !state.isScoping && !PCUtils.IsAiming(state.attackMode);

        private void PublishMessage(IEvent message) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(message));

        private static class PCUtils {
            public static PlayerStance NextStance(PlayerStance stance) {
                if (stance == PlayerStance.CROUCHING)
                    return PlayerStance.CRAWLING;
                return PlayerStance.CROUCHING;
            }

            public static bool IsAiming(AttackMode mode) =>
                mode == AttackMode.WEAPON || mode == AttackMode.FIRING;

            public static bool IsMovable(PlayerStance stance, PlayerState state) =>
                stance != PlayerStance.CRAWLING
                || (!IsAiming(state.attackMode) && !state.isScoping);

            public static bool CanAttack(AttackMode mode) =>
                mode != AttackMode.NONE
                    && mode != AttackMode.FIRING
                    && mode != AttackMode.MELEE;
        }

        public void OnStateEnter(PlayerAnim anim) {
            this.anim.Play(anim.ToString());
            PlayerState prevState = state;

            state = anim switch {
                PlayerAnim.stand_idle => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.HAND,
                    isMoving = false,
                },
                PlayerAnim.stand_move => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.HAND,
                    isMoving = true,
                    isScoping = false,
                },
                PlayerAnim.stand_punch => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.MELEE,
                },
                PlayerAnim.stand_fall => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.NONE,
                    isScoping = false,
                },

                PlayerAnim.crouch_idle_bino => state with {
                    stance = PlayerStance.CROUCHING,
                    isMoving = false,
                    isScoping = true,
                },
                PlayerAnim.crouch_move_bino => state with {
                    stance = PlayerStance.CROUCHING,
                    isMoving = true,
                    isScoping = true,
                },
                PlayerAnim.crouch_tobino => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.NONE,
                    isScoping = true,
                },
                PlayerAnim.crouch_idle => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.HAND,
                    isMoving = false,
                    isScoping = false,
                },
                PlayerAnim.crouch_move => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.HAND,
                    isMoving = true,
                    isScoping = false,
                },
                PlayerAnim.crouch_punch => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.MELEE,
                },
                PlayerAnim.crouch_toaim => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.NONE,
                },
                PlayerAnim.crouch_idle_aim => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.WEAPON,
                    isMoving = false,
                },
                PlayerAnim.crouch_move_aim => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.WEAPON,
                    isMoving = true,
                },
                PlayerAnim.crouch_fire => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.FIRING,
                },

                PlayerAnim.crawl_idle_bino => state with {
                    stance = PlayerStance.CRAWLING,
                },
                PlayerAnim.crawl_tobino => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.NONE,
                    isScoping = true,
                },
                PlayerAnim.crawl_idle => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.HAND,
                    isMoving = false,
                },
                PlayerAnim.crawl_move => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.HAND,
                    isMoving = true,
                    isScoping = false,
                },
                PlayerAnim.crawl_punch => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.MELEE,
                },
                PlayerAnim.crawl_toaim => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.NONE,
                },
                PlayerAnim.crawl_idle_aim => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.WEAPON,
                    isMoving = false,
                },
                PlayerAnim.crawl_fire => state with {
                    stance = PlayerStance.CRAWLING,
                    attackMode = AttackMode.FIRING,
                },

                _ => state
            };

            ActivateStance();

            PublishChanged(prevState.stance, state.stance, new StanceChanged(state.stance));
            PublishChanged(prevState.attackMode, state.attackMode, new AttackModeChanged(state.attackMode));
            PublishChanged(prevState.isMoving, state.isMoving, new MovementChanged(state.isMoving));
            PublishChanged(prevState.isScoping, state.isScoping, new ScopingChanged(state.isScoping));
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue))
                PublishMessage(e);
        }
    }
}

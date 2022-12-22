using OSBE.Controllers.Player.Interfaces;
using OSBE.Controllers.Player;
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
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer, IPlayerController, IPlayerMainController, IStateReceiver<PlayerAnim> {
        private static readonly PlayerState DEFAULT_STATE = new() {
            input = new() {
                movement = Vector2.zero,
                facing = Vector2.zero,
                mouseLookTimer = 0f,
                controls = PlayerInputControlMap.Standard,
            },
            stance = PlayerStance.STANDING,
            attackMode = AttackMode.HAND,
            isMoving = false,
            isSprinting = false,
            isScoping = false,
            isGrounded = true,
        };

        [SerializeField] private PlayerCfgSO cfg;

        private PlayerState state;
        private PlayerAnimator animController;
        private PlayerInput input;
        private IPlayerInputController stdInput;

        private Rigidbody rb;
        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;
        private GameObject hang;

        public void Publish(IEvent e) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));

        public PlayerState UpdateState(Func<PlayerState, PlayerState> updateFn) =>
            state = updateFn(state);

        public void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);

            animController.Send(isMoving ? PlayerAnimSignal.MOVE_ON : PlayerAnimSignal.MOVE_OFF);
            UpdateState(state => state with {
                input = state.input with {
                    movement = direction
                }
            });
        }

        public void OnSprintInput(bool isSprinting) {
            if (isSprinting && PlayerControllerUtils.ShouldTransitionToSprint(state))
                animController.Send(PlayerAnimSignal.SPRINT);
        }

        public void OnLookInput(Vector2 direction, bool isMouse) {
            animController.Send(PlayerAnimSignal.LOOK);
            UpdateState(state => state with {
                input = state.input with {
                    facing = direction,
                    mouseLookTimer = isMouse && Vectors.NonZero(direction)
                        ? cfg.mouseLookReset
                        : state.input.mouseLookTimer
                }
            });
        }

        public void OnStanceInput() {
            PlayerStance nextStance = PlayerControllerUtils.NextStance(state.stance);
            if (!state.isMoving || PlayerControllerUtils.IsMovable(nextStance, state)) {
                animController.Send(PlayerAnimSignal.STANCE);
            }
        }

        public void OnAimInput(bool isAiming) {
            animController.Send(isAiming ? PlayerAnimSignal.AIM_ON : PlayerAnimSignal.AIM_OFF);
        }

        public void OnAttackInput(bool isAttacking) {
            if (isAttacking && PlayerControllerUtils.CanAttack(state.attackMode))
                animController.Send(PlayerAnimSignal.ATTACK);
        }

        public void OnScopeInput(bool isScoping) {
            animController.Send(isScoping ? PlayerAnimSignal.SCOPE_ON : PlayerAnimSignal.SCOPE_OFF);
        }

        public void OnClimbInput(bool isClimbing) {
            if (isClimbing) {
                animController.Send(PlayerAnimSignal.LEDGE_CLIMB);
                transform.position -= new Vector3(0, 0, 0.1f);
            }
        }

        public void OnDropInput(bool isDropping) {
            if (isDropping) {
                animController.Send(PlayerAnimSignal.LEDGE_DROP);
                UpdateState(state => state with {
                    input = state.input with {
                        facing = Vector3.zero,
                    }
                });
            }
        }

        public void OnPlayerStep() { }

        public void OnStateEnter(PlayerAnim prev, PlayerAnim curr) {
            PlayerState prevState = state;
            UpdateState(state => PlayerControllerUtils.TransitionState(state, curr));

            string controls = state.input.controls.ToString();
            if (input.currentActionMap.name != controls)
                input.SwitchCurrentActionMap(controls);

            ActivateStance();
            PublishChanged(prevState.stance, state.stance, new StanceChanged(state.stance));
            PublishChanged(prevState.attackMode, state.attackMode, new AttackModeChanged(state.attackMode));
            PublishChanged(prevState.isScoping, state.isScoping, new ScopingChanged(state.isScoping));
        }

        public void OnStateExit(PlayerAnim prev, PlayerAnim curr) {
            if (prev.ToString().StartsWith("hang") && !curr.ToString().StartsWith("hang")) {
                rb.isKinematic = false;
                UpdateState(state => state with {
                    input = state.input with {
                        controls = PlayerInputControlMap.Standard
                    }
                });
            }
        }

        private void Start() {
            stdInput = new StandardInputController(this, cfg, transform);
            animController = GetComponentInChildren<PlayerAnimator>();
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();

            stand = FindStance("stand");
            crouch = FindStance("crouch");
            crawl = FindStance("crawl");
            hang = FindStance("hang");

            state = DEFAULT_STATE;
            ActivateStance();
        }

        private void Update() {
            stdInput.OnUpdate(state);
        }

        private void FixedUpdate() {
            bool prevGrounded = state.isGrounded;
            Collider ledge = state.ground.collider;
            bool wasCrouching = state.stance == PlayerStance.CROUCHING;

            stdInput.OnFixedUpdate(state);

            bool isFallStart = prevGrounded && !state.isGrounded;

            if (isFallStart && ledge != null && wasCrouching) {
                animController.SetSpeed(0.5f);
                animController.Send(PlayerAnimSignal.FALLING_LUNGE);
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                Vector3 pt = ledge.ClosestPoint(transform.position);
                Vector3 diff = transform.position - pt;
                Vector2 direction = diff.normalized;
                transform.position += (direction * 0.25f).Upgrade();

                UpdateState(state => state with {
                    input = state.input with {
                        movement = Vector3.zero,
                        facing = pt - transform.position,
                    },
                    ledge = ledge,
                });
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
            hang.SetActive(state.stance == PlayerStance.HANGING);
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue)) Publish(e);
        }
    }
}

using OSBE.Controllers.Player.Interfaces;
using OSBE.Controllers.Player;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer, IController<PlayerControllerInput>, IPlayerMainController, IStateReceiver<PlayerAnim> {
        private static readonly PlayerState DEFAULT_STATE = new() {
            common = new() {
                anim = PlayerAnim.crouch_idle,
                animSpeed = 1f,
                controls = PlayerInputControlMap.Standard,
            },
            std = new() {
                mouseLookTimer = 0f,
                stance = PlayerStance.STANDING,
                attackMode = AttackMode.HAND,
                isMoving = false,
                isSprinting = false,
                isScoping = false,
                isGrounded = true,
                movement = Vector2.zero,
                facing = Vector2.zero,
            },
            hang = new() {
                hangingPoint = Vector3.zero,
                ledge = default,
            }
        };

        [SerializeField] private PlayerCfgSO cfg;

        private PlayerState state;
        private PlayerAnimator anim;
        private PlayerInput input;
        private IDictionary<PlayerInputControlMap, IPlayerInputController> controllers;

        private Rigidbody rb;
        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;
        private GameObject hang;

        public void On(PlayerControllerInput e) {
            switch (e) {
                case MovementInput ev: OnMovementInput(ev.direction); break;
                case SprintInput ev: OnSprintInput(ev.isSprinting); break;
                case LookInput ev: OnLookInput(ev.direction, ev.isMouse); break;
                case StanceInput: OnStanceInput(); break;
                case ScopeInput ev: OnScopeInput(ev.isScoping); break;
                case AimInput ev: OnAimInput(ev.isAiming); break;
                case AttackInput ev: OnAttackInput(ev.isAttacking); break;
                case ClimbInput ev:
                    if (ev.direction == ClimbDirection.UP) OnClimbUp();
                    else OnClimbDown();
                    break;
            }
        }

        public void Publish(IEvent e) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));

        public PlayerState UpdateState(Func<PlayerState, PlayerState> updateFn) =>
            state = updateFn(state);

        public void OnStateExit(PlayerAnim state) {
            switch (state) {
                case PlayerAnim.stand_move:
                case PlayerAnim.crouch_move:
                case PlayerAnim.crouch_move_aim:
                case PlayerAnim.crouch_move_bino:
                case PlayerAnim.crawl_move:
                    anim.SetSpeed(1f);
                    break;
            }
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            if (prev.ToString().StartsWith("hang") && !curr.ToString().StartsWith("hang")) {
                rb.isKinematic = false;
                UpdateState(state => state with {
                    common = state.common with {
                        controls = PlayerInputControlMap.Standard
                    }
                });
            }
        }

        public void OnStateEnter(PlayerAnim anim) {
            PlayerState prevState = state;
            UpdateState(state => PlayerControllerUtils.TransitionState(state, anim));

            if (state.common.controls != PlayerInputControlMap.None) {
                string controls = state.common.controls.ToString();
                if (input.currentActionMap.name != controls)
                    input.SwitchCurrentActionMap(controls);
            }

            ActivateStance();
            PublishChanged(prevState.std.stance, state.std.stance, new StanceChanged(state.std.stance));
            PublishChanged(prevState.std.attackMode, state.std.attackMode, new AttackModeChanged(state.std.attackMode));
            PublishChanged(prevState.std.isScoping, state.std.isScoping, new ScopingChanged(state.std.isScoping));
        }

        private void Start() {
            controllers = new Dictionary<PlayerInputControlMap, IPlayerInputController>() {
                { PlayerInputControlMap.None, new NoopInputController() },
                { PlayerInputControlMap.Standard, new StandardInputController(this, cfg, transform) },
                { PlayerInputControlMap.LedgeHang, new LedgeHangInputController(this, cfg, transform) }
            };
            anim = GetComponentInChildren<PlayerAnimator>();
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
            controllers.Get(state.common.controls).OnUpdate(state);
        }

        private void FixedUpdate() {
            bool prevGrounded = state.std.isGrounded;
            Collider ledge = state.std.ground.collider;
            bool wasCrouching = state.std.stance == PlayerStance.CROUCHING;

            controllers.Get(state.common.controls).OnFixedUpdate(state);

            bool isFallStart = prevGrounded && !state.std.isGrounded;
            bool isCatchable = ledge != null
                && system.Send<ITagRegistry, ISet<GameObject>>(tags =>
                    tags.Get(IdTag.PLATFORM_CATCHABLE))
                    .Contains(ledge.gameObject);

            if (isFallStart && isCatchable && wasCrouching)
                TransitionToLedgeHang(ledge);
        }

        private GameObject FindStance(string name) =>
            Transforms
                .FindInChildren(transform, xform => xform.name == name)
                .First()
                .gameObject;

        private void ActivateStance() {
            stand.SetActive(state.std.stance == PlayerStance.STANDING);
            crouch.SetActive(state.std.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.std.stance == PlayerStance.CRAWLING);
            hang.SetActive(state.std.stance == PlayerStance.HANGING);
        }

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue)) Publish(e);
        }

        private void TransitionToLedgeHang(Collider ledge) {
            float distanceToGround = float.PositiveInfinity;
            if (Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out RaycastHit ground,
                1000f))
                distanceToGround = ground.distance;

            if (distanceToGround >= 0.6f) {
                anim.Send(PlayerAnimSignal.FALLING_LUNGE);
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;

                Vector3 pt = ledge.ClosestPoint(transform.position);
                Vector2 direction = (transform.position - pt).normalized;
                transform.position += (direction * 0.275f).Upgrade();

                UpdateState(state => state with {
                    std = state.std with {
                        movement = Vector3.zero,
                        facing = pt - transform.position,
                    },
                    hang = state.hang with {
                        hangingPoint = pt,
                        ledge = ledge,
                    }
                });
            }
        }

        private void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            PlayerAnimSignal signal = isMoving ? PlayerAnimSignal.MOVE_ON : PlayerAnimSignal.MOVE_OFF;
            if (anim.CanTransition(signal))
                anim.Send(signal);

            UpdateState(state => state with {
                std = state.std with {
                    movement = direction
                }
            });
        }

        private void OnSprintInput(bool isSprinting) {
            PlayerAnimSignal signal = PlayerAnimSignal.SPRINT;
            if (isSprinting && PlayerControllerUtils.CanSprint(state) && anim.CanTransition(signal))
                anim.Send(signal);
        }

        private void OnLookInput(Vector2 direction, bool isMouse) {
            PlayerAnimSignal signal = PlayerAnimSignal.LOOK;
            if (anim.CanTransition(signal))
                anim.Send(signal);

            UpdateState(state => state with {
                std = state.std with {
                    facing = direction,
                    mouseLookTimer = isMouse && Vectors.NonZero(direction)
                        ? cfg.mouseLookReset
                        : state.std.mouseLookTimer
                }
            });
        }

        private void OnStanceInput() {
            PlayerAnimSignal signal = PlayerAnimSignal.STANCE;
            PlayerStance nextStance = PlayerControllerUtils.NextStance(state.std.stance);
            if (anim.CanTransition(signal)
                && (!state.std.isMoving || PlayerControllerUtils.IsMovable(nextStance, state)))
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
                && PlayerControllerUtils.CanAttack(state.std.attackMode))
                anim.Send(signal);
        }

        private void OnClimbUp() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_CLIMB;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                Vector3 diff = (state.hang.hangingPoint - transform.position) * 1.2f;
                transform.position += diff;
                UpdateState(state => state with {
                    std = state.std with {
                        facing = Vector3.zero,
                    }
                });
            }
        }

        private void OnClimbDown() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_DROP;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                float pointZ = state.hang.hangingPoint.z;
                transform.position = transform.position.WithZ(pointZ + 0.55f);
                UpdateState(state => state with {
                    std = state.std with {
                        facing = Vector3.zero,
                    }
                });
            }
        }
    }

    internal class NoopInputController : IPlayerInputController {
        public void On(PlayerControllerInput e) { }
    }
}

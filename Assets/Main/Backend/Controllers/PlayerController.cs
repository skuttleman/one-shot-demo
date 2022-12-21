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
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer, IPlayerController, IPlayerMainController, IStateReceiver<PlayerAnim> {
        private static readonly PlayerState DEFAULT_STATE = new() {
            input = new() {
                movement = Vector2.zero,
                facing = Vector2.zero,
                mouseLookTimer = 0f,
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
        private IPlayerInputController stdInput;

        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;

        public void Publish(IEvent e) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));

        public PlayerState UpdateState(Func<PlayerState, PlayerState> updateFn) {
            PlayerState nextState = updateFn(state);
            return state = nextState;
        }

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

        public void OnPlayerStep() { }

        public void OnStateEnter(PlayerAnim anim) {
            PlayerState prevState = state;

            UpdateState(state => PlayerControllerUtils.TransitionState(state, anim));

            ActivateStance();
            PublishChanged(prevState.stance, state.stance, new StanceChanged(state.stance));
            PublishChanged(prevState.attackMode, state.attackMode, new AttackModeChanged(state.attackMode));
            PublishChanged(prevState.isScoping, state.isScoping, new ScopingChanged(state.isScoping));
        }

        private void Start() {
            stdInput = new StandardInputController(this, cfg, transform);
            animController = GetComponentInChildren<PlayerAnimator>();

            stand = FindStance("stand");
            crouch = FindStance("crouch");
            crawl = FindStance("crawl");

            state = DEFAULT_STATE;
            ActivateStance();
        }

        private void Update() {
            stdInput.OnUpdate(state);
        }

        private void FixedUpdate() {
            stdInput.OnFixedUpdate(state);
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

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue)) Publish(e);
        }
    }
}

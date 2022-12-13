using OSBE.Async.Core;
using OSCore.Data.Enums;
using OSCore.Data.Events.Brains;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;
using OSCore.Data;

namespace OSBE.Controllers {
    public class PlayerStateReducer : IPlayerStateReducer {
        private static readonly string ANIM_STANCE = "stance";
        private static readonly string ANIM_MOVE = "isMoving";
        private static readonly string ANIM_SCOPE = "isScoping";
        private static readonly string ANIM_AIM = "isAiming";
        private static readonly string ANIM_ATTACK = "isAttacking";

        private readonly IGameSystem system;
        private readonly Transform target;
        private readonly Animator anim;

        private IStateReceiver<PlayerState> receiver;
        private PlayerCfgSO cfg = null;
        private PlayerState state;
        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;

        public PlayerStateReducer(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
            anim = target.gameObject.GetComponentInChildren<Animator>();

            state = new PlayerState {
                movement = Vector2.zero,
                facing = Vector2.zero,
                stance = PlayerStance.STANDING,
                attackMode = AttackMode.HAND,
                mouseLookTimer = 0f,
                isMoving = false,
                isSprinting = false,
                isScoping = false
            };
        }

        public void OnUpdate() {
            if (state.mouseLookTimer > 0f) {
                state = state with { mouseLookTimer = state.mouseLookTimer - Time.deltaTime };
                receiver.OnStateChange(state);
            }
        }

        public void Init(IStateReceiver<PlayerState> receiver, PlayerCfgSO cfg) {
            this.receiver = receiver;
            this.cfg = cfg;
            stand = GameObject.Find("/Characters/Player/Entity/stand");
            crouch = GameObject.Find("/Characters/Player/Entity/crouch");
            crawl = GameObject.Find("/Characters/Player/Entity/crawl");

            EmitState(state);
            ActivateStance();
        }

        public void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            anim.SetBool(ANIM_MOVE, isMoving);

            EmitState(state with {
                isMoving = isMoving,
                movement = direction,
                isSprinting = isMoving && state.isSprinting
            });
        }

        public void OnSprintInput(bool isSprinting) {
            if (isSprinting && ShouldTransitionToSprint()) {
                anim.SetInteger(ANIM_STANCE, (int)PlayerStance.STANDING);
                EmitState(state with {
                    stance = PlayerStance.STANDING,
                    isSprinting = true
                });
            }
        }

        public void OnLookInput(Vector2 direction, bool isMouse) {
            EmitState(state with {
                facing = direction,
                isSprinting = false,
                mouseLookTimer = isMouse && Vectors.NonZero(direction) ? cfg.mouseLookReset : state.mouseLookTimer
            });
        }

        public void OnStanceInput(float holdDuration) {
            PlayerStance nextStance = PCUtils.NextStance(
                cfg,
                state.stance,
                holdDuration);
            if (!state.isMoving || PCUtils.IsMovable(nextStance, state))
                anim.SetInteger(ANIM_STANCE, (int)nextStance);

            EmitState(state with { isSprinting = false });
        }

        public void OnAimInput(bool isAiming) {
            anim.SetBool(ANIM_AIM, isAiming);
            EmitState(state with { isSprinting = false });
        }

        public void OnAttackInput(bool isAttacking) {
            if (isAttacking && PCUtils.CanAttack(state.attackMode)) {
                float attackSpeed = state.attackMode == AttackMode.HAND
                    ? cfg.punchingSpeed
                    : cfg.firingSpeed;

                system.Send<PromiseFactory, IPromise<dynamic>>(promises => {
                    anim.SetBool(ANIM_ATTACK, true);
                    return promises
                        .Await(attackSpeed)
                        .Then(() => anim.SetBool(ANIM_ATTACK, false));
                });

                EmitState(state with { isSprinting = false });
            }
        }

        public void OnScopeInput(bool isScoping) {
            anim.SetBool(ANIM_SCOPE, isScoping);

            EmitState(state with { isSprinting = !isScoping && state.isScoping });
        }

        public void OnStanceChanged(PlayerStance stance) {
            if (state.stance != stance) {
                EmitState(state with { stance = stance });
                ActivateStance();
                PublishMessage(new StanceChanged(stance));
            }
        }

        public void OnAttackModeChanged(AttackMode attackMode) {
            if (state.attackMode != attackMode) {
                EmitState(state with { attackMode = attackMode });
                PublishMessage(new AttackModeChanged(attackMode));
            }
        }

        public void OnMovementChanged(bool isMoving) {
            if (state.isMoving != isMoving) {
                EmitState(state with { isMoving = isMoving });
                PublishMessage(new MovementChanged(isMoving));
            }
        }

        public void OnScopingChanged(bool isScoping) {
            if (state.isScoping != isScoping) {
                EmitState(state with { isScoping = isScoping });
                PublishMessage(new ScopingChanged(isScoping));
            }
        }

        public void OnPlayerStep() { }

        private bool ShouldTransitionToSprint() =>
                !state.isSprinting && !state.isScoping && !PCUtils.IsAiming(state.attackMode);

        private void PublishMessage(IEvent message) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(message));

        private void ActivateStance() {
            stand.SetActive(state.stance == PlayerStance.STANDING);
            crouch.SetActive(state.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.stance == PlayerStance.CRAWLING);
        }

        private void EmitState(PlayerState state) {
            this.state = state;
            receiver.OnStateChange(state);
        }
    }

    public static class PCUtils {
        public static PlayerStance NextStance(PlayerCfgSO cfg, PlayerStance stance, float stanceDuration) {
            bool held = stanceDuration >= cfg.stanceChangeHeldThreshold;

            if (held && stance == PlayerStance.CRAWLING)
                return PlayerStance.STANDING;
            else if (held)
                return PlayerStance.CRAWLING;
            else if (stance == PlayerStance.CROUCHING)
                return PlayerStance.STANDING;
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
}
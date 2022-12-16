using OSCore.Data.Enums;
using OSCore.Data.Events.Brains;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer, IPlayerController {
        private static readonly string ANIM_STANCE = "stance";
        private static readonly string ANIM_MOVE = "isMoving";
        private static readonly string ANIM_SCOPE = "isScoping";
        private static readonly string ANIM_AIM = "isAiming";
        private static readonly string ANIM_ATTACK = "isAttacking";

        [SerializeField] private PlayerCfgSO cfg;

        private Rigidbody rb;
        private Animator anim;
        private PlayerState state;
        private bool isGrounded = false;
        private RaycastHit ground;

        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;

        public void OnMovementInput(Vector2 direction) {
            bool isMoving = Vectors.NonZero(direction);
            anim.SetBool(ANIM_MOVE, isMoving);

            state = state with {
                isMoving = isMoving,
                movement = direction,
                isSprinting = isMoving && state.isSprinting
            };
        }

        public void OnSprintInput(bool isSprinting) {
            if (state.isSprinting && ShouldTransitionToSprint()) {
                anim.SetInteger(ANIM_STANCE, (int)PlayerStance.STANDING);
                state = state with {
                    stance = PlayerStance.STANDING,
                    isSprinting = true
                };
            }
        }

        public void OnLookInput(Vector2 direction, bool isMouse) {
            state = state with {
                facing = direction,
                isSprinting = false,
                mouseLookTimer = isMouse && Vectors.NonZero(direction) ? cfg.mouseLookReset : state.mouseLookTimer
            };
        }

        public void OnStanceInput(float holdDuration) {
            PlayerStance nextStance = PCUtils.NextStance(
                cfg,
                state.stance,
                holdDuration);
            if (!state.isMoving || PCUtils.IsMovable(nextStance, state))
                anim.SetInteger(ANIM_STANCE, (int)nextStance);

            state = state with { isSprinting = false };
        }

        public void OnAimInput(bool isAiming) {
            anim.SetBool(ANIM_AIM, isAiming);
            state = state with { isSprinting = false };
        }

        public void OnAttackInput(bool isAttacking) {
            if (isAttacking && PCUtils.CanAttack(state.attackMode)) {
                float attackSpeed = state.attackMode == AttackMode.HAND
                    ? cfg.punchingSpeed
                    : cfg.firingSpeed;
                anim.SetBool(ANIM_ATTACK, true);

                state = state with { isSprinting = false };

                StartCoroutine(PCUtils.After(
                    attackSpeed,
                    () => anim.SetBool(ANIM_ATTACK, false)));
            }
        }

        public void OnScopeInput(bool isScoping) {
            anim.SetBool(ANIM_SCOPE, isScoping);
            state = state with { isSprinting = !isScoping && state.isScoping };
        }

        public void OnStanceChanged(PlayerStance stance) {
            if (state.stance != stance) {
                state = state with { stance = stance };
                ActivateStance();
                PublishMessage(new StanceChanged(stance));
            }
        }

        public void OnAttackModeChanged(AttackMode attackMode) {
            if (state.attackMode != attackMode) {
                state = state with { attackMode = attackMode };
                PublishMessage(new AttackModeChanged(attackMode));
            }
        }

        public void OnMovementChanged(bool isMoving) {
            if (state.isMoving != isMoving) {
                state = state with { isMoving = isMoving };
                PublishMessage(new MovementChanged(isMoving));
            }
        }

        public void OnScopingChanged(bool isScoping) {
            if (state.isScoping != isScoping) {
                state = state with { isScoping = isScoping };
                PublishMessage(new ScopingChanged(isScoping));
            }
        }

        public void OnPlayerStep() { }

        private void Start() {
            rb = GetComponent<Rigidbody>();
            anim = GetComponentInChildren<Animator>();

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
                isSprinting = false,
                isScoping = false
            };
            ActivateStance();
        }

        private void Update() {
            if (state.mouseLookTimer > 0f)
                state = state with { mouseLookTimer = state.mouseLookTimer - Time.deltaTime };
            RotatePlayer(MoveCfg());
        }

        private void FixedUpdate() {
            isGrounded = Physics.Raycast(
                transform.position - new Vector3(0, 0, 0.01f),
                Vectors.DOWN,
                out ground,
                cfg.groundedDist);

            anim.SetBool("isGrounded", isGrounded);

            if (!isGrounded) {
                anim.SetBool("isAiming", false);
                anim.SetBool("isScoping", false);
                anim.SetInteger("stance", 0);
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

                    if (state.isSprinting)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        private MoveConfig MoveCfg() =>
                state.stance switch {
                    PlayerStance.CROUCHING => cfg.crouching,
                    PlayerStance.CRAWLING => cfg.crawling,
                    _ => state.isSprinting ? cfg.sprinting : cfg.standing
                };

        private void ActivateStance() {
            stand.SetActive(state.stance == PlayerStance.STANDING);
            crouch.SetActive(state.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.stance == PlayerStance.CRAWLING);
        }

        private bool ShouldTransitionToSprint() =>
                !state.isSprinting && !state.isScoping && !PCUtils.IsAiming(state.attackMode);

        private void PublishMessage(IEvent message) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(message));

        private static class PCUtils {
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

            public static IEnumerator<YieldInstruction> After(float seconds, Action cb) {
                yield return new WaitForSeconds(seconds);
                cb();
            }
        }
    }
}

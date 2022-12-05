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
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Brains {
    public class PlayerControllerBrain : IPlayerControllerBrain {
        static readonly string ANIM_STANCE = "stance";
        static readonly string ANIM_MOVE = "isMoving";
        static readonly string ANIM_SCOPE = "isScoping";
        static readonly string ANIM_AIM = "isAiming";
        static readonly string ANIM_ATTACK = "isAttacking";

        PlayerCfgSO cfg = null;
        readonly IGameSystem system;
        readonly Transform target;
        readonly Rigidbody rb;
        readonly Animator anim;
        readonly GameObject stand;
        readonly GameObject crouch;
        readonly GameObject crawl;

        // movement state
        Vector2 movement = Vector2.zero;
        Vector2 facing = Vector2.zero;
        PlayerStance stance = PlayerStance.STANDING;
        AttackMode attackMode = AttackMode.HAND;
        float mouseLookTimer = 0f;
        bool isGrounded = false;
        bool isMoving = false;
        bool isSprinting = false;
        bool isScoping = false;

        public PlayerControllerBrain(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
            rb = target.GetComponent<Rigidbody>();
            anim = target.gameObject.GetComponentInChildren<Animator>();

            // TODO - this better
            stand = GameObject.Find("/Characters/Player/Entity/stand");
            crouch = GameObject.Find("/Characters/Player/Entity/crouch");
            crawl = GameObject.Find("/Characters/Player/Entity/crawl");
            ActivateStance();
        }

        public void Update() {
            if (cfg is not null)
                RotatePlayer(MoveCfg());
        }

        public void FixedUpdate() {
            if (cfg is not null) {
                isGrounded = Physics.Raycast(
                    target.position - new Vector3(0, 0, 0.01f),
                    Vectors.DOWN,
                    cfg.groundedDist);
                MovePlayer(MoveCfg());
            }
        }

        void RotatePlayer(MoveConfig moveCfg) {
            if (mouseLookTimer > 0f) mouseLookTimer -= Time.deltaTime;

            Vector2 direction;

            if (Vectors.NonZero(facing))
                direction = facing;
            else if (mouseLookTimer <= 0f && Vectors.NonZero(movement))
                direction = movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            target.rotation = Quaternion.Lerp(
                    target.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    moveCfg.rotationSpeed * Time.deltaTime);
        }

        void MovePlayer(MoveConfig moveCfg) {
            if (PBUtils.IsMovable(stance, attackMode, isGrounded, isScoping)) {
                float speed = moveCfg.moveSpeed;

                if (PBUtils.IsAiming(attackMode)) speed *= cfg.aimFactor;
                else if (isScoping) speed *= cfg.scopeFactor;

                float movementSpeed = Mathf.Max(
                    Mathf.Abs(movement.x),
                    Mathf.Abs(movement.y));
                bool isForceable = rb.velocity.magnitude < moveCfg.maxVelocity;
                Vector3 dir = speed * movement.Upgrade();

                float velocityDiff = moveCfg.maxVelocity - rb.velocity.magnitude;
                if (velocityDiff < moveCfg.maxVelocitydamper)
                    dir *= velocityDiff / moveCfg.maxVelocitydamper;

                if (isForceable && Vectors.NonZero(movement)) {
                    anim.speed = movementSpeed * speed * Time.fixedDeltaTime * moveCfg.animFactor;

                    if (isSprinting)
                        rb.AddRelativeForce(Vectors.FORWARD * dir.magnitude);
                    else rb.AddForce(dir);
                }
            }
        }

        /*
         *
         * Input Event Handlers
         *
         */

        public void OnMovementInput(Vector2 direction) {
            movement = direction;
            bool isMoving = Vectors.NonZero(movement);
            anim.SetBool(ANIM_MOVE, isMoving);

            if (!isMoving) isSprinting = false;
        }

        public void OnSprintInput(bool isSprinting) {
            if (isSprinting && ShouldTransitionToSprint()) {
                this.isSprinting = true;
                stance = PlayerStance.STANDING;
                anim.SetInteger(ANIM_STANCE, (int)stance);
            }
        }

        public void OnLookInput(Vector2 direction, bool isMouse) {
            facing = direction;
            isSprinting = false;

            if (isMouse && Vectors.NonZero(direction))
                mouseLookTimer = cfg.mouseLookReset;
        }

        public void OnStanceInput(float holdDuration) {
            isSprinting = false;
            PlayerStance nextStance = PBUtils.NextStance(
                cfg,
                stance,
                holdDuration);

            if (!isMoving || PBUtils.IsMovable(nextStance, attackMode, isGrounded, isScoping))
                anim.SetInteger(ANIM_STANCE, (int)nextStance);
        }

        public void OnAimInput(bool isAiming) {
            isSprinting = false;
            anim.SetBool(ANIM_AIM, isAiming);
        }

        public void OnAttackInput(bool isAttacking) {
            if (isAttacking && PBUtils.CanAttack(attackMode)) {
                isSprinting = false;
                float attackSpeed = attackMode == AttackMode.HAND
                    ? cfg.punchingSpeed
                    : cfg.firingSpeed;

                system.Send<PromiseFactory, IPromise<dynamic>>(promises => {
                    anim.SetBool(ANIM_ATTACK, true);
                    return promises
                        .Await(attackSpeed)
                        .Then(() => anim.SetBool(ANIM_ATTACK, false));
                });
            }
        }

        public void OnScopeInput(bool isScoping) {
            anim.SetBool(ANIM_SCOPE, isScoping);

            if (isScoping) isSprinting = false;
        }

        /*
         *
         * Animation Event Handlers
         *
         */

        public void OnStanceChanged(PlayerStance stance) {
            if (this.stance != stance) {
                this.stance = stance;
                ActivateStance();
                PublishMessage(new StanceChanged(stance));
            }
        }

        public void OnAttackModeChanged(AttackMode attackMode) {
            if (this.attackMode != attackMode) {
                this.attackMode = attackMode;
                PublishMessage(new AttackModeChanged(attackMode));
            }
        }

        public void OnMovementChanged(bool isMoving) {
            if (this.isMoving != isMoving) {
                this.isMoving = isMoving;
                PublishMessage(new MovementChanged(isMoving));
            }
        }

        public void OnScopingChanged(bool isScoping) {
            if (this.isScoping != isScoping) {
                this.isScoping = isScoping;
                PublishMessage(new ScopingChanged(isScoping));
            }
        }

        public void OnPlayerStep() { }

        /*
         *
         * Admin Event Handlers
         *
         */

        MoveConfig MoveCfg() =>
            stance switch {
                PlayerStance.CROUCHING => cfg.crouching,
                PlayerStance.CRAWLING => cfg.crawling,
                _ => isSprinting ? cfg.sprinting : cfg.standing
            };

        bool ShouldTransitionToSprint() =>
            !isSprinting && !isScoping && !PBUtils.IsAiming(attackMode);

        public void Init(PlayerCfgSO cfg) =>
            this.cfg = cfg;

        void PublishMessage(IEvent message) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(message));

        void ActivateStance() {
            stand.SetActive(stance == PlayerStance.STANDING);
            crouch.SetActive(stance == PlayerStance.CROUCHING);
            crawl.SetActive(stance == PlayerStance.CRAWLING);
        }

        static class PBUtils {
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

            public static bool IsMovable(PlayerStance stance, AttackMode mode, bool isGrounded, bool isScoping) =>
                isGrounded && (stance != PlayerStance.CRAWLING || (!IsAiming(mode) && !isScoping));

            public static bool CanAttack(AttackMode mode) =>
                mode != AttackMode.NONE
                    && mode != AttackMode.FIRING
                    && mode != AttackMode.MELEE;
        }
    }
}

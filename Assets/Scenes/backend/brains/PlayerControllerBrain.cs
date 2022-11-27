using System;
using System.Collections.Generic;
using OSCore.Events.Brains;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces;
using OSCore.Utils;
using UnityEditor;
using UnityEngine;
using static OSCore.Events.Brains.Player.AnimationEmittedEvent;
using static OSCore.Events.Brains.Player.InputEvent;

namespace OSBE.Brains {
    public class PlayerControllerBrain : IControllerBrain {
        static readonly string ANIM_STANCE = "stance";
        static readonly string ANIM_MOVE = "isMoving";
        static readonly string ANIM_SCOPE = "isScoping";
        static readonly string ANIM_AIM = "isAiming";
        static readonly string ANIM_ATTACK = "attack";

        // movement state
        PlayerCfgSO cfg;
        Transform target;
        Animator anim;
        Vector2 movement = Vector2.zero;
        Vector2 facing = Vector2.zero;
        PlayerStance stance;
        PlayerAttackMode attackMode;
        bool isMoving = false;
        bool isScoping = false;

        public PlayerControllerBrain(Transform transform) {
            cfg = AssetDatabase.LoadAssetAtPath<PlayerCfgSO>("Assets/Config/PlayerMovementCfg.asset");
            target = transform;
            anim = target.gameObject.GetComponentInChildren<Animator>();
        }

        public void Update(IGameSystem session) {
            PlayerCfgSO.MoveConfig moveCfg = stance switch {
                PlayerStance.CROUCHING => cfg.crouching,
                PlayerStance.CRAWLING => cfg.crawling,
                _ => cfg.standing
            };

            RotatePlayer(moveCfg);
            MovePlayer(moveCfg);
        }

        public void OnMessage(IEvent e) {
            switch (e) {
                case LookInput ev: Handle(ev); break;
                case MovementInput ev: Handle(ev); break;
                case StanceInput ev: Handle(ev); break;
                case AimInput ev: Handle(ev); break;
                case AttackInput ev: Handle(ev); break;
                case ScopeInput ev: Handle(ev); break;
                case StanceChanged ev: Handle(ev); break;
                case AttackModeChanged ev: Handle(ev); break;
                case MovementChanged ev: Handle(ev); break;
                case ScopingChanged ev: Handle(ev); break;
                case PlayerStep ev: Handle(ev); break;
                default: Debug.LogWarning("unhandled event " + e); break;
            }
        }

        void RotatePlayer(PlayerCfgSO.MoveConfig moveCfg) {
            float rotationZ;
            if (Vectors.NonZero(facing)) rotationZ = Vectors.AngleTo(Vector2.zero, facing);
            else if (Vectors.NonZero(movement)) rotationZ = Vectors.AngleTo(Vector2.zero, movement);
            else return;

            target.rotation = Quaternion.Lerp(
                target.rotation,
                Quaternion.Euler(0f, 0f, rotationZ),
                moveCfg.rotationSpeed * Time.deltaTime);
        }

        void MovePlayer(PlayerCfgSO.MoveConfig moveCfg) {
            if (PBUtils.IsMovable(stance, attackMode, isScoping)) {
                float speed = moveCfg.moveSpeed;

                if (PBUtils.IsAiming(attackMode)) speed *= cfg.aimFactor;
                else if (isScoping) speed *= cfg.scopeFactor;
                float movementSpeed = Mathf.Max(
                    Mathf.Abs(movement.x),
                    Mathf.Abs(movement.y));
                anim.speed = movementSpeed * speed * moveCfg.animFactor;

                if (Vectors.NonZero(movement))
                    target.position += speed * Time.deltaTime * Vectors.Upgrade(movement);
            }
        }

        /*
         *
         * Input Event Handlers
         *
         */

        void Handle(LookInput ev) =>
            facing = ev.direction;

        void Handle(MovementInput ev) {
            movement = ev.direction;
            anim.SetBool(ANIM_MOVE, Vectors.NonZero(movement));
        }

        void Handle(StanceInput ev) {
            PlayerStance nextStance = PBUtils.NextStance(
                cfg,
                stance,
                ev.holdDuration);

            if (!isMoving || PBUtils.IsMovable(nextStance, attackMode, isScoping)) {
                stance = nextStance;
                anim.SetInteger(ANIM_STANCE, (int)nextStance);
            }
        }

        void Handle(ScopeInput ev) =>
            anim.SetBool(ANIM_SCOPE, ev.isScoping);

        void Handle(AimInput ev) =>
            anim.SetBool(ANIM_AIM, ev.isAiming);

        void Handle(AttackInput ev) {
            if (ev.isAttacking && PBUtils.CanAttack(attackMode))
                anim.SetTrigger(ANIM_ATTACK);
        }

        /*
         *
         * Animation Event Handlers
         *
         */

        void Handle(StanceChanged ev) =>
            stance = ev.stance;

        void Handle(AttackModeChanged ev) =>
            attackMode = ev.mode;

        void Handle(MovementChanged ev) =>
            isMoving = ev.isMoving;

        void Handle(ScopingChanged ev) =>
            isScoping = ev.isScoping;

        void Handle(PlayerStep ev) { }

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

            public static bool IsAiming(PlayerAttackMode mode) =>
                mode == PlayerAttackMode.WEAPON || mode == PlayerAttackMode.FIRING;

            public static bool IsMovable(PlayerStance stance, PlayerAttackMode mode, bool isScoping) =>
                stance != PlayerStance.CRAWLING || (!IsAiming(mode) && !isScoping);

            public static bool CanAttack(PlayerAttackMode mode) =>
                mode != PlayerAttackMode.NONE
                    && mode != PlayerAttackMode.FIRING
                    && mode != PlayerAttackMode.PUNCHING;
        }
    }
}

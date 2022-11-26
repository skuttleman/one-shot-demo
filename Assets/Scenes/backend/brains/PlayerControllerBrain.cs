using System;
using System.Collections.Generic;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces;
using OSCore.Utils;
using UnityEditor;
using UnityEngine;
using static OSCore.Events.Brains.Player.AnimationEmittedEvent;
using static OSCore.Events.Brains.Player.InputEvent;

namespace OSBE.Brains {
    public class PlayerControllerBrain : AControllerBrain<IPlayerEvent> {
        static readonly string ANIM_MOVE = "isMoving";
        static readonly string ANIM_STANCE = "stance";
        static readonly string ANIM_AIM = "isAiming";
        static readonly string ANIM_SCOPE = "isScoping";
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
        bool isLooking = false;

        public PlayerControllerBrain(Transform transform) {
            cfg = AssetDatabase.LoadAssetAtPath<PlayerCfgSO>("Assets/Config/PlayerMovementCfg.asset");
            target = transform;
            anim = target.gameObject.GetComponentInChildren<Animator>();
        }

        public override void Update(IGameSystem session) {
            base.Update(session);
            RotatePlayer();
            MovePlayer();
        }

        internal override Action ProcessMessage(IPlayerEvent message) =>
            message switch {
                InputEvent e => () => Handle(e),
                AnimationEmittedEvent e => () => Handle(e),
                IPlayerEvent msg => () => Debug.LogError("Don't know how to process" + msg),
            };

        public void Handle(InputEvent e) {
            switch (e) {
                case LookInput ev:
                    facing = ev.direction;
                    break;

                case MovementInput ev:
                    movement = ev.direction;
                    //anim.SetBool(ANIM_MOVE, Vectors.NonZero(movement));
                    break;

                case StanceInput ev:
                    PlayerStance nextStance = PBUtils.NextStance(
                        cfg,
                        stance,
                        ev.holdDuration);

                    if (PBUtils.IsMovable(nextStance, attackMode, isScoping)) {
                        stance = nextStance;
                        //anim.SetInteger(ANIM_STANCE, (int)nextStance);
                    }
                    break;

                case AimInput ev:
                    //anim.SetBool(ANIM_AIM, ev.isAiming);
                    break;

                case AttackInput ev:
                    //if (ev.isAttacking && PBUtils.CanAttack(attackMode))
                    //    anim.SetTrigger(ANIM_ATTACK);
                    break;

                case ScopeInput ev:
                    //anim.SetBool(ANIM_SCOPE, ev.isScoping);
                    break;
            }
        }

        public void Handle(AnimationEmittedEvent e) {
            switch (e) {
                case StanceChanged ev:
                    stance = ev.stance;
                    break;

                case AttackModeChanged ev:
                    attackMode = ev.mode;
                    break;

                case MovementChanged ev:
                    isMoving = Maths.NonZero(ev.speed);
                    break;

                case ScopingChanged ev:
                    isScoping = ev.isScoping;
                    break;

                case PlayerStep:
                    break;
            }
        }

        void RotatePlayer() {
            float rotationZ;
            if (isLooking) rotationZ = Vectors.AngleTo(Vector2.zero, facing);
            else if (Vectors.NonZero(movement)) rotationZ = Vectors.AngleTo(Vector2.zero, movement);
            else return;

            //Vector3 torque = cfg.RotationTorque(
            //    stance,
            //    rb.rotation.eulerAngles.z,
            //    rotationZ);
            //if (torque != Vector3.zero)
            //    rb.AddRelativeTorque(torque);
        }

        void MovePlayer() {
            if (PBUtils.IsMovable(stance, attackMode, isScoping)) {
                //    anim.speed = cfg.AnimationSpeed(stance, movement);
                //    float force = cfg.MovementForce(
                //        stance,
                //        movement,
                //        IsAiming(),
                //        isScoping);
                //    if (Maths.NonZero(force))
                //        rb.AddForce(movement.normalized * force);
                //}
            }
        }

        public static class PBUtils {
            public static PlayerStance NextStance(PlayerCfgSO cfg, PlayerStance stance, float stanceDuration) {
                bool held = stanceDuration >= cfg.stanceChangeButtonHeldThreshold;

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

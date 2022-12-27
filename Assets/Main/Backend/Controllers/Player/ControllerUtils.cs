using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using System;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;
using static OSCore.ScriptableObjects.PlayerCfgSO;

namespace OSBE.Controllers.Player {
    public static class ControllerUtils {
        public static PlayerStance NextStance(PlayerStance stance) {
            if (stance == PlayerStance.CROUCHING)
                return PlayerStance.CRAWLING;
            return PlayerStance.CROUCHING;
        }

        public static bool IsAiming(AttackMode mode) =>
            mode == AttackMode.WEAPON || mode == AttackMode.FIRING;

        public static bool IsMovable(PlayerStance stance, PlayerControllerState state) =>
            stance != PlayerStance.CRAWLING
            || (!IsAiming(state.attackMode) && !state.isScoping);

        public static bool CanAttack(AttackMode mode) =>
            mode != AttackMode.NONE
                && mode != AttackMode.FIRING
                && mode != AttackMode.MELEE;

        public static MoveConfig MoveCfg(PlayerCfgSO cfg, PlayerControllerState state) =>
            state.stance switch {
                PlayerStance.CROUCHING => cfg.crouching,
                PlayerStance.CRAWLING => cfg.crawling,
                _ => cfg.sprinting
            };

        public static bool CanSprint(PlayerControllerState state) =>
            !state.isScoping && !IsAiming(state.attackMode);

        public static PlayerControllerState TransitionControllerState(
            PlayerAnim prev, PlayerAnim curr, PlayerControllerState state) =>
            curr switch {
                PlayerAnim.stand_idle => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.HAND,
                    isMoving = false,
                    isSprinting = false,
                },
                PlayerAnim.stand_move => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.HAND,
                    isMoving = true,
                    isSprinting = true,
                    isScoping = false,
                },
                PlayerAnim.stand_punch => state with {
                    stance = PlayerStance.STANDING,
                    attackMode = AttackMode.MELEE,
                },
                PlayerAnim.stand_fall => state with {
                    controls = PlayerInputControlMap.Standard,
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
                    isSprinting = false,
                    isScoping = true,
                },
                PlayerAnim.crouch_idle => state with {
                    controls = PlayerInputControlMap.Standard,
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.HAND,
                    isMoving = false,
                    isScoping = false,
                },
                PlayerAnim.crouch_move => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.HAND,
                    isMoving = true,
                    isSprinting = false,
                    isScoping = false,
                },
                PlayerAnim.crouch_punch => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.MELEE,
                },
                PlayerAnim.crouch_toaim => state with {
                    stance = PlayerStance.CROUCHING,
                    attackMode = AttackMode.NONE,
                    isSprinting = false,
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
                PlayerAnim.hang_lunge => state with {
                    controls = PlayerInputControlMap.None,
                    isMoving = false,
                    isSprinting = false,
                },
                PlayerAnim.hang_idle => state with {
                    controls = PlayerInputControlMap.LedgeHang,
                    isMoving = false,
                },
                PlayerAnim.hang_move => state with {
                    isMoving = true,
                },
                PlayerAnim.hang_climb => state with {
                    controls = PlayerInputControlMap.None,
                },

                _ => state,
            };

        public static PlayerAnimState TransitionAnimState(
            PlayerAnim prev, PlayerAnim curr, PlayerAnimState state) =>
            curr switch {
                PlayerAnim.stand_idle => state with {
                    stance = PlayerStance.CROUCHING,
                },
                PlayerAnim.stand_move => state with {
                    stance = PlayerStance.STANDING,
                    scope = false,
                    aim = false,
                },
                PlayerAnim.stand_punch => state with {
                    attack = false,
                },
                PlayerAnim.stand_fall => state with {
                    stance = PlayerStance.STANDING,
                },
                PlayerAnim.crouch_idle_bino => state with { },
                PlayerAnim.crouch_tobino => state with {
                    sprint = false,
                },
                PlayerAnim.crouch_idle => state with {
                    stance = PlayerStance.CROUCHING,
                },
                PlayerAnim.crouch_move => state with {
                    stance = PlayerStance.CROUCHING,
                    sprint = prev != PlayerAnim.stand_move && state.sprint
                },
                PlayerAnim.crouch_punch => state with {
                    attack = false,
                },
                PlayerAnim.crouch_toaim => state with {
                    sprint = false,
                },
                PlayerAnim.crouch_idle_aim => state with { },
                PlayerAnim.crouch_move_aim => state with { },
                PlayerAnim.crouch_fire => state with {
                    attack = false,
                },
                PlayerAnim.crawl_idle_bino => state with {
                    stance = PlayerStance.CRAWLING,
                },
                PlayerAnim.crawl_tobino => state with { },
                PlayerAnim.crawl_idle => state with {
                    stance = PlayerStance.CRAWLING,
                },
                PlayerAnim.crawl_move => state with {
                    stance = PlayerStance.CRAWLING,
                },
                PlayerAnim.crawl_punch => state with {
                    attack = false,
                },
                PlayerAnim.crawl_toaim => state with { },
                PlayerAnim.crawl_idle_aim => state with { },
                PlayerAnim.crawl_fire => state with {
                    attack = false,
                },
                PlayerAnim.hang_lunge => state with { },
                PlayerAnim.hang_idle => state with { },
                PlayerAnim.hang_move => state with { },
                PlayerAnim.hang_climb => state with {
                    climb = false,
                },

                _ => state,
            } with { attack = false };
    }
}

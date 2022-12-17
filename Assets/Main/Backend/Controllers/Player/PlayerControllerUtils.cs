using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.Data;

namespace OSBE.Controllers.Player {
    public static class PlayerControllerUtils {
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

        public static PlayerState UpdateState(this PlayerState state, PlayerAnim anim) =>
            anim switch {
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
    }
}

using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.System;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/animator")]
    public class PlayerAnimationCfgSO : ACharacterAnimatorCfgSO<PlayerAnim, PlayerAnimState> {
        [field: Header("Transition speeds")]
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float scopingSpeed { get; private set; }
        [field: SerializeField] public float punchingSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }
        [field: SerializeField] public float landingSpeed { get; private set; }
        [field: SerializeField] public float lungingSpeed { get; private set; }
        [field: SerializeField] public float ledgeShimmySpeed { get; private set; }

        private AnimNode<PlayerAnim, PlayerAnimState> BuildAsset() {
            AnimNode<PlayerAnim, PlayerAnimState> stand_idle = new(PlayerAnim.stand_idle);
            AnimNode<PlayerAnim, PlayerAnimState> stand_move = new(PlayerAnim.stand_move);
            AnimNode<PlayerAnim, PlayerAnimState> stand_punch = new(PlayerAnim.stand_punch);
            AnimNode<PlayerAnim, PlayerAnimState> stand_fall = new(PlayerAnim.stand_fall);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_idle_bino = new(PlayerAnim.crouch_idle_bino);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_move_bino = new(PlayerAnim.crouch_move_bino);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_tobino = new(PlayerAnim.crouch_tobino);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_idle = new(PlayerAnim.crouch_idle);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_move = new(PlayerAnim.crouch_move);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_punch = new(PlayerAnim.crouch_punch);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_toaim = new(PlayerAnim.crouch_toaim);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_idle_aim = new(PlayerAnim.crouch_idle_aim);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_move_aim = new(PlayerAnim.crouch_move_aim);
            AnimNode<PlayerAnim, PlayerAnimState> crouch_fire = new(PlayerAnim.crouch_fire);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_idle_bino = new(PlayerAnim.crawl_idle_bino);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_tobino = new(PlayerAnim.crawl_tobino);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_idle = new(PlayerAnim.crawl_idle);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_move = new(PlayerAnim.crawl_move);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_punch = new(PlayerAnim.crawl_punch);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_toaim = new(PlayerAnim.crawl_toaim);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_idle_aim = new(PlayerAnim.crawl_idle_aim);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_fire = new(PlayerAnim.crawl_fire);
            AnimNode<PlayerAnim, PlayerAnimState> crawl_dive = new(PlayerAnim.crawl_dive);
            AnimNode<PlayerAnim, PlayerAnimState> hang_lunge = new(PlayerAnim.hang_lunge, lungingSpeed);
            AnimNode<PlayerAnim, PlayerAnimState> hang_idle = new(PlayerAnim.hang_idle);
            AnimNode<PlayerAnim, PlayerAnimState> hang_move = new(PlayerAnim.hang_move, ledgeShimmySpeed);
            AnimNode<PlayerAnim, PlayerAnimState> hang_climb = new(PlayerAnim.hang_climb);


            stand_idle
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.sprint && state.move, stand_move)
                .To(state => !state.move || !state.sprint || state.stance != PlayerStance.STANDING,
                    defaultSpeed,
                    0f,
                    crouch_idle);
            stand_move
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.stance != PlayerStance.STANDING || !state.sprint, defaultSpeed, 0f, crouch_move)
                .To(state => !state.move, stand_idle)
                .To(state => state.scope, crouch_tobino)
                .To(state => state.aim, crouch_toaim)
                .To(state => state.attack, stand_punch);
            stand_punch
                .To(state => state.fall, stand_fall)
                .To(state => state.move, punchingSpeed, 0f, stand_move)
                .To(state => !state.move, punchingSpeed, 0f, stand_idle);
            stand_fall
                .To(state => state.hang, hang_lunge)
                .To(state => !state.fall && state.sprint, stand_move)
                .To(state => !state.fall && !state.sprint, stand_idle);


            crouch_idle_bino
                .To(state => state.fall, stand_fall)
                .To(state => state.stance == PlayerStance.STANDING, defaultSpeed, 0f, crouch_tobino)
                .To(state => state.stance == PlayerStance.CRAWLING, defaultSpeed, 0f, crawl_idle_bino)
                .To(state => !state.scope, crouch_tobino)
                .To(state => state.move, crouch_move_bino);
            crouch_move_bino
                .To(state => state.fall, stand_fall)
                .To(state => state.sprint, crouch_tobino)
                .To(state => state.stance == PlayerStance.STANDING, defaultSpeed, 0f, crouch_tobino)
                .To(state => !state.scope, crouch_tobino)
                .To(state => state.move, crouch_idle_bino);
            crouch_tobino
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.move && state.sprint, scopingSpeed, 0f, stand_move)
                .To(state => state.move && state.scope, scopingSpeed, 0f, crouch_move_bino)
                .To(state => state.move && !state.scope, scopingSpeed, 0f, crouch_move)
                .To(state => !state.move && state.scope, scopingSpeed, 0f, crouch_idle_bino)
                .To(state => !state.move && !state.scope, scopingSpeed, 0f, crouch_idle);
            crouch_idle
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.stance == PlayerStance.CRAWLING, defaultSpeed, 0f, crawl_idle)
                .To(state => state.move, crouch_move)
                .To(state => state.scope, crouch_tobino)
                .To(state => state.aim, crouch_toaim)
                .To(state => state.attack, crouch_punch);
            crouch_move
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.sprint, defaultSpeed, 0.5f, stand_move)
                .To(state => state.stance == PlayerStance.CRAWLING, defaultSpeed, 0f, crawl_move)
                .To(state => !state.move, crouch_idle)
                .To(state => state.scope, crouch_tobino)
                .To(state => state.aim, crouch_toaim)
                .To(state => state.attack, crouch_punch);
            crouch_punch
                .To(state => state.fall, stand_fall)
                .To(state => state.move, punchingSpeed, 0f, crouch_move)
                .To(state => !state.move, punchingSpeed, 0f, crouch_idle);
            crouch_toaim
                .To(state => state.fall, stand_fall)
                .To(state => state.dive, crawl_dive)
                .To(state => state.move && state.sprint, aimingSpeed, 0f, stand_move)
                .To(state => state.move && state.aim, aimingSpeed, 0f, crouch_move_aim)
                .To(state => state.move && !state.aim, aimingSpeed, 0f, crouch_move)
                .To(state => !state.move && state.aim, aimingSpeed, 0f, crouch_idle_aim)
                .To(state => !state.move && !state.aim, aimingSpeed, 0f, crouch_idle);
            crouch_idle_aim
                .To(state => state.fall, stand_fall)
                .To(state => state.stance == PlayerStance.CRAWLING, defaultSpeed, 0f, crawl_idle_aim)
                .To(state => state.move, crouch_move_aim)
                .To(state => !state.aim, crouch_toaim)
                .To(state => state.attack, crouch_fire);
            crouch_move_aim
                .To(state => state.fall, stand_fall)
                .To(state => state.sprint, crouch_toaim)
                .To(state => !state.move, crouch_idle_aim)
                .To(state => !state.aim, crouch_toaim)
                .To(state => state.attack, crouch_fire);
            crouch_fire
                .To(state => state.fall, stand_fall)
                .To(state => state.move, firingSpeed, 0f, crouch_move_aim)
                .To(state => !state.move, firingSpeed, 0f, crouch_idle_aim);


            crawl_idle_bino
                .To(state => state.fall, stand_fall)
                .To(state => state.stance != PlayerStance.CRAWLING, defaultSpeed, 0f, crouch_idle_bino)
                .To(state => !state.scope || state.move, crawl_tobino);
            crawl_tobino
                .To(state => state.fall, stand_fall)
                .To(state => state.move, scopingSpeed, 0f, crawl_move)
                .To(state => state.scope, scopingSpeed, 0f, crawl_idle_bino)
                .To(state => !state.scope, scopingSpeed, 0f, crawl_idle);
            crawl_idle
                .To(state => state.fall, stand_fall)
                .To(state => state.stance != PlayerStance.CRAWLING, defaultSpeed, 0f, crouch_idle)
                .To(state => state.move, crawl_move)
                .To(state => state.scope, crawl_tobino)
                .To(state => state.aim, crawl_toaim)
                .To(state => state.attack, crawl_punch);
            crawl_move
                .To(state => state.fall, stand_fall)
                .To(state => state.sprint, defaultSpeed, 0.5f, crouch_move)
                .To(state => state.stance != PlayerStance.CRAWLING, defaultSpeed, 0f, crouch_move)
                .To(state => !state.move, crawl_idle);
            crawl_punch
                .To(state => state.fall, stand_fall)
                .To(state => state.move, punchingSpeed, 0f, crawl_move)
                .To(state => !state.move, punchingSpeed, 0f, crawl_idle);
            crawl_toaim
                .To(state => state.fall, stand_fall)
                .To(state => state.move, aimingSpeed, 0f, crawl_move)
                .To(state => state.aim, aimingSpeed, 0f, crawl_idle_aim)
                .To(state => !state.aim, aimingSpeed, 0f, crawl_idle);
            crawl_idle_aim
                .To(state => state.fall, stand_fall)
                .To(state => state.stance != PlayerStance.CRAWLING, defaultSpeed, 0f, crouch_idle_aim)
                .To(state => !state.aim || state.move, crawl_toaim)
                .To(state => state.attack, crawl_fire);
            crawl_fire
                .To(state => state.fall, stand_fall)
                .To(state => !state.attack, firingSpeed, 0f, crawl_idle_aim);
            crawl_dive
                .To(state => state.fall, 0.5f, 0f, stand_fall)
                .To(state => !state.fall, crawl_idle);


            hang_lunge
                .To(state => state.hang, 0f, 1f, hang_idle);
            hang_idle
                .To(state => state.fall, stand_fall)
                .To(state => state.climb, hang_climb)
                .To(state => state.move, hang_move);
            hang_move
                .To(state => state.fall, stand_fall)
                .To(state => state.climb, hang_climb)
                .To(state => !state.move, ledgeShimmySpeed, 0f, hang_idle);
            hang_climb
                .To(state => state.climb, 0f, 1f, crouch_idle);

            return crouch_idle;
        }

        public override AnimNode<PlayerAnim, PlayerAnimState> Init() {
            return BuildAsset();
        }

        public record PlayerAnimState : AnimStateDetails<PlayerAnim> {
            public PlayerStance stance { get; init; }
            public bool fall { get; init; }
            public bool sprint { get; init; }
            public bool dive { get; init; }
            public bool attack { get; init; }
            public bool move { get; init; }
            public bool hang { get; init; }
            public bool climb { get; init; }
            public bool scope { get; init; }
            public bool aim { get; init; }
        }
    }
}

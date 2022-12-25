using OSCore.Data.Animations;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/animator")]
    public class PlayerAnimationCfgSO : ACharacterAnimatorCfgSOOld<PlayerAnim, PlayerAnimSignal> {
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float scopingSpeed { get; private set; }
        [field: SerializeField] public float punchingSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }
        [field: SerializeField] public float landingSpeed { get; private set; }
        [field: SerializeField] public float lungingSpeed { get; private set; }
        [field: SerializeField] public float ledgeShimmySpeed { get; private set; }

        public override AStateNodeOld<PlayerAnim, PlayerAnimSignal> Init() {
            StableNodeOld<PlayerAnim, PlayerAnimSignal> stand_move = new(PlayerAnim.stand_move);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> stand_fall = new(PlayerAnim.stand_fall);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_idle_bino = new(PlayerAnim.crouch_idle_bino);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_move_bino = new(PlayerAnim.crouch_move_bino);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_idle = new(PlayerAnim.crouch_idle);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_move = new(PlayerAnim.crouch_move);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_idle_aim = new(PlayerAnim.crouch_idle_aim);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crouch_move_aim = new(PlayerAnim.crouch_move_aim);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crawl_idle_bino = new(PlayerAnim.crawl_idle_bino);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crawl_idle = new(PlayerAnim.crawl_idle);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crawl_move = new(PlayerAnim.crawl_move);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> crawl_idle_aim = new(PlayerAnim.crawl_idle_aim);
            StableNodeOld<PlayerAnim, PlayerAnimSignal> hang_idle = new(PlayerAnim.hang_idle);

            stand_move
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_move)
                .To(PlayerAnimSignal.LOOK, crouch_move)
                .To(PlayerAnimSignal.MOVE_OFF, crouch_idle, (PlayerAnim.stand_idle, defaultSpeed))
                .To(PlayerAnimSignal.SCOPE_ON, crouch_move_bino, (PlayerAnim.crouch_tobino, scopingSpeed))
                .To(PlayerAnimSignal.AIM_ON, crouch_move_aim, (PlayerAnim.crouch_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.stand_punch, punchingSpeed);
            stand_fall
                .To(PlayerAnimSignal.LAND_SPRINT, stand_move)
                .To(PlayerAnimSignal.FALLING_LUNGE, hang_idle, (PlayerAnim.hang_lunge, 0f, 1f, lungingSpeed))
                .To(PlayerAnimSignal.LAND_MOVE, crouch_move, (PlayerAnim.stand_idle, landingSpeed))
                .To(PlayerAnimSignal.LAND_IDLE, crouch_idle, (PlayerAnim.stand_idle, landingSpeed));

            crouch_idle_bino
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle_bino)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move_bino)
                .To(PlayerAnimSignal.SCOPE_OFF, crouch_idle, (PlayerAnim.crouch_tobino, scopingSpeed));
            crouch_move_bino
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.MOVE_OFF, crouch_idle_bino)
                .To(PlayerAnimSignal.SCOPE_OFF, crouch_move, (PlayerAnim.crouch_tobino, scopingSpeed));
            crouch_idle
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move)
                .To(PlayerAnimSignal.SCOPE_ON, crouch_idle_bino, (PlayerAnim.crouch_tobino, scopingSpeed))
                .To(PlayerAnimSignal.AIM_ON, crouch_idle_aim, (PlayerAnim.crouch_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_punch, punchingSpeed);
            crouch_move
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_move)
                .To(PlayerAnimSignal.MOVE_OFF, crouch_idle)
                .To(PlayerAnimSignal.SPRINT, stand_move)
                .To(PlayerAnimSignal.SCOPE_ON, crouch_move_bino, (PlayerAnim.crouch_tobino, scopingSpeed))
                .To(PlayerAnimSignal.AIM_ON, crouch_move_aim, (PlayerAnim.crouch_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_punch, punchingSpeed);
            crouch_idle_aim
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle_aim)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move_aim)
                .To(PlayerAnimSignal.AIM_OFF, crouch_idle, (PlayerAnim.crouch_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_fire, firingSpeed);
            crouch_move_aim
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, stand_move)
                .To(PlayerAnimSignal.MOVE_ON, crouch_idle_aim)
                .To(PlayerAnimSignal.AIM_OFF, crouch_move, (PlayerAnim.crouch_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_fire, firingSpeed);

            crawl_idle_bino
                .To(PlayerAnimSignal.FALLING, stand_fall, (PlayerAnim.crouch_idle, defaultSpeed))
                .To(PlayerAnimSignal.STANCE, crouch_idle_bino)
                .To(PlayerAnimSignal.SCOPE_OFF, crawl_idle, (PlayerAnim.crawl_tobino, scopingSpeed));
            crawl_idle
                .To(PlayerAnimSignal.FALLING, stand_fall, (PlayerAnim.crouch_idle, defaultSpeed))
                .To(PlayerAnimSignal.STANCE, crouch_idle)
                .To(PlayerAnimSignal.MOVE_ON, crawl_move)
                .To(PlayerAnimSignal.SCOPE_ON, crawl_idle_bino, (PlayerAnim.crawl_tobino, scopingSpeed))
                .To(PlayerAnimSignal.AIM_ON, crawl_idle_aim, (PlayerAnim.crawl_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crawl_punch, punchingSpeed);
            crawl_move
                .To(PlayerAnimSignal.FALLING, stand_fall, (PlayerAnim.crouch_idle, defaultSpeed))
                .To(PlayerAnimSignal.STANCE, crouch_move)
                .To(PlayerAnimSignal.SPRINT, stand_move, (PlayerAnim.crouch_move, defaultSpeed))
                .To(PlayerAnimSignal.MOVE_OFF, crawl_idle);
            crawl_idle_aim
                .To(PlayerAnimSignal.FALLING, stand_fall, (PlayerAnim.crouch_idle, defaultSpeed))
                .To(PlayerAnimSignal.STANCE, crouch_idle_aim)
                .To(PlayerAnimSignal.AIM_OFF, crawl_idle, (PlayerAnim.crawl_toaim, aimingSpeed))
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crawl_fire, firingSpeed);

            hang_idle
                .With(PlayerAnimSignal.MOVE_ON, PlayerAnim.hang_move, 0.4f, 1f, ledgeShimmySpeed)
                .To(PlayerAnimSignal.LEDGE_CLIMB, crouch_idle, (PlayerAnim.hang_climb, defaultSpeed))
                .To(PlayerAnimSignal.LEDGE_DROP, stand_fall);

            return crouch_idle;
        }
    }
}

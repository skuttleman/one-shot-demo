using OSCore.Data.Animations;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/animator")]
    public class PlayerAnimationCfgSO : ACharacterAnimatorCfgSO<PlayerAnim, PlayerAnimSignal> {
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float scopingSpeed { get; private set; }
        [field: SerializeField] public float punchingSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }
        [field: SerializeField] public float landingSpeed { get; private set; }

        public override AStateNode<PlayerAnim, PlayerAnimSignal> Init() {
            StableNode<PlayerAnim, PlayerAnimSignal> stand_move = new(PlayerAnim.stand_move);
            StableNode<PlayerAnim, PlayerAnimSignal> stand_fall = new(PlayerAnim.stand_fall);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_idle_bino = new(PlayerAnim.crouch_idle_bino);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_move_bino = new(PlayerAnim.crouch_move_bino);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_idle = new(PlayerAnim.crouch_idle);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_move = new(PlayerAnim.crouch_move);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_idle_aim = new(PlayerAnim.crouch_idle_aim);
            StableNode<PlayerAnim, PlayerAnimSignal> crouch_move_aim = new(PlayerAnim.crouch_move_aim);
            StableNode<PlayerAnim, PlayerAnimSignal> crawl_idle_bino = new(PlayerAnim.crawl_idle_bino);
            StableNode<PlayerAnim, PlayerAnimSignal> crawl_idle = new(PlayerAnim.crawl_idle);
            StableNode<PlayerAnim, PlayerAnimSignal> crawl_move = new(PlayerAnim.crawl_move);
            StableNode<PlayerAnim, PlayerAnimSignal> crawl_idle_aim = new(PlayerAnim.crawl_idle_aim);

            stand_move
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_move)
                .Through(PlayerAnimSignal.MOVE_OFF, PlayerAnim.stand_idle, defaultSpeed, crouch_idle)
                .Through(PlayerAnimSignal.SCOPE_ON, PlayerAnim.crouch_tobino, scopingSpeed, crouch_move_bino)
                .Through(PlayerAnimSignal.AIM_ON, PlayerAnim.crouch_toaim, aimingSpeed, crouch_move_aim)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.stand_punch, punchingSpeed);
            stand_fall
                .To(PlayerAnimSignal.LAND_SPRINT, stand_move)
                .Through(PlayerAnimSignal.LAND_MOVE, PlayerAnim.stand_idle, landingSpeed, crouch_move)
                .Through(PlayerAnimSignal.LAND_IDLE, PlayerAnim.stand_idle, landingSpeed, crouch_idle);

            crouch_idle_bino
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle_bino)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move_bino)
                .Through(PlayerAnimSignal.SCOPE_OFF, PlayerAnim.crouch_tobino, scopingSpeed, crouch_idle);
            crouch_move_bino
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.MOVE_OFF, crouch_idle_bino)
                .Through(PlayerAnimSignal.SCOPE_OFF, PlayerAnim.crouch_tobino, scopingSpeed, crouch_idle);
            crouch_idle
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move)
                .Through(PlayerAnimSignal.SCOPE_ON, PlayerAnim.crouch_tobino, scopingSpeed, crouch_idle_bino)
                .Through(PlayerAnimSignal.AIM_ON, PlayerAnim.crouch_toaim, aimingSpeed, crouch_idle_aim)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_punch, punchingSpeed);
            crouch_move
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_move)
                .To(PlayerAnimSignal.MOVE_OFF, crouch_idle)
                .To(PlayerAnimSignal.SPRINT, stand_move)
                .Through(PlayerAnimSignal.SCOPE_ON, PlayerAnim.crouch_tobino, scopingSpeed, crouch_move_bino)
                .Through(PlayerAnimSignal.AIM_ON, PlayerAnim.crouch_toaim, aimingSpeed, crouch_move_aim)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_punch, punchingSpeed);
            crouch_idle_aim
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crawl_idle_aim)
                .To(PlayerAnimSignal.MOVE_ON, crouch_move_aim)
                .Through(PlayerAnimSignal.AIM_OFF, PlayerAnim.crouch_toaim, aimingSpeed, crouch_idle)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_fire, firingSpeed);
            crouch_move_aim
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, stand_move)
                .To(PlayerAnimSignal.MOVE_ON, crouch_idle_aim)
                .Through(PlayerAnimSignal.AIM_OFF, PlayerAnim.crouch_toaim, aimingSpeed, crouch_idle)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crouch_fire, firingSpeed);


            crawl_idle_bino
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_idle_bino)
                .Through(PlayerAnimSignal.SCOPE_OFF, PlayerAnim.crawl_tobino, scopingSpeed, crawl_idle);
            crawl_idle
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_idle)
                .To(PlayerAnimSignal.MOVE_ON, crawl_move)
                .Through(PlayerAnimSignal.SCOPE_ON, PlayerAnim.crawl_tobino, scopingSpeed, crawl_idle_bino)
                .Through(PlayerAnimSignal.AIM_ON, PlayerAnim.crawl_toaim, aimingSpeed, crawl_idle_aim)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crawl_punch, punchingSpeed);
            crawl_move
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_move)
                .Through(PlayerAnimSignal.SPRINT, PlayerAnim.crouch_move, defaultSpeed, stand_move)
                .To(PlayerAnimSignal.MOVE_OFF, crawl_idle);
            crawl_idle_aim
                .To(PlayerAnimSignal.FALLING, stand_fall)
                .To(PlayerAnimSignal.STANCE, crouch_idle_aim)
                .Through(PlayerAnimSignal.AIM_OFF, PlayerAnim.crawl_toaim, aimingSpeed, crawl_idle)
                .With(PlayerAnimSignal.ATTACK, PlayerAnim.crawl_fire, firingSpeed);

            return crouch_idle;
        }
    }
}

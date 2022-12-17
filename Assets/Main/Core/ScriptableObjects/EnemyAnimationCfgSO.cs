using OSCore.Data.Animations;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/animator")]
    public class EnemyAnimationCfgSO : ACharacterAnimatorCfgSO<EnemyAnim, EnemyAnimSignal> {
        [field: Header("Transition speeds")]
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float meleeSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }

        public override AStateNode<EnemyAnim, EnemyAnimSignal> Init() {
            StableNode<EnemyAnim, EnemyAnimSignal> stand_idle = new(EnemyAnim.stand_idle);
            StableNode<EnemyAnim, EnemyAnimSignal> stand_move = new(EnemyAnim.stand_move);
            StableNode<EnemyAnim, EnemyAnimSignal> stand_idle_aim = new(EnemyAnim.stand_idle_aim);

            stand_idle
                .To(EnemyAnimSignal.MOVE_ON, stand_move)
                .Through(EnemyAnimSignal.AIM_ON, EnemyAnim.stand_toaim, aimingSpeed, stand_idle_aim)
                .With(EnemyAnimSignal.ATTACK, EnemyAnim.stand_melee, meleeSpeed);
            stand_move
                .Through(EnemyAnimSignal.MOVE_OFF, EnemyAnim.stand_idle, defaultSpeed, stand_idle);
            stand_idle_aim
                .Through(EnemyAnimSignal.AIM_OFF, EnemyAnim.stand_toaim, aimingSpeed, stand_idle)
                .With(EnemyAnimSignal.ATTACK, EnemyAnim.stand_fire, firingSpeed);

            return stand_idle;
        }
    }
}

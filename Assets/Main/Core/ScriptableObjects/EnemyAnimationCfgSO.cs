using OSCore.Data.Animations;
using OSCore.System;
using OSCore.System.Interfaces;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/animator")]
    public class EnemyAnimationCfgSO : ACharacterAnimatorCfgSO<EnemyAnim> {
        [field: Header("Transition speeds")]
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float meleeSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }

        public override AStateNode<EnemyAnim> Init() {
            StableNode<EnemyAnim> stand_idle = new(EnemyAnim.stand_idle);
            StableNode<EnemyAnim> stand_move = new(EnemyAnim.stand_move);
            StableNode<EnemyAnim> stand_idle_aim = new(EnemyAnim.stand_idle_aim);

            stand_idle
                .To<EnemyAnim, EnemyAnimState>(state => state.isMoving, stand_move)
                .To<EnemyAnim, EnemyAnimState>(state => state.isAiming, stand_idle_aim, (EnemyAnim.stand_toaim, aimingSpeed))
                .With<EnemyAnim, EnemyAnimState>(state => state.isAttacking, EnemyAnim.stand_melee, meleeSpeed);
            stand_move
                .To<EnemyAnim, EnemyAnimState>(state => !state.isMoving, stand_idle);
            stand_idle_aim
                .To<EnemyAnim, EnemyAnimState>(state => !state.isAiming, stand_idle, (EnemyAnim.stand_toaim, aimingSpeed))
                .With<EnemyAnim, EnemyAnimState>(state => state.isAttacking, EnemyAnim.stand_fire, firingSpeed);

            return stand_idle;
        }

        public record EnemyAnimState : IStateDetails<EnemyAnim> {
            public EnemyAnim state { get; init; }
            public bool isMoving = false;
            public bool isAiming = false;
            public bool isAttacking = false;
        }
    }
}

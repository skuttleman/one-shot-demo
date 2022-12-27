using OSCore.Data.Animations;
using OSCore.System;
using UnityEngine;
using static OSCore.ScriptableObjects.EnemyAnimationCfgSO;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/animator")]
    public class EnemyAnimationCfgSO : ACharacterAnimatorCfgSO<EnemyAnim, EnemyAnimState> {
        [field: Header("Transition speeds")]
        [field: SerializeField] public float defaultSpeed { get; private set; }
        [field: SerializeField] public float aimingSpeed { get; private set; }
        [field: SerializeField] public float meleeSpeed { get; private set; }
        [field: SerializeField] public float firingSpeed { get; private set; }

        public override AnimNode<EnemyAnim, EnemyAnimState> Init() {
            AnimNode<EnemyAnim, EnemyAnimState> stand_idle = new(EnemyAnim.stand_idle);
            AnimNode<EnemyAnim, EnemyAnimState> stand_move = new(EnemyAnim.stand_move);
            AnimNode<EnemyAnim, EnemyAnimState> stand_idle_aim = new(EnemyAnim.stand_idle_aim);

            stand_idle.To(state => state.isMoving, stand_move);
            stand_move.To(state => !state.isMoving, stand_idle);

            return stand_idle;
        }

        public record EnemyAnimState : AnimStateDetails<EnemyAnim> {
            public bool isMoving { get; init; }
            public bool isAiming { get; init; }
            public bool isAttacking { get; init; }
        }
    }
}

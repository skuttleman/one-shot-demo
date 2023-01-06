using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.ScriptableObjects.EnemyAnimationCfgSO;

namespace OSBE.Controllers {
    public class EnemyAnimator : ACharacterAnimator<EnemyAnim, EnemyAnimState> {
        [SerializeField] private EnemyAnimationCfgSO cfg;

        private void Start() {
            IStateReceiver<EnemyAnim> receiver = Transforms
                .Body(transform)
                .GetComponentInChildren<IStateReceiver<EnemyAnim>>();
            Init(cfg.animator, receiver, cfg.Init(), new() {
                isMoving = false,
                isAiming = false,
                isAttacking = false,
            });
        }
    }
}

using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using UnityEngine;

namespace OSBE.Controllers {
    public class EnemyAnimator : ACharacterAnimator<EnemyAnim, EnemyAnimSignal> {
        [SerializeField] private EnemyAnimationCfgSO cfg;

        public void Init(IStateReceiver<EnemyAnim> receiver) {
            Init(receiver, cfg.Init());
        }
    }
}

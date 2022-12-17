using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers {
    public class EnemyAnimator : ACharacterAnimator<EnemyAnim, EnemyAnimSignal> {
        [SerializeField] private EnemyAnimationCfgSO cfg;

        private void Start() {
            IStateReceiver<EnemyAnim> receiver = Transforms
                .Entity(transform)
                .GetComponentInChildren<IStateReceiver<EnemyAnim>>();
            Init(receiver, cfg.Init());
        }
    }
}

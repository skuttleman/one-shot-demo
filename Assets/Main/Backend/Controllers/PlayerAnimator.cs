using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers {
    public class PlayerAnimator : ACharacterAnimator<PlayerAnim, PlayerAnimSignal> {
        [SerializeField] private PlayerAnimationCfgSO cfg;

        private void Start() {
            IStateReceiver<PlayerAnim> receiver = Transforms
                .Entity(transform)
                .GetComponentInChildren<IStateReceiver<PlayerAnim>>();
            Init(receiver, cfg.Init());
        }
    }
}

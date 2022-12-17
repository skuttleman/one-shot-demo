using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using UnityEngine;

namespace OSBE.Controllers {
    public class PlayerAnimator : ACharacterAnimator<PlayerAnim, PlayerAnimSignal> {
        [SerializeField] private PlayerAnimationCfgSO cfg;

        public void Init(IStateReceiver<PlayerAnim> receiver) {
            Init(receiver, cfg.Init());
        }
    }
}

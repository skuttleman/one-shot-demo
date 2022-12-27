using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;

namespace OSBE.Controllers {
    public class PlayerAnimator : ACharacterAnimator<PlayerAnim, PlayerAnimState> {
        [SerializeField] private PlayerAnimationCfgSO cfg;

        private void Start() {
            IStateReceiver<PlayerAnim> receiver = Transforms
                .Entity(transform)
                .GetComponentInChildren<IStateReceiver<PlayerAnim>>();
            Init(receiver, cfg.Init(), new() {
                state = PlayerAnim.crouch_idle,
                fall = false,
                attack = false,
                aim = false,
                scope = false,
                move = false,
                sprint = false,
                hang = false,
                climb = false,
            });
        }
    }
}

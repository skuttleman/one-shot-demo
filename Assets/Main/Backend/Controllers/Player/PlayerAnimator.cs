using Cinemachine;
using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;

namespace OSBE.Controllers.Player {
    public class PlayerAnimator : ACharacterAnimator<PlayerAnim, PlayerAnimState> {
        [SerializeField] private PlayerAnimationCfgSO cfg;

        private void Start() {
            IStateReceiver<PlayerAnim> receiver = Transforms
                .Body(transform)
                .GetComponentInChildren<IStateReceiver<PlayerAnim>>();
            Init(cfg.animator, receiver, cfg.Init(), new() {
                fall = false,
                attack = false,
                aim = false,
                scope = false,
                move = false,
                sprint = false,
                hang = false,
                climb = false,
            });

            FindObjectOfType<CinemachineStateDrivenCamera>()
                .m_AnimatedTarget = GetComponent<Animator>();
        }
    }
}

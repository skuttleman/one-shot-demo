using OSCore.Data.Enums;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;

namespace OSFE.Characters.Player {
    public class PlayerAnimationListener : MonoBehaviour {
        IGameSystem system;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
        }

        public void OnStanceChange(PlayerStance stance) =>
                Brain().OnStanceChanged(stance);

        public void OnAttackMode(AttackMode mode) =>
            Brain().OnAttackModeChanged(mode);

        public void OnMovement(int moving) =>
            Brain().OnMovementChanged(moving != 0);

        public void OnScope(int enabled) =>
            Brain().OnScopingChanged(enabled != 0);

        public void OnStep() =>
            Brain().OnPlayerStep();

        IPlayerStateReducer Brain() =>
            system.Send<IControllerManager, IPlayerStateReducer>(mngr =>
                mngr.Ensure<IPlayerStateReducer>(transform.parent));
    }
}

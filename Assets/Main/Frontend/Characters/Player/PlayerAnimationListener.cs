using OSCore.Data.Enums;
using OSCore.System.Interfaces.Brains;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Characters.Player {
    public class PlayerAnimationListener : MonoBehaviour {
        IPlayerController controller;

        public void OnStanceChange(PlayerStance stance) =>
            controller.OnStanceChanged(stance);

        public void OnAttackMode(AttackMode mode) =>
            controller.OnAttackModeChanged(mode);

        public void OnMovement(int moving) =>
            controller.OnMovementChanged(moving != 0);

        public void OnScope(int enabled) =>
            controller.OnScopingChanged(enabled != 0);

        public void OnStep() =>
            controller.OnPlayerStep();

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IPlayerController>();
        }
    }
}

using OSCore.Data.Enums;
using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Characters.Enemy {
    public class EnemyAnimationListener : MonoBehaviour {
        private IEnemyController controller;

        public void OnAttackMode(AttackMode mode) =>
            controller.OnAttackModeChanged(mode);

        public void OnMovement(int moving) =>
            controller.OnMovementChanged(moving != 0);

        public void OnStep() =>
            controller.OnEnemyStep();

        private void Start() {
            controller = Transforms.Entity(transform).GetComponent<IEnemyController>();
        }
    }
}

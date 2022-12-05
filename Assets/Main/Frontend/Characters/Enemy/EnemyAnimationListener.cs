using OSCore.Data.Enums;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore;
using UnityEngine;

namespace OSFE.Characters.Enemy {
    public class EnemyAnimationListener : MonoBehaviour {
        IGameSystem system;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
        }

        public void OnAttackMode(AttackMode mode) =>
            Brain().OnAttackModeChanged(mode);

        public void OnMovement(int moving) =>
            Brain().OnMovementChanged(moving != 0);

        public void OnStep() =>
            Brain().OnEnemyStep();

        IEnemyController Brain() =>
            system.Send<IControllerManager, IEnemyController>(mngr =>
                mngr.Ensure<IEnemyController>(transform.root));
    }
}

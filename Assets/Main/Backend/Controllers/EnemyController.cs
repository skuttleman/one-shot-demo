using OSCore.Data.Enums;
using OSCore.System.Interfaces;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

namespace OSBE.Controllers {
    public class EnemyController : IEnemyController {
        readonly IGameSystem system;
        readonly Transform target;

        public EnemyController(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
        }

        public void OnAttackModeChanged(AttackMode attackMode) { }

        public void OnEnemyStep() { }

        public void OnMovementChanged(bool isMoving) { }
    }
}

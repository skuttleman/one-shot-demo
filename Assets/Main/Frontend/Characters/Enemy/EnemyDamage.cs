using OSCore.System.Interfaces.Brains;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Characters.Enemy {
    public class EnemyDamage : MonoBehaviour, IDamage {
        private IEnemyController controller;

        public void OnAttack(float damage) =>
            controller.OnDamage(damage);

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IEnemyController>();
        }
    }
}

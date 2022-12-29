using OSCore.Data.Controllers;
using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSFE.Scripts {
    public class EnemyDamage : MonoBehaviour, IDamage {
        private IController<EnemyControllerInput> controller;

        public void OnAttack(float damage) {
            controller.On(new DamageInput(damage));
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            controller = Transforms.Body(transform)
                .GetComponent<IController<EnemyControllerInput>>();
        }
    }
}

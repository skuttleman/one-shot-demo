using OSCore.Data.Controllers;
using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Scripts {
    public class EnemyAnimationListener : MonoBehaviour {
        private IController<EnemyControllerInput> controller;

        public void OnStep() {
            controller.OnStep();
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

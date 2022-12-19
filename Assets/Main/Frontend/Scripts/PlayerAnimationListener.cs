using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine;

namespace OSFE.Scripts {
    public class PlayerAnimationListener : MonoBehaviour {
        IPlayerController controller;

        public void OnStep() =>
            controller.OnPlayerStep();

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IPlayerController>();
        }
    }
}

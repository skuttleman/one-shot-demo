using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OSFE.Scripts {
    public class PlayerInputListener : MonoBehaviour {
        private IPlayerController controller;

        public void OnMove(InputValue value) {
            controller.OnMovementInput(value.Get<Vector2>());
        }

        public void OnRun(InputValue value) =>
            controller.OnSprintInput(value.isPressed);

        public void OnLook(InputValue value) =>
            controller.OnLookInput(value.Get<Vector2>(), false);

        public void OnMouseLook(InputValue value) =>
            controller.OnLookInput(value.Get<Vector2>(), true);

        public void OnStance(InputValue _) =>
            controller.OnStanceInput();

        public void OnScope(InputValue value) =>
            controller.OnScopeInput(Maths.NonZero(value.Get<float>()));

        public void OnAim(InputValue value) =>
            controller.OnAimInput(Maths.NonZero(value.Get<float>()));

        public void OnAttack(InputValue value) =>
            controller.OnAttackInput(value.isPressed);

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IPlayerController>();
        }
    }
}

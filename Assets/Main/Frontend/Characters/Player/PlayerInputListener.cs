using OSCore.System.Interfaces.Brains;
using OSCore.Utils;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OSFE.Characters.Player {
    public class PlayerInputListener : MonoBehaviour {
        private IPlayerController controller;

        public void OnInputMove(InputValue value) {
            controller.OnMovementInput(value.Get<Vector2>());
        }

        public void OnInputRun(InputValue value) =>
            controller.OnSprintInput(value.isPressed);

        public void OnInputLook(InputValue value) =>
            controller.OnLookInput(value.Get<Vector2>(), false);

        public void OnInputMouseLook(InputValue value) =>
            controller.OnLookInput(value.Get<Vector2>(), true);

        public void OnInputStance(InputValue value) =>
            controller.OnStanceInput(value.Get<float>());

        public void OnInputScope(InputValue value) =>
            controller.OnScopeInput(Maths.NonZero(value.Get<float>()));

        public void OnInputAim(InputValue value) =>
            controller.OnAimInput(Maths.NonZero(value.Get<float>()));

        public void OnInputAttack(InputValue value) =>
            controller.OnAttackInput(value.isPressed);

        private void OnEnable() {
            controller = Transforms.Entity(transform)
                .GetComponent<IPlayerController>();
        }
    }
}

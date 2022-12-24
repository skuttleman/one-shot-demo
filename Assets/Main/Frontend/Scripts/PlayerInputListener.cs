using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine.InputSystem;
using UnityEngine;
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSFE.Scripts {
    public class PlayerInputListener : MonoBehaviour {
        private IController<PlayerControllerInput> controller;

        public void OnMove(InputValue value) {
            controller.On(new MovementInput(value.Get<Vector2>()));
        }

        public void OnSprint(InputValue value) {
            controller.On(new SprintInput(value.isPressed));
        }

        public void OnLook(InputValue value) {
            controller.On(new LookInput(value.Get<Vector2>(), false));
        }

        public void OnMouseLook(InputValue value) {
            controller.On(new LookInput(value.Get<Vector2>(), true));
        }

        public void OnStance(InputValue _) {
            controller.On(new StanceInput());
        }

        public void OnScope(InputValue value) {
            controller.On(new ScopeInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAim(InputValue value) {
            controller.On(new AimInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAttack(InputValue value) {
            controller.On(new AttackInput(value.isPressed));
        }

        public void OnClimb(InputValue value) {
            controller.On(new ClimbInput(ClimbDirection.UP));
        }

        public void OnDrop(InputValue value) {
            controller.On(new ClimbInput(ClimbDirection.DOWN));
        }

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IController<PlayerControllerInput>>();
        }
    }
}

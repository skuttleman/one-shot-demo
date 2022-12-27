using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.System.Interfaces.Controllers;
using OSCore.Utils;
using UnityEngine.InputSystem.Interactions;
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

        public void OnScope(InputValue value) {
            controller.On(new ScopeInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAim(InputValue value) {
            controller.On(new AimInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAttack(InputValue value) {
            controller.On(new AttackInput(value.isPressed));
        }

        public void OnClimb(InputValue _) {
            controller.On(new ClimbInput(ClimbDirection.UP));
        }

        public void OnDrop(InputValue _) {
            controller.On(new ClimbInput(ClimbDirection.DOWN));
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            controller = Transforms.Entity(transform)
                .GetComponent<IController<PlayerControllerInput>>();
            GetComponent<PlayerInput>()
                .currentActionMap
                .actions
                .First(Fns.Filter<InputAction>(action => action.name == "Stance"))
                .performed += context => {
                    switch (context.interaction) {
                        case MultiTapInteraction interaction:
                            controller.On(new DiveInput());
                            break;
                        case TapInteraction interaction:
                            controller.On(new StanceInput());
                            break;
                    }
                };
        }
    }
}

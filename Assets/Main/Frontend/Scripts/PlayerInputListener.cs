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
            controller.Handle(new MovementInput(value.Get<Vector2>()));
        }

        public void OnSprint(InputValue value) {
            controller.Handle(new SprintInput(value.isPressed));
        }

        public void OnLook(InputValue value) {
            controller.Handle(new LookInput(value.Get<Vector2>(), false));
        }

        public void OnMouseLook(InputValue value) {
            controller.Handle(new LookInput(value.Get<Vector2>(), true));
        }

        public void OnScope(InputValue value) {
            controller.Handle(new ScopeInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAim(InputValue value) {
            controller.Handle(new AimInput(Maths.NonZero(value.Get<float>())));
        }

        public void OnAttack(InputValue value) {
            controller.Handle(new AttackInput(value.isPressed));
        }

        public void OnClimb(InputValue _) {
            controller.Handle(new ClimbInput(ClimbDirection.UP));
        }

        public void OnDrop(InputValue _) {
            controller.Handle(new ClimbInput(ClimbDirection.DOWN));
        }

        public void OnTBD(InputValue _) {
            controller.Handle(new TBDInput());
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            FilterXF<InputAction> xform =
                (FilterXF<InputAction>)Fns.Filter<InputAction>(action => action.name == "Stance");
            xform.XForm<PlayerInput>((acc, item) => acc);

            controller = Transforms.Body(transform)
                .GetComponent<IController<PlayerControllerInput>>();
            GetComponent<PlayerInput>()
                .currentActionMap
                .actions
                .First(xform)
                .performed += context => {
                    switch (context.interaction) {
                        case MultiTapInteraction interaction:
                            controller.Handle(new DiveInput());
                            break;
                        case TapInteraction interaction:
                            controller.Handle(new StanceInput());
                            break;
                    }
                };
        }
    }
}

using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OSFE.Characters.Player {
    public class PlayerInputListener : MonoBehaviour {
        [SerializeField] PlayerCfgSO cfg;

        IGameSystem system;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
            Brain().Init(cfg);
        }

        public void OnInputMove(InputValue value) =>
            Brain().OnMovementInput(value.Get<Vector2>());

        public void OnInputRun(InputValue value) =>
            Brain().OnSprintInput(value.isPressed);

        public void OnInputLook(InputValue value) =>
            Brain().OnLookInput(value.Get<Vector2>(), false);

        public void OnInputMouseLook(InputValue value) =>
            Brain().OnLookInput(value.Get<Vector2>(), true);

        public void OnInputStance(InputValue value) =>
            Brain().OnStanceInput(value.Get<float>());

        public void OnInputScope(InputValue value) =>
            Brain().OnScopeInput(Maths.NonZero(value.Get<float>()));

        public void OnInputAim(InputValue value) =>
            Brain().OnAimInput(Maths.NonZero(value.Get<float>()));

        public void OnInputAttack(InputValue value) =>
            Brain().OnAttackInput(value.isPressed);

        IPlayerController Brain() =>
            system.Send<IControllerManager, IPlayerController>(mngr =>
                mngr.Ensure<IPlayerController>(transform));
    }
}

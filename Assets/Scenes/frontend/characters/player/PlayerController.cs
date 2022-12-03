using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using UnityEngine.InputSystem;
using UnityEngine;

namespace OSFE.Characters.Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField] PlayerCfgSO cfg;

        IGameSystem system;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
            Brain().Init(cfg);
        }

        /* Input Events */

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

        /* Animation Events */

        public void OnStanceChange(PlayerStance stance) =>
            Brain().OnStanceChanged(stance);

        public void OnAttackMode(PlayerAttackMode mode) =>
            Brain().OnAttackModeChanged(mode);

        public void OnMovement(int moving) =>
            Brain().OnMovementChanged(moving != 0);

        public void OnScope(int enabled) =>
            Brain().OnScopingChanged(enabled != 0);

        public void OnStep() =>
            Brain().OnPlayerStep();

        IPlayerControllerBrain Brain() =>
            system.Send<IControllerBrainManager, IPlayerControllerBrain>(mngr =>
                mngr.Ensure<IPlayerControllerBrain>(transform));
    }
}

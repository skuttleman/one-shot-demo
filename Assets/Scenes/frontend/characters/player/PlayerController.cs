using OSCore.Data.Enums;
using OSCore.Data.Events.Brains;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using UnityEngine.InputSystem;
using UnityEngine;
using static OSCore.Data.Events.Brains.Player.AnimationEmittedEvent;
using static OSCore.Data.Events.Brains.Player.InputEvent;
using static OSCore.Data.Events.Brains.SPA.SPAEvent;

namespace OSFE.Characters.Player {
    public class PlayerController : MonoBehaviour {
        [SerializeField] PlayerCfgSO cfg;
        [SerializeField] GravityCfgSO gravityCfg;
        [SerializeField] CollisionCfgSO collisionCfg;

        IGameSystem system;

        void OnEnable() {
            system = FindObjectOfType<GameController>();
            ToController(new InitEvent<PlayerCfgSO>(cfg));
            ToSPA(new InitEvent<GravityCfgSO>(gravityCfg));
            ToSPA(new InitEvent<CollisionCfgSO>(collisionCfg));

            system.Send<IControllerBrainManager>(mngr => {
                IControllerBrain brain = mngr.Ensure(EControllerBrainTag.SPA, transform);
                mngr.Ensure(EControllerBrainTag.PLAYER, transform)
                    .Handle(new InstallSPA(brain));
            });
        }

        /* Input Events */

        public void OnInputMove(InputValue value) =>
            ToController(new MovementInput(value.Get<Vector2>()));

        public void OnInputRun(InputValue value) =>
            ToController(new SprintInput(value.isPressed));

        public void OnInputLook(InputValue value) =>
            ToController(new LookInput(value.Get<Vector2>(), false));

        public void OnInputMouseLook(InputValue value) =>
            ToController(new LookInput(value.Get<Vector2>(), true));

        public void OnInputStance(InputValue value) =>
            ToController(new StanceInput(value.Get<float>()));

        public void OnInputScope(InputValue value) =>
            ToController(new ScopeInput(Maths.NonZero(value.Get<float>())));

        public void OnInputAim(InputValue value) =>
            ToController(new AimInput(Maths.NonZero(value.Get<float>())));

        public void OnInputAttack(InputValue value) =>
            ToController(new AttackInput(value.isPressed));

        /* Animation Events */

        public void OnStanceChange(PlayerStance stance) =>
            ToController(new StanceChanged(stance));

        public void OnAttackMode(PlayerAttackMode mode) =>
            ToController(new AttackModeChanged(mode));

        public void OnMovement(int moving) =>
            ToController(new MovementChanged(moving != 0));

        public void OnScope(int enabled) =>
            ToController(new ScopingChanged(enabled != 0));

        public void OnStep() =>
            ToController(new PlayerStep());

        /* Send to Brain */

        void ToController(IEvent message) =>
            SendMessage(EControllerBrainTag.PLAYER, message);

        void ToSPA(IEvent message) =>
            SendMessage(EControllerBrainTag.SPA, message);

        void SendMessage(EControllerBrainTag tag, IEvent message) =>
            system.Send<IControllerBrainManager>(mngr =>
                mngr.Ensure(tag, transform)
                    .Handle(message));
    }
}

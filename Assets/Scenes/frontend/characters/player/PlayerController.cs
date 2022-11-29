using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using OSCore;
using OSCore.Utils;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;
using OSCore.Data.Enums;
using OSCore.Events.Brains;

namespace OSFE {
    public class PlayerController : MonoBehaviour {
        [SerializeField] PlayerCfgSO cfg;
        IGameSystem system;

        void Start() {
            system = FindObjectOfType<GameController>();

            SendMessage(new InitEvent<PlayerCfgSO>(cfg));
        }

        /* Input Events */

        public void OnInputMove(InputValue value) =>
            SendMessage(new InputEvent.MovementInput(value.Get<Vector2>()));

        public void OnInputLook(InputValue value) =>
            SendMessage(new InputEvent.LookInput(value.Get<Vector2>(), false));

        public void OnInputMouseLook(InputValue value) =>
            SendMessage(new InputEvent.LookInput(value.Get<Vector2>(), true));

        public void OnInputStance(InputValue value) =>
            SendMessage(new InputEvent.StanceInput(value.Get<float>()));

        public void OnInputScope(InputValue value) =>
            SendMessage(new InputEvent.ScopeInput(Maths.NonZero(value.Get<float>())));

        public void OnInputAim(InputValue value) =>
            SendMessage(new InputEvent.AimInput(Maths.NonZero(value.Get<float>())));

        public void OnInputAttack(InputValue value) =>
            SendMessage(new InputEvent.AttackInput(value.isPressed));

        /* Animation Events */

        public void OnStanceChange(PlayerStance stance) =>
            SendMessage(new AnimationEmittedEvent.StanceChanged(stance));

        public void OnAttackMode(PlayerAttackMode mode) =>
            SendMessage(new AnimationEmittedEvent.AttackModeChanged(mode));

        public void OnMovement(int moving) =>
            SendMessage(new AnimationEmittedEvent.MovementChanged(moving != 0));

        public void OnScope(int enabled) =>
            SendMessage(new AnimationEmittedEvent.ScopingChanged(enabled != 0));

        public void OnStep() =>
            SendMessage(new AnimationEmittedEvent.PlayerStep());

        /* Send to Brain */

        void SendMessage(IEvent message) =>
            system.Send<IControllerBrainManager>(mngr =>
                mngr.EnsureUnique(EControllerBrainTag.PLAYER, transform)
                    .OnMessage(message));
    }
}

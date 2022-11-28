using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using OSCore;
using OSCore.Utils;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;

namespace OSFE {
    public class PlayerController : MonoBehaviour {
        IGameSystem controller;

        void Start() {
            controller = FindObjectOfType<GameController>();
        }

        /* Input Events */

        public void OnInputMove(InputValue value) =>
            SendMessage(new InputEvent.MovementInput(value.Get<Vector2>()));

        public void OnInputLook(InputValue value) =>
            SendMessage(new InputEvent.LookInput(value.Get<Vector2>()));

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

        void SendMessage(IPlayerEvent message) =>
            controller.Send<IControllerBrainManager>(mngr =>
                mngr.Ensure(transform, EControllerBrainTag.PLAYER)
                    .OnMessage(message));
    }
}

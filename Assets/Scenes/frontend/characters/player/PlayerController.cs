using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using OSCore.Interfaces;
using OSCore;
using OSCore.Utils;
using OSCore.Events.Brains.Player;

namespace OSFE {
    public class PlayerController : MonoBehaviour {
        IGameSystem controller;

        void Start() {
            controller = FindObjectOfType<GameController>();
        }

        public void OnInputMove(InputValue value) =>
            SendMessage(new InputEvent.MovementInput(value.Get<Vector2>()));

        public void OnInputLook(InputValue value) =>
            SendMessage(new InputEvent.LookInput(value.Get<Vector2>()));

        public void OnInputStance(InputValue value) =>
            SendMessage(new InputEvent.StanceInput(value.Get<float>()));

        public void OnInputScope(InputValue value) =>
            SendMessage(new InputEvent.ScopeInput(value.isPressed));

        public void OnInputAim(InputValue value) =>
            SendMessage(new InputEvent.AimInput(Maths.NonZero(value.Get<float>())));

        public void OnInputAttack(InputValue value) =>
            SendMessage(new InputEvent.AttackInput(value.isPressed));

        void SendMessage(IPlayerEvent message) =>
            controller.With<IControllerBrainManager>(mngr =>
                mngr.Ensure(transform, Sets.Of("player"))
                    .OnMessageSync(message));
    }
}

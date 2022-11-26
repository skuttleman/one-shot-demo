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

        void SendMessage(IPlayerEvent message) =>
            controller.With<IControllerBrainManager>(mngr =>
                mngr.Ensure(transform, Sets.Of("player"))
                    .OnMessageSync(message));
    }
}

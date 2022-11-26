using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using OSCore.Interfaces;
using OSCore;
using OSCore.Utils;
using OSCore.Events.Brains.Player;

namespace OSFE {
    public class PlayerController : MonoBehaviour {
        GameController controller;
        IControllerBrain brain;

        void Start() {
            controller = FindObjectOfType<GameController>();
            brain = controller.Get<IControllerBrainFactory>().Create(transform, Sets.Of("player"));
        }

        public void OnInputMove(InputValue value) {
            brain.OnMessageSync(Messages.Move(value.Get<Vector2>()));
        }
    }
}

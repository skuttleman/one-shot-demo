using System.Collections;
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
            brain.OnMessageSync(new PlayerBrainMessage.StringMessage("A [STRING] message for you"));
        }

        // Update is called once per frame
        void Update() {


        }

        public void OnInputMove(InputValue value) {
            //Debug.Log("I*NPU MOV");
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using OSBE.Brains;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces;
using UnityEngine;

namespace OSBE.Brains {
    public class PlayerControllerBrain : AControllerBrain<PlayerBrainMessage> {
        Transform target;

        public PlayerControllerBrain(Transform transform) =>
            target = transform;

        internal override Action ProcessMessage(PlayerBrainMessage message) {
            return message switch {
                PlayerBrainMessage.StringMessage msg => () => Debug.Log("processing msg " + msg.message),
                _ => () => { }
            };
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using OSBE.Brains;
using OSCore.Events.Brains.Player;
using OSCore.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Brains {
    public class PlayerControllerBrain : AControllerBrain<Message> {
        Transform target;
        Animator animator;
        Rigidbody rb;

        // movement state
        Vector2 movement = Vector2.zero;
        Vector2 facing = Vector2.zero;
        bool isMoving = false;
        bool isScoping = false;
        bool isLooking = false;

        public PlayerControllerBrain(Transform transform) =>
            target = transform;

        internal override Action ProcessMessage(Message message) {
            return message switch {
                Message.Movement msg  => () => target.position += msg.direction.Upgrade(),
                Message msg => () => Debug.LogError("Don't know how to process" + msg),
            };
        }
    }
}

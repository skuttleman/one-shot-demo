﻿using OSCore.System.Interfaces;
using OSCore.System;
using System;
using UnityEngine;

namespace OSBE.Controllers {
    public abstract class ACharacterAnimator<State, Details> : MonoBehaviour
        where Details : AnimStateDetails<State> {
        private IStateReceiver<State> receiver;
        private AStateNode<State, Details> node = null;
        public State state => node == null ? default : node.state;
        private Details details;
        private Animator anim;
        private float timeInState;
        public float animSpeed { get; private set; } = 1f;

        protected void Init(IStateReceiver<State> receiver, AStateNode<State, Details> node, Details details) {
            this.receiver = receiver;
            this.node = node;
            this.details = details;
            anim = GetComponent<Animator>();
            anim.speed = animSpeed;
            timeInState = 0f;
            receiver.OnStateInit(state);
        }

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * node.animSpeed;
        }

        public void UpdateState(Func<Details, Details> updateFn) {
            details = updateFn(details);
        }

        private void Transition(AStateNode<State, Details> node) {
            if (this.node != node) {
                State prev = this.node.state;
                this.node = node;
                timeInState = 0f;
                receiver.OnStateTransition(prev, state);
                anim.speed = animSpeed * node.animSpeed;
                anim.Play(node.state.ToString());
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Update() {
            if (node is null) return;
            timeInState += Time.deltaTime;

            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            Transition(node.Next(details with {
                timeInState = timeInState,
                loops = info.normalizedTime,
            }));
        }
    }
}

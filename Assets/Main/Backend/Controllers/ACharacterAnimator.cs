using OSCore.System.Interfaces;
using OSCore.System;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers {
    public abstract class ACharacterAnimator<State, Signal> : MonoBehaviour {
        private readonly Queue<Signal> signals;
        private Animator anim;
        private AStateNode<State, Signal> state = null;
        private IStateReceiver<State> receiver;
        private float timeInState;

        public ACharacterAnimator() {
            signals = new();
        }

        protected void Init(IStateReceiver<State> receiver, AStateNode<State, Signal> tree) {
            this.receiver = receiver;
            anim = GetComponent<Animator>();
            state = tree;
            timeInState = 0f;
            receiver.OnStateEnter(state.state);
        }

        public void SetSpeed(float speed) =>
            anim.speed = speed;

        public void Send(Signal signal) =>
            signals.Enqueue(signal);

        private void Update() {
            if (state is null) return;
            timeInState += Time.deltaTime;
            if (timeInState < state.minTime) return;

            if (state != state.next)
                Transition(state.next);
            else if (signals.TryDequeue(out Signal signal))
                Transition(state.Next(signal));
        }

        private void Transition(AStateNode<State, Signal> state) {
            if (this.state != state) {
                receiver.OnStateExit(this.state.state);
                this.state = state;
                timeInState = 0f;
                anim.Play(state.state.ToString());
                receiver.OnStateEnter(this.state.state);
            }
        }
    }
}

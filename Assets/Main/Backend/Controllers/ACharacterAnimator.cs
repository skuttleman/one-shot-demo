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
        public float animSpeed { get; private set; } = 1f;

        public ACharacterAnimator() {
            signals = new();
        }

        protected void Init(IStateReceiver<State> receiver, AStateNode<State, Signal> tree) {
            this.receiver = receiver;
            anim = GetComponent<Animator>();
            anim.speed = animSpeed;
            state = tree;
            timeInState = 0f;
            receiver.OnStateEnter(state.state);
        }

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * state.animSpeed;
        }

        public void Send(Signal signal) =>
            signals.Enqueue(signal);

        public bool CanTransition(Signal signal) =>
            state != state.Next(signal);

        public bool DidAllFramesPlay() {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            return info.loop || info.normalizedTime >= 1f;
        }

        private void Update() {
            if (state is null || !DidAllFramesPlay()) return;
            timeInState += Time.deltaTime;
            if (timeInState < state.minTime) return;

            if (state != state.next)
                Transition(state.next);
            else if (signals.TryDequeue(out Signal signal))
                Transition(state.Next(signal));
        }

        private void Transition(AStateNode<State, Signal> state) {
            if (this.state != state) {
                State curr = this.state.state;
                receiver.OnStateExit(curr);
                this.state = state;
                timeInState = 0f;
                receiver.OnStateTransition(curr, state.state);
                anim.speed = animSpeed * state.animSpeed;
                anim.Play(state.state.ToString());
                receiver.OnStateEnter(state.state);
            }
        }
    }
}

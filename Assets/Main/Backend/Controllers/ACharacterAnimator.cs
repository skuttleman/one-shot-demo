using OSCore.System.Interfaces;
using OSCore.System;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OSBE.Controllers {
    public abstract class ACharacterAnimator<State, Details> : MonoBehaviour
        where Details : AnimStateDetails<State> {
        private IStateReceiver<State> receiver;
        private AStateNode<State, Details> node = null;
        private Details state;
        private Animator anim;
        private float timeInState;
        public float animSpeed { get; private set; } = 1f;

        protected void Init(IStateReceiver<State> receiver, AStateNode<State, Details> node, Details state) {
            this.receiver = receiver;
            this.node = node;
            this.state = state;
            anim = GetComponent<Animator>();
            anim.speed = animSpeed;
            timeInState = 0f;
            receiver.OnStateInit(this.state.state);
        }

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * node.animSpeed;
        }

        public void UpdateState(Func<Details, Details> updateFn) {
            state = updateFn(state);
        }

        private void Transition(AStateNode<State, Details> node) {
            if (this.node != node) {
                State curr = this.node.state;
                this.node = node;
                timeInState = 0f;
                receiver.OnStateTransition(curr, node.state);
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
            Transition(node.Next(state with {
                timeInState = timeInState,
                loops = info.normalizedTime,
            }));
        }
    }

    public abstract class ACharacterAnimatorOld<State, Signal> : MonoBehaviour {
        private readonly Queue<Signal> signals;
        private Animator anim;
        private AStateNodeOld<State, Signal> state = null;
        private IStateReceiver<State> receiver;
        private float timeInState;
        public float animSpeed { get; private set; } = 1f;

        public ACharacterAnimatorOld() {
            signals = new();
        }

        protected void Init(IStateReceiver<State> receiver, AStateNodeOld<State, Signal> state) {
            this.receiver = receiver;
            anim = GetComponent<Animator>();
            anim.speed = animSpeed;
            this.state = state;
            timeInState = 0f;
        }

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * state.animSpeed;
        }

        public void Send(Signal signal) {
            signals.Enqueue(signal);
        }

        public bool CanTransition(Signal signal) =>
            state != state.Next(signal);

        private void Transition(AStateNodeOld<State, Signal> state) {
            if (this.state != state) {
                State curr = this.state.state;
                this.state = state;
                timeInState = 0f;
                receiver.OnStateTransition(curr, state.state);
                anim.speed = animSpeed * state.animSpeed;
                anim.Play(state.state.ToString());
            }
        }

        private bool DidAllFramesPlay() {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            return info.loop || info.normalizedTime >= (state?.minLoops ?? 0f);
        }

        /*
         * Lifecycle Methods
         */

        private void Update() {
            if (state is null || !DidAllFramesPlay()) return;
            timeInState += Time.deltaTime;
            if (timeInState < state.minTime) return;

            if (state != state.next)
                Transition(state.next);
            else if (signals.TryDequeue(out Signal signal))
                Transition(state.Next(signal));
        }
    }
}

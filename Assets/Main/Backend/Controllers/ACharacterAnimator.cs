using OSCore.System.Interfaces;
using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers {
    public abstract class ACharacterAnimator<State, Details> : APredicativeStateMachine<State, Details>, IStateReceiver<State>
        where Details : AnimStateDetails<State> {

        public float animSpeed { get; private set; } = 1f;

        private AnimNode<State, Details> animNode => (AnimNode<State, Details>)node;
        private IStateReceiver<State> receiver;
        private Animator anim;

        protected void Init(
            RuntimeAnimatorController controller,
            IStateReceiver<State> receiver,
            AnimNode<State, Details> node,
            Details details) {
            this.receiver = receiver;
            anim = gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            anim.speed = animSpeed;
            base.Init(this, node, details);
            Play(node.state);
        }

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * animNode.animSpeed;
        }

        public void OnStateInit(State curr) {
            Play(curr);
            receiver.OnStateInit(curr);
        }
        public void OnStateTransition(State prev, State curr) {
            Play(curr);
            receiver.OnStateTransition(prev, curr);
        }

        private void Play(State state) {
            anim.Play(state.ToString());
        }

        protected override Details Enrich(Details details) {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            return details with { loops = info.normalizedTime };
        }
    }
}

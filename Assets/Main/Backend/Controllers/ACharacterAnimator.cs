using OSCore.System.Interfaces;
using OSCore.System;
using System;
using UnityEngine;

namespace OSBE.Controllers {
    public abstract class ACharacterAnimator<State, Details> : APredicativeStateMachine<State, Details>
        where State : Enum
        where Details : AnimStateDetails<State> {

        public float animSpeed { get; private set; } = 1f;

        private AnimNode<State, Details> animNode => (AnimNode<State, Details>)node;
        private Animator anim;

        public void SetSpeed(float speed) {
            animSpeed = speed;
            anim.speed = animSpeed * animNode.animSpeed;
        }

        protected void Init(
            RuntimeAnimatorController controller,
            IStateReceiver<State> receiver,
            AnimNode<State, Details> node,
            Details details
        ) {
            anim = gameObject.AddComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            anim.speed = animSpeed;

            AnimStateReceiver<State> impl = new(receiver, anim);
            base.Init(impl, node, details);

            impl.Play(node.state);
        }

        protected override Details Enrich(Details details) {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            return details with { loops = info.normalizedTime };
        }
    }

    internal class AnimStateReceiver<State> : IStateReceiver<State> where State : Enum {
        private readonly IStateReceiver<State> receiver;
        private readonly Animator anim;

        public AnimStateReceiver(IStateReceiver<State> receiver, Animator anim) {
            this.receiver = receiver;
            this.anim = anim;
        }

        public void OnStateInit(State curr) {
            Play(curr);
            receiver.OnStateInit(curr);
        }

        public void OnStateTransition(State prev, State curr) {
            Play(curr);
            receiver.OnStateTransition(prev, curr);
        }

        public void Play(State state) {
            anim.Play(state.ToString());
        }
    }
}

using OSCore.System.Interfaces;
using OSCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.System {
    [Serializable]
    public abstract record APredicativeStateDetails<State> {
        public float timeInState { get; init; }
    }

    [Serializable]
    public abstract class APredicativeStateNode<State, Details> where Details : APredicativeStateDetails<State> {
        public readonly State state;

        protected readonly IList<(Predicate<Details>, APredicativeStateNode<State, Details>)> edges;

        public APredicativeStateNode(State state) {
            this.state = state;
            edges = new List<(Predicate<Details>, APredicativeStateNode<State, Details>)>();
        }

        public void AddTransition(Predicate<APredicativeStateDetails<State>> pred, APredicativeStateNode<State, Details> node) {
            edges.Add((pred, node));
        }

        public APredicativeStateNode<State, Details> Next(Details details) {
            IXForm<
                (Predicate<Details> pred, APredicativeStateNode<State, Details> state),
                APredicativeStateNode<State, Details>> xform =
                    Fns.Filter<(
                        Predicate<Details> pred,
                        APredicativeStateNode<State, Details> state)>(tpl => tpl.pred(details))
                    .Comp(Fns.Map<(
                        Predicate<Details> pred,
                        APredicativeStateNode<State, Details> state),
                        APredicativeStateNode<State, Details>>(tpl => tpl.state));
            return edges.First(xform) ?? this;
        }
    }

    public abstract class APredicativeStateMachine<State, Details> : MonoBehaviour
            where Details : APredicativeStateDetails<State> {

        public State state => node == null ? default : node.state;

        protected APredicativeStateNode<State, Details> node = null;

        private IStateReceiver<State> receiver;
        private Details details;
        private float timeInState;

        protected virtual void Init(
            IStateReceiver<State> receiver,
            APredicativeStateNode<State, Details> node,
            Details details) {
            this.receiver = receiver;
            this.node = node;
            this.details = details;
            timeInState = 0f;
            receiver.OnStateInit(state);
        }

        public void Transition(Func<Details, Details> updateFn) {
            details = updateFn(details);
        }

        private void TransitionTo(APredicativeStateNode<State, Details> node) {
            if (this.node != node) {
                State prev = this.node.state;
                this.node = node;
                timeInState = 0f;
                receiver.OnStateTransition(prev, state);
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Update() {
            if (node is null) return;
            TransitionTo(node.Next(Enrich(details with {
                timeInState = timeInState,
            })));
            timeInState += Time.deltaTime;
        }

        protected virtual Details Enrich(Details details) =>
            details;
    }
}

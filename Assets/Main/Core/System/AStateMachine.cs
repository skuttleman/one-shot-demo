using OSCore.System.Interfaces;
using OSCore.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.System {
    public abstract record StateDetails<State> {
        public float timeInState { get; init; }
    }

    public abstract class AStateNode<State, Details> where Details : StateDetails<State> {
        public readonly State state;

        protected readonly IList<(Predicate<Details>, AStateNode<State, Details>)> edges;

        public AStateNode(State state) {
            this.state = state;
            edges = new List<(Predicate<Details>, AStateNode<State, Details>)>();
        }

        public void AddTransition(Predicate<StateDetails<State>> pred, AStateNode<State, Details> node) {
            edges.Add((pred, node));
        }

        public AStateNode<State, Details> Next(Details details) {
            IXForm<
                (Predicate<Details> pred, AStateNode<State, Details> state),
                AStateNode<State, Details>> xform =
                    Fns.Filter<(
                        Predicate<Details> pred,
                        AStateNode<State, Details> state)>(tpl => tpl.pred(details))
                    .Comp(Fns.Map<(
                        Predicate<Details> pred,
                        AStateNode<State, Details> state),
                        AStateNode<State, Details>>(tpl => tpl.state));
            return edges.First(xform) ?? this;
        }
    }

    public abstract class AStateMachine<State, Details> : MonoBehaviour
            where Details : StateDetails<State> {

        public State state => node == null ? default : node.state;

        protected AStateNode<State, Details> node = null;

        private IStateReceiver<State> receiver;
        private Details details;
        private float timeInState;

        protected virtual void Init(
            IStateReceiver<State> receiver,
            AStateNode<State, Details> node,
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

        private void TransitionTo(AStateNode<State, Details> node) {
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

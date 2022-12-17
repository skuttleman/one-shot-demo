using System.Collections.Generic;

namespace OSCore.System {
    public abstract class AStateNode<State, Signal> {
        public readonly State state;
        public readonly float minTime;
        public virtual AStateNode<State, Signal> next => this;

        private readonly IDictionary<Signal, AStateNode<State, Signal>> edges;

        public AStateNode(State state, float minTime) {
            this.state = state;
            this.minTime = minTime;
            edges = new Dictionary<Signal, AStateNode<State, Signal>>();
        }

        public void SetEdge(Signal signal, AStateNode<State, Signal> node) =>
            edges.Add(signal, node);

        public AStateNode<State, Signal> Next(Signal signal) {
            if (edges.TryGetValue(signal, out AStateNode<State, Signal> node))
                return node;
            return this;
        }
    }

    public class StableNode<State, Signal> : AStateNode<State, Signal> {
        public StableNode(State state) : base(state, 0f) { }
        public StableNode(State state, float minTime) : base(state, minTime) { }
    }

    public class TransitionNode<State, Signal> : AStateNode<State, Signal> {
        private readonly AStateNode<State, Signal> target;
        public override AStateNode<State, Signal> next => target;

        public TransitionNode(State state, float minTime, AStateNode<State, Signal> target) : base(state, minTime) {
            this.target = target;
        }
    }

    public static class StateNodeBuilder {
        public static AStateNode<State, Signal> To<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            AStateNode<State, Signal> target) {
            node.SetEdge(signal, target);
            return node;
        }

        public static AStateNode<State, Signal> Through<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            State transition,
            float minTime,
            AStateNode<State, Signal> target) {
            node.SetEdge(signal, new TransitionNode<State, Signal>(transition, minTime, target));
            return node;
        }

        public static AStateNode<State, Signal> With<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            State transition,
            float minTime) =>
            node.Through(signal, transition, minTime, node);
    }
}

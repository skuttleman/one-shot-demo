using System.Collections.Generic;
using OSCore.Utils;

namespace OSCore.System {
    public abstract class AStateNode<State, Signal> {
        public readonly State state;
        public readonly float minTime;
        public readonly float minLoops;
        public readonly float animSpeed;
        public virtual AStateNode<State, Signal> next => this;

        private readonly IDictionary<Signal, AStateNode<State, Signal>> edges;

        public AStateNode(State state, float minTime, float minLoops, float animSpeed) {
            this.state = state;
            this.minTime = minTime;
            this.minLoops = minLoops;

            this.animSpeed = animSpeed;
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
        public StableNode(State state) : base(state, 0f, 0f, 1f) { }
        public StableNode(State state, float minTime, float minLoops) : base(state, minTime, minLoops, 1f) { }
    }

    public class TransitionNode<State, Signal> : AStateNode<State, Signal> {
        private readonly AStateNode<State, Signal> target;
        public override AStateNode<State, Signal> next => target;

        public TransitionNode(
            State state,
            float minTime,
            AStateNode<State, Signal> target) : this(state, minTime, 0f, 1f, target) { }

        public TransitionNode(
            State state,
            float minTime,
            float minLoops,
            float animSpeed,
            AStateNode<State, Signal> target) : base(state, minTime, minLoops, animSpeed) {
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

        public static AStateNode<State, Signal> To<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            AStateNode<State, Signal> target,
            params (State transition, float minTime)[] comps) {
            node.SetEdge(signal, comps.Reduce((target, comp) =>
                new TransitionNode<State, Signal>(comp.transition, comp.minTime, target)
                , target));
            return node;
        }

        public static AStateNode<State, Signal> To<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            AStateNode<State, Signal> target,
            params (State transition, float minTime, float minLoops, float animSpeed)[] comps) {
            node.SetEdge(signal, comps.Reduce((target, comp) =>
                new TransitionNode<State, Signal>(comp.transition, comp.minTime, comp.minLoops, comp.animSpeed, target)
                , target));
            return node;
        }

        public static AStateNode<State, Signal> With<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            State transition,
            float minTime) =>
            node.To(signal, node, (transition, minTime));

        public static AStateNode<State, Signal> With<State, Signal>(
            this AStateNode<State, Signal> node,
            Signal signal,
            State transition,
            float minTime,
            float minLoops,
            float animSpeed) =>
            node.To(signal, node, (transition, minTime, minLoops, animSpeed));
    }
}

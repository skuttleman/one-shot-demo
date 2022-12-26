using System;
using System.Collections.Generic;
using OSCore.Utils;

namespace OSCore.System {
    public abstract record AnimStateDetails<State> {
        public State state { get; init; }
        public float timeInState { get; init; }
        public float loops { get; init; }
    }

    public abstract class AStateNode<State, Details>
        where Details : AnimStateDetails<State> {
        public readonly State state;
        public readonly float animSpeed;

        private readonly IList<(Predicate<Details>, AStateNode<State, Details>)> edges;

        public AStateNode(State state, float animSpeed) {
            this.state = state;

            this.animSpeed = animSpeed;
            edges = new List<(Predicate<Details>, AStateNode<State, Details>)>();
        }

        public void AddTransition(Predicate<AnimStateDetails<State>> pred, AStateNode<State, Details> node) {
            edges.Add((pred, node));
        }

        public AStateNode<State, Details> Next(Details details) =>
            edges.First(
                Fns.Filter<(
                    Predicate<Details> pred,
                    AStateNode<State, Details> state)>(tpl => tpl.pred(details))
                .Comp(Fns.Map<(
                    Predicate<Details> pred,
                    AStateNode<State, Details> state),
                    AStateNode<State, Details>>(tpl => tpl.state)))
                ?? this;
    }

    public class StableNode<State, Details> : AStateNode<State, Details>
        where Details : AnimStateDetails<State> {
        public StableNode(State state) : this(state, 1f) { }
        public StableNode(State state, float animSpeed) : base(state, animSpeed) { }
    }

    public class TransitionNode<State, Details> : AStateNode<State, Details>
        where Details : AnimStateDetails<State> {
        private readonly AStateNode<State, Details> target;

        public TransitionNode(
            State state,
            AStateNode<State, Details> target) : this(state, 1f, target) { }

        public TransitionNode(
            State state,
            float animSpeed,
            AStateNode<State, Details> target) : base(state, animSpeed) {
            this.target = target;
        }
    }

    public abstract class AStateNodeOld<State, Signal> {
        public readonly State state;
        public readonly float minTime;
        public readonly float minLoops;
        public readonly float animSpeed;
        public virtual AStateNodeOld<State, Signal> next => this;

        private readonly IDictionary<Signal, AStateNodeOld<State, Signal>> edges;

        public AStateNodeOld(State state, float minTime, float minLoops, float animSpeed) {
            this.state = state;
            this.minTime = minTime;
            this.minLoops = minLoops;

            this.animSpeed = animSpeed;
            edges = new Dictionary<Signal, AStateNodeOld<State, Signal>>();
        }

        public void SetEdge(Signal signal, AStateNodeOld<State, Signal> node) {
            edges.Add(signal, node);
        }

        public AStateNodeOld<State, Signal> Next(Signal signal) {
            if (edges.TryGetValue(signal, out AStateNodeOld<State, Signal> node))
                return node;
            return this;
        }
    }

    public class StableNodeOld<State, Signal> : AStateNodeOld<State, Signal> {
        public StableNodeOld(State state) : base(state, 0f, 0f, 1f) { }
        public StableNodeOld(State state, float minTime, float minLoops) : base(state, minTime, minLoops, 1f) { }
    }

    public class TransitionNodeOld<State, Signal> : AStateNodeOld<State, Signal> {
        private readonly AStateNodeOld<State, Signal> target;
        public override AStateNodeOld<State, Signal> next => target;

        public TransitionNodeOld(
            State state,
            float minTime,
            AStateNodeOld<State, Signal> target) : this(state, minTime, 0f, 1f, target) { }

        public TransitionNodeOld(
            State state,
            float minTime,
            float minLoops,
            float animSpeed,
            AStateNodeOld<State, Signal> target) : base(state, minTime, minLoops, animSpeed) {
            this.target = target;
        }
    }

    public static class StateNodeBuilder {
        public static AStateNode<State, Details> To<State, Details>(
            this AStateNode<State, Details> node,
            Predicate<Details> pred,
            AStateNode<State, Details> target)
            where Details : AnimStateDetails<State> {
            node.AddTransition(details => pred((Details)details), target);
            return node;
        }

        public static AStateNode<State, Details> To<State, Details>(
            this AStateNode<State, Details> node,
            Predicate<Details> pred,
            float minTime,
            float minLoops,
            AStateNode<State, Details> target)
            where Details : AnimStateDetails<State> {
            node.AddTransition(details =>
                details.timeInState >= minTime
                && details.loops >= minLoops
                && pred((Details)details), target);
            return node;
        }






        public static AStateNodeOld<State, Signal> To<State, Signal>(
            this AStateNodeOld<State, Signal> node,
            Signal signal,
            AStateNodeOld<State, Signal> target) {
            node.SetEdge(signal, target);
            return node;
        }

        public static AStateNodeOld<State, Signal> To<State, Signal>(
            this AStateNodeOld<State, Signal> node,
            Signal signal,
            AStateNodeOld<State, Signal> target,
            params (State transition, float minTime)[] comps) {
            node.SetEdge(signal, comps.Reduce((target, comp) =>
                new TransitionNodeOld<State, Signal>(comp.transition, comp.minTime, target)
                , target));
            return node;
        }

        public static AStateNodeOld<State, Signal> To<State, Signal>(
            this AStateNodeOld<State, Signal> node,
            Signal signal,
            AStateNodeOld<State, Signal> target,
            params (State transition, float minTime, float minLoops, float animSpeed)[] comps) {
            node.SetEdge(signal, comps.Reduce((target, comp) =>
                new TransitionNodeOld<State, Signal>(comp.transition, comp.minTime, comp.minLoops, comp.animSpeed, target)
                , target));
            return node;
        }

        public static AStateNodeOld<State, Signal> With<State, Signal>(
            this AStateNodeOld<State, Signal> node,
            Signal signal,
            State transition,
            float minTime) =>
            node.To(signal, node, (transition, minTime));

        public static AStateNodeOld<State, Signal> With<State, Signal>(
            this AStateNodeOld<State, Signal> node,
            Signal signal,
            State transition,
            float minTime,
            float minLoops,
            float animSpeed) =>
            node.To(signal, node, (transition, minTime, minLoops, animSpeed));
    }
}

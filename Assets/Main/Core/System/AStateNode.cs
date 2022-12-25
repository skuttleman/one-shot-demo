using System;
using System.Collections.Generic;
using OSCore.System.Interfaces;
using OSCore.Utils;

namespace OSCore.System {
    namespace Interfaces {
        public interface IStateDetails<State> {
            public State state { get; }
        }
    }

    public abstract class AStateNode<State> {
        public readonly State state;
        public readonly float minTime;
        public readonly float minLoops;
        public readonly float animSpeed;

        private readonly IList<(Predicate<IStateDetails<State>>, AStateNode<State>)> edges;

        public AStateNode(State state, float minTime, float minLoops, float animSpeed) {
            this.state = state;
            this.minTime = minTime;
            this.minLoops = minLoops;

            this.animSpeed = animSpeed;
            edges = new List<(Predicate<IStateDetails<State>>, AStateNode<State>)>();
        }

        public void AddTransition(Predicate<IStateDetails<State>> pred, AStateNode<State> node) {
            edges.Add((pred, node));
        }

        public AStateNode<State> Next(IStateDetails<State> details) =>
            edges.First(
                Fns.Filter<(
                    Predicate<IStateDetails<State>> pred,
                    AStateNode<State> state)>(tpl => tpl.pred(details))
                .Comp(Fns.Map<(
                    Predicate<IStateDetails<State>> pred,
                    AStateNode<State> state),
                    AStateNode<State>>(tpl => tpl.state)))
                ?? this;
    }

    public class StableNode<State> : AStateNode<State> {
        public StableNode(State state) : this(state, 0f, 0f) { }
        public StableNode(State state, float minTime, float minLoops)
            : base(state, minTime, minLoops, 1f) { }
    }

    public class TransitionNode<State> : AStateNode<State> {
        private readonly AStateNode<State> target;

        public TransitionNode(
            State state,
            float minTime,
            AStateNode<State> target) : this(state, minTime, 0f, 1f, target) { }

        public TransitionNode(
            State state,
            float minTime,
            float minLoops,
            float animSpeed,
            AStateNode<State> target) : base(state, minTime, minLoops, animSpeed) {
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
        public static AStateNode<State> To<State, Details>(
            this AStateNode<State> node,
            Predicate<Details> pred,
            AStateNode<State> target)
            where Details : IStateDetails<State> {
            node.AddTransition(details => pred((Details)details), target);
            return node;
        }

        public static AStateNode<State> To<State, Details>(
            this AStateNode<State> node,
            Predicate<Details> pred,
            AStateNode<State> target,
            params (State transition, float minTime)[] comps)
            where Details : IStateDetails<State> {
            node.AddTransition(
                details => pred((Details)details),
                comps.Reduce((target, comp) =>
                    new TransitionNode<State>(comp.transition, comp.minTime, target)
                , target));
            return node;
        }

        public static AStateNode<State> To<State, Details>(
            this AStateNode<State> node,
            Predicate<Details> pred,
            AStateNode<State> target,
            params (State transition, float minTime, float minLoops, float animSpeed)[] comps)
            where Details : IStateDetails<State> {
            node.AddTransition(
                details => pred((Details)details),
                comps.Reduce((target, comp) =>
                    new TransitionNode<State>(comp.transition, comp.minTime, comp.minLoops, comp.animSpeed, target)
                , target));
            return node;
        }

        public static AStateNode<State> With<State, Details>(
            this AStateNode<State> node,
            Predicate<Details> pred,
            State transition,
            float minTime)
            where Details : IStateDetails<State> =>
                node.To(pred, node, (transition, minTime));

        public static AStateNode<State> With<State, Details>(
            this AStateNode<State> node,
            Predicate<Details> pred,
            State transition,
            float minTime,
            float minLoops,
            float animSpeed)
            where Details : IStateDetails<State> =>
                node.To(pred, node, (transition, minTime, minLoops, animSpeed));






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

using System;
using System.Collections.Generic;
using OSCore.Utils;

namespace OSCore.System {
    public abstract record AnimStateDetails<State> {
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

    public static class StateNodeBuilder {
        public static AStateNode<State, Details> To<State, Details>(
            this AStateNode<State, Details> node,
            Predicate<Details> pred,
            AStateNode<State, Details> target)
            where Details : AnimStateDetails<State> =>
            node.To(pred, 0.1f, 0f, target);

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
    }
}

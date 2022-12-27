using System;
using System.Collections.Generic;
using OSCore.Utils;

namespace OSCore.System {
    public abstract record AnimStateDetails<State> {
        public float timeInState { get; init; }
        public float loops { get; init; }
    }

    public class AnimNode<State, Details>
        where Details : AnimStateDetails<State> {
        public readonly State state;
        public readonly float animSpeed;

        private readonly IList<(Predicate<Details>, AnimNode<State, Details>)> edges;

        public AnimNode(State state) : this(state, 1f) { }

        public AnimNode(State state, float animSpeed) {
            this.state = state;

            this.animSpeed = animSpeed;
            edges = new List<(Predicate<Details>, AnimNode<State, Details>)>();
        }

        public void AddTransition(Predicate<AnimStateDetails<State>> pred, AnimNode<State, Details> node) {
            edges.Add((pred, node));
        }

        public AnimNode<State, Details> Next(Details details) =>
            edges.First(
                Fns.Filter<(
                    Predicate<Details> pred,
                    AnimNode<State, Details> state)>(tpl => tpl.pred(details))
                .Comp(Fns.Map<(
                    Predicate<Details> pred,
                    AnimNode<State, Details> state),
                    AnimNode<State, Details>>(tpl => tpl.state)))
                ?? this;
    }

    public static class AnimNodeUtils {
        public static AnimNode<State, Details> To<State, Details>(
            this AnimNode<State, Details> node,
            Predicate<Details> pred,
            AnimNode<State, Details> target)
            where Details : AnimStateDetails<State> =>
            node.To(pred, 0.1f, 0f, target);

        public static AnimNode<State, Details> To<State, Details>(
            this AnimNode<State, Details> node,
            Predicate<Details> pred,
            float minTime,
            float minLoops,
            AnimNode<State, Details> target)
            where Details : AnimStateDetails<State> {
            node.AddTransition(details =>
                details.timeInState >= minTime
                && details.loops >= minLoops
                && pred((Details)details), target);
            return node;
        }
    }
}

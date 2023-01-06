using System;

namespace OSCore.System {
    public abstract record AnimStateDetails<State> : StateDetails<State> {
        public float loops { get; init; }
    }

    public class AnimNode<State, Details> : AStateNode<State, Details>
        where Details : AnimStateDetails<State> {
        public readonly float animSpeed;

        public AnimNode(State state) : this(state, 1f) { }

        public AnimNode(State state, float animSpeed) : base(state) {
            this.animSpeed = animSpeed;
        }
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
                && ((AnimStateDetails<State>)details).loops >= minLoops
                && pred((Details)details), target);
            return node;
        }
    }
}

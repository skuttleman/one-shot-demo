using System;
using OSCore.Data.AI;

namespace OSCore.System {
    public record EnemyAIStateDetails : APredicativeStateDetails<EnemyAwareness> {
        public float suspicion { get; init; }
    }

    public class EnemyAINode : APredicativeStateNode<EnemyAwareness, EnemyAIStateDetails> {
        public EnemyAINode(EnemyAwareness state) : base(state) { }
    }

    public static class AINodeUtils {
        public static EnemyAINode To(
            this EnemyAINode node,
            Predicate<EnemyAIStateDetails> pred,
            EnemyAINode target) =>
            node.To(pred, 0.1f, target);

        public static EnemyAINode To(
            this EnemyAINode node,
            Predicate<EnemyAIStateDetails> pred,
            float minTime,
            EnemyAINode target) {
            node.AddTransition(details =>
                details.timeInState >= minTime
                && pred((EnemyAIStateDetails)details), target);

            return node;
        }
    }
}

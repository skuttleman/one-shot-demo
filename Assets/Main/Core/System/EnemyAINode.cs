using OSCore.Data.AI;
using OSCore.Data.Enums;
using System;
using UnityEngine;

namespace OSCore.System {
    public enum Visibility { NONE, LOW, MED, HIGH }
    public enum ViewDistance { OOV, NEAR, MED, FAR }
    public enum ViewAngle { OOV, PERIPHERY, BROAD, MAIN }

    public record EnemyAIStateDetails : APredicativeStateDetails<EnemyAwareness> {
        public BehaviorConfig cfg { get; init; }
        public StateNodeStatus status { get; init; }

        public PlayerStance playerStance { get; init; }
        public PlayerSpeed playerSpeed { get; init; }
        public Visibility playerVisibility { get; init; }
        public ViewDistance playerDistance { get; init; }
        public ViewAngle playerAngle { get; init; }
        public Vector3 lastKnownPosition { get; init; }

        public float unSightedElapsed { get; init; }
        public float unMovedElapsed { get; init; }

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

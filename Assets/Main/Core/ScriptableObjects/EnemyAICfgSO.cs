using OSCore.Data.AI;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/ai")]
    public class EnemyAICfgSO : ScriptableObject {
        [field: SerializeField] public BehaviorConfig passiveCfg { get; private set; }
        [field: SerializeField] public BehaviorConfig alertCfg { get; private set; }
        [field: SerializeField] public BehaviorConfig aggressiveCfg { get; private set; }

        [field: SerializeField] public float speechSpeed { get; private set; }

        [field: Header("Suscipicion Threshholds")]
        [field: SerializeField] public float maxSuspicion { get; private set; }
        [field: SerializeField] public float passiveToCurious { get; private set; }
        [field: SerializeField] public float curiousToInvestigating { get; private set; }

        public EnemyAINode Init() =>
            BuildAsset();

        public BehaviorConfig ActiveCfg(EnemyAwareness awareness) =>
            awareness switch {
                EnemyAwareness.AGGRESIVE => aggressiveCfg,
                EnemyAwareness.SEARCHING => aggressiveCfg,
                EnemyAwareness.ALERT => alertCfg,
                EnemyAwareness.ALERT_INVESTIGATING => alertCfg,
                _ => passiveCfg,
            };

        private EnemyAINode BuildAsset() {
            EnemyAINode passive = new(EnemyAwareness.PASSIVE);
            EnemyAINode return_passive = new(EnemyAwareness.RETURN_PASSIVE);
            EnemyAINode curious = new(EnemyAwareness.CURIOUS);
            EnemyAINode investigating = new(EnemyAwareness.INVESTIGATING);
            EnemyAINode alert = new(EnemyAwareness.ALERT);
            EnemyAINode return_alert = new(EnemyAwareness.RETURN_ALERT);
            EnemyAINode alertInvestigating = new(EnemyAwareness.ALERT_INVESTIGATING);
            EnemyAINode aggressive = new(EnemyAwareness.AGGRESIVE);
            EnemyAINode searching = new(EnemyAwareness.SEARCHING);

            passive
                .To(state => state.suspicion >= passiveToCurious, curious);
            return_passive
                .To(state => state.suspicion >= passiveToCurious, curious)
                .To(state => IsFinished(state.status),
                    passive);
            curious
                .To(state => state.unSightedElapsed > 2f && state.suspicion < 0.1f, passive)
                .To(state => state.suspicion >= curiousToInvestigating, investigating);
            investigating
                .To(state => state.suspicion < 0.1f && IsFinished(state.status), return_passive);

            return passive;
        }

        private static bool IsFinished(StateNodeStatus status) =>
            status == StateNodeStatus.SUCCESS
                || status == StateNodeStatus.FAILURE;
    }
}
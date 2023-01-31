using OSCore.Data.AI;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/ai")]
    public class EnemyAICfgSO : ScriptableObject {
        [field: SerializeField] public StateConfig passiveCfg { get; private set; }
        [field: SerializeField] public StateConfig alertCfg { get; private set; }
        [field: SerializeField] public StateConfig aggressiveCfg { get; private set; }

        [field: SerializeField] public float speechSpeed { get; private set; }

        public EnemyAINode Init() =>
            BuildAsset();

        public StateConfig ActiveCfg(EnemyAwareness awareness) =>
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
                .To(state => state.suspicion >= 0.5f, curious);
            return_passive
                .To(state => state.suspicion >= 0.5f, curious)
                .To(state => state.status == StateNodeStatus.SUCCESS
                        || state.status == StateNodeStatus.FAILURE,
                    passive);
            curious
                .To(state => state.unSightedElapsed > 2f && state.suspicion < 0.1f, return_passive)
                .To(state => state.suspicion >= 2.5f, investigating);
            investigating
                .To(state => state.unSightedElapsed > 5f && state.suspicion < 0.1f, return_passive);

            return passive;
        }
    }
}
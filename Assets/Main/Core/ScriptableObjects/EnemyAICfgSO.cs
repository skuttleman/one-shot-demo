using System;
using OSCore.Data.AI;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/ai")]
    public class EnemyAICfgSO : ScriptableObject {
        public EnemyAINode Init() =>
            BuildAsset();

        private EnemyAINode BuildAsset() {
            EnemyAINode passive = new(EnemyAwareness.PASSIVE);
            EnemyAINode curious = new(EnemyAwareness.CURIOUS);
            EnemyAINode investigating = new(EnemyAwareness.INVESTIGATING);
            EnemyAINode alert = new(EnemyAwareness.ALERT);
            EnemyAINode alertInvestigating = new(EnemyAwareness.ALERT_INVESTIGATING);
            EnemyAINode aggressive = new(EnemyAwareness.AGGRESIVE);
            EnemyAINode searching = new(EnemyAwareness.SEARCHING);

            passive
                .To(state => state.suspicion >= 0.5f, curious);
            curious
                .To(state => state.suspicion < 0.1f, passive)
                .To(state => state.suspicion >= 1f, investigating);

            return passive;
        }
    }
}
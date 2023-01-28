using OSCore.Data.AI;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyAI : APredicativeStateMachine<EnemyAwareness, EnemyAIStateDetails> {
        [SerializeField] private EnemyAICfgSO cfg;
        private AStateNode behavior = null;

        private void Start() {
            IStateReceiver<EnemyAwareness> receiver = Transforms
                .Body(transform)
                .GetComponentInChildren<IStateReceiver<EnemyAwareness>>();
            Init(receiver, cfg.Init(), new() {
                timeInState = 0f,
                suspicion = 0f,
            });
        }

        public void Behave(AStateNode behavior) {
            this.behavior = behavior;
        }

        protected override void Update() {
            base.Update();
            behavior?.Process();
        }
    }
}

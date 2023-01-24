using System;
using OSCore.Data.AI;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.System.Interfaces;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyAI : AStateMachine<EnemyAwareness, EnemyAIStateDetails> {
        [SerializeField] private EnemyAICfgSO cfg;

        private void Start() {
            IStateReceiver<EnemyAwareness> receiver = Transforms
                .Body(transform)
                .GetComponentInChildren<IStateReceiver<EnemyAwareness>>();
            Init(receiver, cfg.Init(), new() {
                timeInState = 0f,
                suspicion = 0f,
            });
        }
    }
}
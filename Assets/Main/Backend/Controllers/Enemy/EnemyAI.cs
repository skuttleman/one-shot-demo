using OSCore.Data.AI;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyAI : APredicativeStateMachine<EnemyAwareness, EnemyAIStateDetails> {
        [SerializeField] private EnemyAICfgSO cfg;

        private void Start() {
            IStateReceiver<EnemyAwareness> receiver = Transforms
                .Body(transform)
                .GetComponentInChildren<IStateReceiver<EnemyAwareness>>();
            Init(receiver, cfg.Init(), new() {
                lastKnownPosition = Vector3.zero,
                timeInState = 0f,
                suspicion = 0f,
                playerStance = PlayerStance.CROUCHING,
                playerSpeed = PlayerSpeed.STOPPED,
                timeSinceSeenPlayer = 0f,
                timeSincePlayerMoved = 0f,
                distanceToPlayer = 1000f,
                angleToPlayer = 0f,
                playerVisibility = 0f,
            });
        }
    }
}

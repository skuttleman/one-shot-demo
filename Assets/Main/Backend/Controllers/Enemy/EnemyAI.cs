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
                timeInState = 0f,
                playerStance = PlayerStance.CROUCHING,
                playerSpeed = PlayerSpeed.STOPPED,
                playerVisibility = Visibility.NONE,
                playerDistance = ViewDistance.OOV,
                playerAngle = ViewAngle.OOV,
                lastKnownPosition = Vector3.zero,

                unSightedElapsed = 0f,
                unMovedElapsed = 0f,

                suspicion = 0f,
            });
        }
    }
}

using OSBE.Controllers.Enemy.Behaviors.Flows;
using OSCore.Data.AI;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using TMPro;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;

namespace OSBE.Controllers.Enemy {
    public class EnemyController :
        ASystemInitializer,
        IController<EnemyControllerInput>,
        IStateReceiver<EnemyAnim>,
        IStateReceiver<EnemyAwareness> {
        [SerializeField] private GameObject footstep;
        [SerializeField] private EnemyAICfgSO cfg;

        private EnemyAI ai;
        private EnemyNavAgent nav;
        private EnemyBehavior behavior;
        private TextMeshPro speech;

        private AStateNode<EnemyAIStateDetails> patrol;
        private AStateNode<EnemyAIStateDetails> activeBehavior;

        public void On(EnemyControllerInput e) {
            behavior.UpdateState(e switch {
                DamageInput => state => state with {
                    timeSinceSeenPlayer = 0f,
                    timeSincePlayerMoved = 0f,
                },
                PlayerLOS ev => state => UpdateLOS(ev, state),
                _ => Fns.Identity
            });
        }

        public void OnStep() {
            if (behavior.state.timeSincePlayerMoved > 0.5f) {
                Instantiate(footstep, transform.position, Quaternion.Euler(90f, 0f, 0f));
            }
        }

        public void OnStateInit(EnemyAwareness curr) {
            Enter(curr);
        }

        public void OnStateTransition(EnemyAwareness prev, EnemyAwareness curr) {
            Enter(curr);
        }

        private EnemyState UpdateLOS(PlayerLOS e, EnemyState details) {
            return details with {
                timeSinceSeenPlayer = e.visibility > 0f ? 0f : details.timeSinceSeenPlayer,
                playerVisibility = e.visibility,
                angleToPlayer = e.periphery,
                distanceToPlayer = e.distance,
            };
        }

        private void Enter(EnemyAwareness awareness) {
            nav.Stop();
            switch (awareness) {
                case EnemyAwareness.PASSIVE:
                    activeBehavior = patrol;
                    break;
                case EnemyAwareness.CURIOUS:
                    activeBehavior = new EnemyCurious(transform);
                    break;
            }
        }

        private StateConfig Cfg(EnemyAwareness awareness) =>
            awareness switch {
                EnemyAwareness.AGGRESIVE => cfg.aggressiveCfg,
                EnemyAwareness.SEARCHING => cfg.aggressiveCfg,
                EnemyAwareness.ALERT => cfg.activeCfg,
                EnemyAwareness.ALERT_INVESTIGATING => cfg.activeCfg,
                _ => cfg.passiveCfg,
            };

        /*
         * Lifecycle Methods
         */

        private void Start() {
            behavior = GetComponent<EnemyBehavior>();
            ai = GetComponent<EnemyAI>();
            nav = GetComponent<EnemyNavAgent>();

            patrol = new TransformPatrol(transform);
            patrol.Init();

            speech = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "speech")
                .First()
                .GetComponent<TextMeshPro>();
            speech.text = "";
        }

        private void Update() {
            activeBehavior?.Process(ai.details with { cfg = Cfg(ai.state) });
        }
    }
}

using OSCore.Data.AI;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using TMPro;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;
using OSBE.Controllers.Enemy.Behaviors.Flows;

namespace OSBE.Controllers.Enemy {
    public class EnemyController :
        ASystemInitializer,
        IController<EnemyControllerInput>,
        IStateReceiver<EnemyAnim>,
        IStateReceiver<EnemyAwareness> {
        [SerializeField] private EnemyCfgSO cfg;
        [SerializeField] GameObject footstep;

        private EnemyAI ai;
        private TextMeshPro speech;
        private TMP_Text debugTxt;
        private EnemyBehavior behavior;
        private AStateNode patrol;

        public void On(EnemyControllerInput e) {
            behavior.UpdateState(e switch {
                DamageInput => state => state with {
                    timeSinceSeenPlayer = 0f,
                    timeSincePlayerMoved = 0f,
                },
                PlayerLOS ev => state => state with {
                    timeSinceSeenPlayer = ev.visibility > 0f ? 0f : state.timeSinceSeenPlayer,
                    playerVisibility = ev.visibility,
                    angleToPlayer = ev.periphery,
                    distanceToPlayer = ev.distance,
                },
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

        private void Enter(EnemyAwareness awareness) {
            switch (awareness) {
                case EnemyAwareness.PASSIVE:
                    ai.Behave(patrol);
                    break;
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            behavior = GetComponent<EnemyBehavior>();
            ai = GetComponent<EnemyAI>();

            patrol = new TransformPatrol(transform);
            patrol.Init();

            speech = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "speech")
                .First()
                .GetComponent<TextMeshPro>();
            speech.text = "";

            debugTxt = system.Send<ITagRegistry, GameObject>(
                registry => registry.GetUnique(IdTag.DEBUG_LAYER))
                .GetComponentInChildren<TMP_Text>();
            debugTxt.text = "";
        }

        private void Update() {
            debugTxt.text = behavior.state.ToString() + "\n\n" + ai.state.ToString();
        }
    }
}

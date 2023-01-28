using OSCore.Data.AI;
using OSCore.Data.Controllers;
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
        IStateReceiver<EnemyAwareness> {
        [SerializeField] private GameObject footstep;
        [SerializeField] private EnemyAICfgSO cfg;

        private GameObject player;
        private EnemyBehavior behavior;
        private TextMeshPro speech;

        public void Handle(EnemyControllerInput e) {
            behavior.UpdateState(e switch {
                DamageInput => HandleDamage,
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
            behavior.SetInterruptState(curr);
        }

        public void OnStateTransition(EnemyAwareness prev, EnemyAwareness curr) {
            behavior.SetInterruptState(curr);
        }

        private EnemyAIStateDetails HandleDamage(EnemyAIStateDetails details) =>
            details with {
                timeSinceSeenPlayer = 0f,
                timeSincePlayerMoved = 0f,
                lastKnownPosition = player.transform.position,
            };

        private EnemyAIStateDetails UpdateLOS(PlayerLOS e, EnemyAIStateDetails details) =>
            details with {
                timeSinceSeenPlayer = e.visibility > 0f ? 0f : details.timeSinceSeenPlayer,
                playerVisibility = e.visibility,
                angleToPlayer = e.periphery,
                distanceToPlayer = e.distance,
            };

        /*
         * Lifecycle Methods
         */

        private void Start() {
            player = system.Player();
            behavior = GetComponent<EnemyBehavior>();

            speech = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "speech")
                .First()
                .GetComponent<TextMeshPro>();
            speech.text = "";
        }
    }
}

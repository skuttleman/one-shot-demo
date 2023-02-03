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
        private TMP_Text debug;

        public void Handle(EnemyControllerInput e) {
            behavior.UpdateState(e switch {
                DamageInput => HandleDamage,
                PlayerLOS ev => state => UpdateLOS(ev, state),
                _ => Fns.Identity
            });
        }

        public void OnStep() {
            if (behavior.details.unMovedElapsed > 0.5f) {
                Instantiate(footstep, transform.position, Quaternion.Euler(90f, 0f, 0f));
            }
        }

        public void OnStateInit(EnemyAwareness curr) {
            behavior.SetInterruptState(curr, curr);
        }

        public void OnStateTransition(EnemyAwareness prev, EnemyAwareness curr) {
            behavior.SetInterruptState(prev, curr);
        }

        private EnemyAIStateDetails HandleDamage(EnemyAIStateDetails details) =>
            details with {
                unSightedElapsed = 0f,
                unMovedElapsed = 0f,
                suspicion = cfg.maxSuspicion,
                lastKnownPosition = player.transform.position,
            };

        private EnemyAIStateDetails UpdateLOS(PlayerLOS e, EnemyAIStateDetails details) {
            BehaviorConfig config = cfg.ActiveCfg(behavior.state);

            return details with {
                cfg = config,
                status = behavior.details.status,
                playerVisibility = CalculatePlayerVisibility(config, e.visibility),
                playerDistance = CalculateViewDistance(config, e.distance),
                playerAngle = CalculateViewAngle(config, e.angle),
            };
        }

        private static Visibility CalculatePlayerVisibility(BehaviorConfig cfg, float visibility) {
            SensorConfig threshholds = cfg.visibility;

            if (visibility < threshholds.limit) return Visibility.NONE;
            if (visibility < threshholds.lowThresh) return Visibility.LOW;
            if (visibility < threshholds.highThresh) return Visibility.MED;
            return Visibility.HIGH;
        }

        private static ViewDistance CalculateViewDistance(BehaviorConfig cfg, float distance) {
            SensorConfig threshholds = cfg.distance;

            if (distance > threshholds.limit) return ViewDistance.OOV;
            if (distance < threshholds.lowThresh) return ViewDistance.NEAR;
            if (distance < threshholds.highThresh) return ViewDistance.MED;
            return ViewDistance.FAR;
        }

        private static ViewAngle CalculateViewAngle(BehaviorConfig cfg, float angle) {
            SensorConfig threshholds = cfg.angle;

            if (angle > threshholds.limit) return ViewAngle.OOV;
            if (angle < threshholds.lowThresh) return ViewAngle.MAIN;
            if (angle < threshholds.highThresh) return ViewAngle.BROAD;

            return ViewAngle.PERIPHERY;
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            player = system.Player();
            behavior = GetComponent<EnemyBehavior>();

            debug = FindObjectOfType<TMP_Text>();
            debug.text = "";
        }

        private void Update() {
            debug.text = $@"

state                     = {GetComponent<EnemyAI>().state}

playerSpeed               = {behavior.details.playerSpeed}
playerVisibility          = {behavior.details.playerVisibility}
playerDistance            = {behavior.details.playerDistance}
playerAngle               = {behavior.details.playerAngle}

suspicion                 = {behavior.details.suspicion}
lastKnownPosition         = {behavior.details.lastKnownPosition}
distance                  = {Vector3.Distance(
                                 behavior.details.lastKnownPosition,
                                 transform.position)}

            ".Trim();
        }
    }
}

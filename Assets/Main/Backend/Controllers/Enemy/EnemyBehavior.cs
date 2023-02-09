using OSBE.Controllers.Enemy.Behaviors.Flows;
using OSCore.Data.AI;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.System.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSBE.Controllers.Enemy {
    public class EnemyBehavior :
        ASystemInitializer,
        IStateReceiver<EnemyAnim> {

        [SerializeField] private EnemyAICfgSO cfg;
        public EnemyAwareness state => ai.state;
        public EnemyAIStateDetails details => ai.details;

        private EnemyAwareness prevAwareness = EnemyAwareness.PASSIVE;

        private EnemyAI ai;
        private GameObject player;
        private IDictionary<EnemyAwareness, ABehaviorNode<EnemyAIStateDetails>> behaviors;

        public EnemyAIStateDetails UpdateState(Func<EnemyAIStateDetails, EnemyAIStateDetails> updateFn) {
            ai.Transition(updateFn);

            return ai.details;
        }

        public void SetInterruptState(EnemyAwareness prev, EnemyAwareness curr) {
            //Debug.Log($"AI transtition from {prev} to {curr}");
        }

        private float CalculateSuspicion(EnemyAIStateDetails details, bool isVisible) {
            BehaviorConfig config = cfg.ActiveCfg(ai.state);
            float increase = ai.state switch {
                EnemyAwareness.PASSIVE => -10f,
                EnemyAwareness.CURIOUS => -10f,
                EnemyAwareness.RETURN_PASSIVE => -10f,
                EnemyAwareness.ALERT => -8f,
                EnemyAwareness.ALERT_CURIOUS => -8f,
                EnemyAwareness.RETURN_ALERT => -8f,
                EnemyAwareness.AGGRESIVE => -5f,
                EnemyAwareness.SEARCHING => -5f,
                _ => 0f
            };

            if (isVisible) {
                increase = config.baseSuspicion;

                increase *= details.playerVisibility switch {
                    Visibility.NONE => 0f,
                    Visibility.LOW => details.playerStance switch {
                        PlayerStance.STANDING => config.vis_MED,
                        PlayerStance.CROUCHING => config.vis_LOW,
                        PlayerStance.CRAWLING => 0f,
                        _=> 1f,
                    },
                    Visibility.MED => (details.playerStance, details.playerSpeed) switch {
                        (PlayerStance.STANDING, _) => 1f,
                        (PlayerStance.CROUCHING, PlayerSpeed.STOPPED) => config.vis_LOW,
                        (PlayerStance.CROUCHING, _) => config.vis_MED,
                        _ => config.vis_LOW,
                    },
                    _ => 1f,
                };

                increase *= details.playerDistance switch {
                    ViewDistance.FAR =>
                        (details.playerStance == PlayerStance.CRAWLING && details.playerSpeed == PlayerSpeed.STOPPED)
                        ? 0f
                        : config.dist_FAR,
                    ViewDistance.MED => config.dist_MED,
                    _ => config.dist_NEAR,
                };

                increase *= details.playerAngle switch {
                    ViewAngle.PERIPHERY => (details.playerDistance, details.playerSpeed) switch {
                        (ViewDistance.FAR, PlayerSpeed.STOPPED) => 0f,
                        (ViewDistance.MED, PlayerSpeed.STOPPED) => 0f,
                        _ => config.angle_PERIPHERY,
                    },
                    ViewAngle.BROAD =>
                        (details.playerDistance == ViewDistance.FAR
                         || details.playerSpeed == PlayerSpeed.STOPPED)
                            ? 0f
                            : config.angle_BROAD,
                    _ => config.angle_MAIN,
                };

                increase *= details.playerSpeed switch {
                    PlayerSpeed.FAST => config.speed_FAST,
                    PlayerSpeed.STOPPED => config.speed_STOPPED,
                    _ => 1f,
                };

                increase *= details.playerStance switch {
                    PlayerStance.STANDING => config.stance_STANDING,
                    PlayerStance.CROUCHING => config.stance_CROUCHING,
                    _ => 1f,
                };
            }

            return Mathf.Clamp(details.suspicion + increase * Time.fixedDeltaTime, 0f, cfg.maxSuspicion);
        }

        private EnemyAIStateDetails ProcessUpdate(EnemyAIStateDetails details) {
            bool isVisible = details.playerVisibility != Visibility.NONE
                && details.playerDistance != ViewDistance.OOV
                && details.playerAngle != ViewAngle.OOV;

            float currSuspicion = details.suspicion;
            float nextSuspicion = CalculateSuspicion(details, isVisible);
            int suspicionChange = nextSuspicion.CompareTo(currSuspicion);

            return details with {
                lastKnownPosition = isVisible && suspicionChange >= 0
                    ? player.transform.position
                    : details.lastKnownPosition,
                suspicion = nextSuspicion,
                suspicionChange = suspicionChange,
            };
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            ai = GetComponent<EnemyAI>();
            player = system.Player();

            behaviors = new Dictionary<EnemyAwareness, ABehaviorNode<EnemyAIStateDetails>>() {
                { EnemyAwareness.PASSIVE, EnemyBehaviors.TransformPatrol(transform).Create(transform) },
                { EnemyAwareness.CURIOUS, EnemyBehaviors.Curious().Create(transform) },
                { EnemyAwareness.INVESTIGATING, EnemyBehaviors.Investigate().Create(transform) },
                { EnemyAwareness.RETURN_PASSIVE, EnemyBehaviors.ReturnToPassive().Create(transform) },

                { EnemyAwareness.ALERT, EnemyBehaviors.TransformPatrol(transform).Create(transform) },
                { EnemyAwareness.ALERT_CURIOUS, EnemyBehaviors.Curious().Create(transform) },
                { EnemyAwareness.ALERT_INVESTIGATING, EnemyBehaviors.Investigate().Create(transform) },
                { EnemyAwareness.RETURN_ALERT, EnemyBehaviors.ReturnToAlert().Create(transform) },

                { EnemyAwareness.AGGRESIVE, EnemyBehaviors.Harrass().Create(transform) },
                { EnemyAwareness.SEARCHING, EnemyBehaviors.SearchHalfHeartedly().Create(transform) }
            };
        }

        private void Update() {
            EnemyAwareness awareness = ai.state;
            ABehaviorNode<EnemyAIStateDetails> behavior = behaviors[awareness];

            if (awareness != prevAwareness) {
                prevAwareness = awareness;
                if (behavior != null) behavior.ReInit();
            }

            UpdateState(state => ProcessUpdate(state) with {
                unMovedElapsed = state.unMovedElapsed + Time.deltaTime,
                unSightedElapsed = state.unSightedElapsed + Time.deltaTime,
                status = behavior?.status ?? StateNodeStatus.INIT,
            });

            if (behavior != null) {
                behavior.Process(ai.details with {
                    cfg = cfg.ActiveCfg(awareness),
                });
            }
        }
    }
}

﻿using OSBE.Controllers.Enemy.Behaviors.Flows;
using OSCore.Data.AI;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.System.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers.Enemy {
    public class EnemyBehavior :
        ASystemInitializer<MovementChanged, StanceChanged>,
        IStateReceiver<EnemyAnim> {

        [SerializeField] private EnemyAICfgSO cfg;
        public EnemyAwareness state => ai.state;
        public EnemyAIStateDetails details => ai.details;

        [SerializeField] private EnemyAwareness awareness;
        private EnemyAwareness prevAwareness = EnemyAwareness.PASSIVE;

        private EnemyAI ai;
        private GameObject player;
        private IDictionary<EnemyAwareness, ABehaviorNode<EnemyAIStateDetails>> behaviors;

        public EnemyAIStateDetails UpdateState(Func<EnemyAIStateDetails, EnemyAIStateDetails> updateFn) {
            ai.Transition(updateFn);

            return ai.details;
        }

        public void SetInterruptState(EnemyAwareness prev, EnemyAwareness curr) {
            Debug.Log($"AI transtition from {prev} to {curr}");
        }

        protected override void OnEvent(MovementChanged e) {
            UpdateState(state => state with {
                playerSpeed = e.speed,
                unMovedElapsed = e.speed != PlayerSpeed.STOPPED
                    ? 0f
                    : state.unMovedElapsed,
            });
        }

        protected override void OnEvent(StanceChanged e) {
            UpdateState(state => state with { playerStance = e.stance });
        }

        private float CalculateSuspicion(EnemyAIStateDetails details, bool isVisible) {
            BehaviorConfig config = cfg.ActiveCfg(ai.state);
            float increase = -10f;

            if (isVisible) {
                increase = config.baseSuspicion;

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

                increase *= details.playerVisibility switch {
                    Visibility.LOW => config.vis_LOW,
                    Visibility.MED => config.vis_MED,
                    _ => 1f,
                };

                increase *= details.playerDistance switch {
                    ViewDistance.NEAR => config.dist_NEAR,
                    ViewDistance.MED => config.dist_MED,
                    ViewDistance.FAR => config.dist_FAR,
                    _ => 1f,
                };

                increase *= details.playerAngle switch {
                    ViewAngle.MAIN => 1f,
                    ViewAngle.BROAD => 0.5f,
                    ViewAngle.PERIPHERY => 0.25f,
                    _ => 1f,
                };
            }

            return Mathf.Clamp(details.suspicion + increase * Time.fixedDeltaTime, 0f, cfg.maxSuspicion);
        }

        private EnemyAIStateDetails ProcessUpdate(EnemyAIStateDetails details) {
            bool isVisible = details.playerVisibility != Visibility.NONE
                && details.playerDistance != ViewDistance.OOV
                && details.playerAngle != ViewAngle.OOV;

            return details with {
                lastKnownPosition = isVisible
                    ? player.transform.position
                    : details.lastKnownPosition,
                //lastKnownPosition = player.transform.position,
                suspicion = CalculateSuspicion(details, isVisible),
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




                //{ EnemyAwareness.PASSIVE, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.CURIOUS, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.INVESTIGATING, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.RETURN_PASSIVE, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.RETURN_PASSIVE_GIVE_UP, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },

                //{ EnemyAwareness.ALERT, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.ALERT_CURIOUS, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.ALERT_INVESTIGATING, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.RETURN_ALERT, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },

                //{ EnemyAwareness.AGGRESIVE, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop },
                //{ EnemyAwareness.SEARCHING, EnemyBehaviors.Noop<EnemyAIStateDetails>.noop }
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

using OSBE.Controllers.Enemy.Behaviors.Actions;
using OSBE.Controllers.Enemy.Behaviors.Composites;
using OSBE.Controllers.Enemy.Behaviors.Main;
using OSCore.Data.AI;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System;
using OSCore.System.Interfaces;
using System;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers.Enemy {
    public class EnemyBehavior :
        ASystemInitializer<MovementChanged, StanceChanged>,
        IStateReceiver<EnemyAnim> {

        [SerializeField] private EnemyAICfgSO cfg;
        public EnemyAwareness state => ai.state;
        public EnemyAIStateDetails details => ai.details;

        private EnemyAI ai;
        private EnemyNavAgent nav;
        private GameObject player;

        private AStateNode<EnemyAIStateDetails> patrol;
        private AStateNode<EnemyAIStateDetails> behavior;

        public EnemyAIStateDetails UpdateState(Func<EnemyAIStateDetails, EnemyAIStateDetails> updateFn) {
            ai.Transition(updateFn);

            return ai.details;
        }

        public void AssignPatrol(AStateNode<EnemyAIStateDetails> patrol) {
            this.patrol = patrol;
        }

        public void AssignInterruptBehavior(AStateNode<EnemyAIStateDetails> behavior) {
            this.behavior = behavior;
        }

        public void SetInterruptState(EnemyAwareness awareness) {
            if (nav != null) nav.Stop();
            if (patrol == null) AssignPatrol(EnemyBehaviors.TransformPatrol(transform));

            switch (awareness) {
                case EnemyAwareness.PASSIVE:
                    AStateNode<EnemyAIStateDetails>.ReInit(patrol);
                    AssignInterruptBehavior(patrol);
                    break;
                case EnemyAwareness.RETURN_PASSIVE:
                    AssignInterruptBehavior(EnemyBehaviors.ReturnToPassive(transform));
                    break;
                case EnemyAwareness.CURIOUS:
                    AssignInterruptBehavior(EnemyBehaviors.Curious(transform));
                    break;
                case EnemyAwareness.INVESTIGATING:
                    AssignInterruptBehavior(EnemyBehaviors.Investigate(transform));
                    break;
            }
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
            float increase = -1f;

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
                suspicion = CalculateSuspicion(details, isVisible),
            };
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            ai = GetComponent<EnemyAI>();
            nav = GetComponent<EnemyNavAgent>();

            player = system.Player();
        }

        private void Update() {
            UpdateState(state => ProcessUpdate(state) with {
                unMovedElapsed = state.unMovedElapsed + Time.deltaTime,
                unSightedElapsed = state.unSightedElapsed + Time.deltaTime,
                status = behavior?.status ?? StateNodeStatus.INIT,
            });

            AStateNode<EnemyAIStateDetails>.Process(
                behavior,
                ai.details with {
                    cfg = cfg.ActiveCfg(ai.state),
                });
        }
    }
}

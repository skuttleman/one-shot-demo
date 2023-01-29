using OSBE.Controllers.Enemy.Behaviors.Flows;
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
        private EnemySpeechAgent speech;
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

            if (patrol == null) {
                AssignPatrol(new TransformPatrol(transform));
                patrol.Init();
            }

            switch (awareness) {
                case EnemyAwareness.PASSIVE:
                    AssignInterruptBehavior(patrol);
                    break;
                case EnemyAwareness.CURIOUS:
                    AssignInterruptBehavior(new EnemyCurious(transform));
                    break;
                case EnemyAwareness.INVESTIGATING:
                    AssignInterruptBehavior(new EnemyInvestigating(transform));
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
            StateConfig config = cfg.ActiveCfg(ai.state);
            float increase = -1f;

            if (isVisible) {
                increase = config.suspicionIncrease;

                increase *= details.playerSpeed switch {
                    PlayerSpeed.FAST => 2f,
                    PlayerSpeed.STOPPED => 0.2f,
                    _ => 1f,
                };

                increase *= details.playerStance switch {
                    PlayerStance.STANDING => 5f,
                    PlayerStance.CROUCHING => 3f,
                    _ => 1f,
                };

                increase *= details.playerVisibility switch {
                    Visibility.LOW => 0.33f,
                    Visibility.MED => 0.66f,
                    _ => 1f,
                };

                increase *= details.playerDistance switch {
                    ViewDistance.NEAR => 0.8f,
                    ViewDistance.MED => 0.5f,
                    ViewDistance.FAR => 0.2f,
                    _ => 1f,
                };

                increase *= details.playerAngle switch {
                    ViewAngle.MAIN => 1f,
                    ViewAngle.BROAD => 0.5f,
                    ViewAngle.PERIPHERY => 0.25f,
                    _ => 1f,
                };
            }

            return Mathf.Clamp(details.suspicion + increase * Time.fixedDeltaTime, 0f, 10f);
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
            speech = GetComponent<EnemySpeechAgent>();

            player = system.Player();
        }

        private void Update() {
            AStateNode<EnemyAIStateDetails>.Process(
                behavior,
                ai.details with {
                    cfg = cfg.ActiveCfg(ai.state),
                });

            UpdateState(state => ProcessUpdate(state) with {
                unMovedElapsed = state.unMovedElapsed + Time.deltaTime,
                unSightedElapsed = state.unSightedElapsed + Time.deltaTime,
            });
        }
    }
}

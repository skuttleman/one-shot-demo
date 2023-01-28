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
        public EnemyAIStateDetails state => ai.details;

        private EnemyAI ai;
        private EnemyNavAgent nav;
        private GameObject player;

        private AStateNode<EnemyAIStateDetails> patrol;
        private AStateNode<EnemyAIStateDetails> behavior;

        public EnemyAIStateDetails UpdateState(Func<EnemyAIStateDetails, EnemyAIStateDetails> updateFn) {
            ai.Transition(updateFn);

            return ai.details;
        }

        public void AssignPatro(AStateNode<EnemyAIStateDetails> patrol) {
            this.patrol = patrol;
        }

        public void AssignInterruptBehavior(AStateNode<EnemyAIStateDetails> behavior) {
            this.behavior = behavior;
        }

        public void SetInterruptState(EnemyAwareness awareness) {
            if (nav != null) nav.Stop();

            switch (awareness) {
                case EnemyAwareness.PASSIVE:
                    behavior = patrol;
                    break;
                case EnemyAwareness.CURIOUS:
                    behavior = new EnemyCurious(transform);
                    break;
            }
        }

        protected override void OnEvent(MovementChanged e) {
            UpdateState(state => state with {
                playerSpeed = e.speed,
                timeSincePlayerMoved = e.speed != PlayerSpeed.STOPPED
                    ? 0f
                    : state.timeSincePlayerMoved,
            });
        }

        protected override void OnEvent(StanceChanged e) {
            UpdateState(state => state with { playerStance = e.stance });
        }

        private float CalculateSuspicion(EnemyAIStateDetails details) {
            StateConfig config = cfg.ActiveCfg(ai.state);
            float increase = config.suspicionIncrease;

            increase *= state.playerSpeed switch {
                PlayerSpeed.FAST => 5f,
                PlayerSpeed.SLOW => 2f,
                PlayerSpeed.STOPPED => 0.2f,
                _ => 1f,
            };

            increase *= state.playerStance switch {
                PlayerStance.STANDING => 5f,
                PlayerStance.CROUCHING => 2f,
                _ => 1f,
            };

            float periphery = state.angleToPlayer / config.fovAngle;
            if (periphery > 0 && state.angleToPlayer <= config.fovAngle) increase /= periphery;
            else increase = 0;

            increase *= state.playerVisibility;
            if (state.distanceToPlayer <= config.fovDistance) {
                increase /= state.distanceToPlayer;
            }

            return Mathf.Clamp(details.suspicion + increase * Time.fixedDeltaTime, 0f, 1f);
        }

        private EnemyAIStateDetails ProcessUpdate(EnemyAIStateDetails details) {
            bool isVisible = state.playerVisibility > 0f;

            return details with {
                lastKnownPosition = isVisible
                    ? player.transform.position
                    : details.lastKnownPosition,
                suspicion = isVisible
                    ? CalculateSuspicion(details)
                    : Mathf.Max(details.suspicion - 0.01f, 0f),
            };
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            ai = GetComponent<EnemyAI>();
            nav = GetComponent<EnemyNavAgent>();
            player = system.Player();

            patrol = new TransformPatrol(transform);
            patrol.Init();

        }

        private void FixedUpdate() {
            AStateNode<EnemyAIStateDetails>.Process(
                behavior,
                ai.details with { cfg = cfg.ActiveCfg(ai.state) });

            UpdateState(state => state with {
                timeSincePlayerMoved = state.timeSincePlayerMoved + Time.fixedDeltaTime,
                timeSinceSeenPlayer = state.timeSinceSeenPlayer + Time.fixedDeltaTime,
            });

            ai.Transition(ProcessUpdate);
        }
    }
}

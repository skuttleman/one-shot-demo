using OSCore.Data.AI;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System;
using System;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers.Enemy {
    public class EnemyBehavior : ASystemInitializer<MovementChanged, StanceChanged> {
        [SerializeField] private EnemyAICfgSO cfg;
        public EnemyState state { get; private set; }

        private EnemyAI ai;
        private GameObject player;

        public EnemyState UpdateState(Func<EnemyState, EnemyState> updateFn) =>
            state = updateFn(state);

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
            StateConfig cfg = Cfg(ai.state);
            float increase = 0.1f;

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

            float periphery = state.angleToPlayer / cfg.fovAngle;
            Debug.Log("PERIPH " + periphery);
            if (periphery > 0 && state.angleToPlayer <= cfg.fovAngle) increase /= periphery;
            else increase = 0;

            increase *= state.playerVisibility;
            if (state.distanceToPlayer <= cfg.fovDistance) {
                increase /= state.distanceToPlayer;
            }

            return Mathf.Clamp(details.suspicion + increase, 0f, 1f);
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
            ai = GetComponent<EnemyAI>();
            player = system.Player();

            state = new() {
                playerStance = PlayerStance.CROUCHING,
                playerSpeed = PlayerSpeed.STOPPED,
                timeSinceSeenPlayer = 0f,
                timeSincePlayerMoved = 0f,
                distanceToPlayer = 1000f,
                angleToPlayer = 0f,
                playerVisibility = 0f,
            };
        }

        private void FixedUpdate() {
            UpdateState(state => state with {
                timeSincePlayerMoved = state.timeSincePlayerMoved + Time.fixedDeltaTime,
                timeSinceSeenPlayer = state.timeSinceSeenPlayer + Time.fixedDeltaTime,
            });

            bool playerInView = state.playerVisibility > 0f;

            ai.Transition(details => details with {
                seesPlayer = playerInView,
                lastKnownPosition = playerInView
                    ? player.transform.position
                    : details.lastKnownPosition,
                suspicion = playerInView
                    ? CalculateSuspicion(details)
                    : Mathf.Max(details.suspicion - 0.01f, 0f),
            });
        }
    }
}

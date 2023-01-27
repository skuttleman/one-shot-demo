using OSCore.Data.Enums;
using OSCore.Data;
using System;
using UnityEngine;
using OSCore.System;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers.Enemy {
    public interface IAIBehavior { }

    public class EnemyBehavior : ASystemInitializer<MovementChanged, StanceChanged>, IAIBehavior {
        public EnemyState state { get; private set; }

        private EnemyAI ai;

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
            float increase = 0.1f;

            increase = state.playerSpeed switch {
                PlayerSpeed.FAST => increase * 5f,
                PlayerSpeed.SLOW => increase * 2f,
                PlayerSpeed.STOPPED => 0.02f,
                _ => increase,
            };

            increase = state.playerStance switch {
                PlayerStance.STANDING => increase * 5f,
                PlayerStance.CROUCHING => increase * 2f,
                _ => increase,
            };

            float periphery = state.angleToPlayer * 2f + 1f;
            if (periphery > 0) increase /= periphery;
            else increase = 0;
            increase /= state.distanceToPlayer;
            increase *= state.playerVisibility;

            return Mathf.Clamp(details.suspicion + increase, 0f, 1f);
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            ai = GetComponent<EnemyAI>();

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

            ai.Transition(details => details with {
                suspicion = state.playerVisibility > 0f
                    ? CalculateSuspicion(details)
                    : Mathf.Max(details.suspicion - 0.01f, 0f),
            });
        }
    }
}

using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using System;
using TMPro;
using UnityEngine.AI;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;
using OSCore.Data.AI;

namespace OSBE.Controllers.Enemy {
    public class EnemyController :
        ASystemInitializer<MovementChanged, StanceChanged>,
        IController<EnemyControllerInput>,
        IStateReceiver<EnemyAnim>,
        IStateReceiver<EnemyAwareness> {
        [SerializeField] private EnemyCfgSO cfg;
        [SerializeField] GameObject footstep;
        private static readonly float SEEN_THRESHOLD = 5f;

        private GameObject player;
        private EnemyAnimator anim;
        private EnemyAI ai;
        private TextMeshPro speech;
        private TextMeshPro awareness;
        private NavMeshAgent nav;
        private EnemyState state = null;

        public void On(EnemyControllerInput e) {
            UpdateState(e switch {
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
            if (state.timeSincePlayerMoved > 0.5f) {
                Instantiate(footstep, transform.position, Quaternion.Euler(90f, 0f, 0f));
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

        protected override void OnEnable() {
            base.OnEnable();
            player = system.Player();
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            anim = GetComponentInChildren<EnemyAnimator>();
            ai = GetComponentInChildren<EnemyAI>();
            nav = GetComponent<NavMeshAgent>();
            nav.updateRotation = false;
            speech = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "speech")
                .First()
                .GetComponent<TextMeshPro>();
            awareness = Transforms
                .FindInActiveChildren(transform.parent, xform => xform.name == "awareness")
                .First()
                .GetComponent<TextMeshPro>();
            speech.text = "";
            awareness.text = "";

            state = new() {
                playerStance = PlayerStance.CROUCHING,
                playerSpeed = PlayerSpeed.STOPPED,
                timeSinceSeenPlayer = 0f,
                timeSincePlayerMoved = 0f,
                distanceToPlayer = 1000f,
                angleToPlayer = 2f,
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

            awareness.text = (state ?? new()).ToString() + "\n\n" + ai.state.ToString();
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
            increase /= periphery;
            increase /= state.distanceToPlayer;
            increase *= state.playerVisibility;

            return Mathf.Clamp(details.suspicion + increase, 0f, 1f);
        }

        private EnemyState UpdateState(Func<EnemyState, EnemyState> updateFn) =>
            state = updateFn(state);
    }
}

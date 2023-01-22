using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using TMPro;
using UnityEngine.AI;
using UnityEngine;
using static OSCore.Data.Controllers.EnemyControllerInput;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers.Enemy {
    public class EnemyController : ASystemInitializer<MovementChanged, StanceChanged>, IController<EnemyControllerInput>, IStateReceiver<EnemyAnim> {
        [SerializeField] private EnemyCfgSO cfg;
        [SerializeField] GameObject footstep;
        private static readonly float SEEN_THRESHOLD = 5f;

        private GameObject player;
        private EnemyAnimator anim;
        private TextMeshPro speech;
        private TextMeshPro awareness;
        private NavMeshAgent nav;
        private EnemyState state = null;

        public void On(EnemyControllerInput e) {
            switch (e) {
                case DamageInput:
                    state = state with {
                        timeSinceSeenPlayer = 0f,
                        timeSincePlayerMoved = 0f,
                    };
                    break;
                case PlayerLOS ev:
                    state = state with {
                        timeSinceSeenPlayer = ev.visibility > 0f ? 0f: state.timeSinceSeenPlayer,
                        playerVisibility = ev.visibility,
                        angleToPlayer = ev.periphery,
                        distanceToPlayer = ev.distance,
                    };
                    break;
            }
        }

        public void OnStep() {
            if (state.timeSincePlayerMoved > 0.5f) {
                Instantiate(footstep, transform.position, Quaternion.Euler(90f, 0f, 0f));
            }
        }

        protected override void OnEvent(MovementChanged e) {
            state = state with {
                playerSpeed = e.speed,
                timeSincePlayerMoved = e.speed != PlayerSpeed.STOPPED
                    ? 0f
                    : state.timeSincePlayerMoved,
            };
        }

        protected override void OnEvent(StanceChanged e) {
            state = state with { playerStance = e.stance };
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
            awareness.text = (state ?? new()).ToString();

            state = state with {
                timeSincePlayerMoved = state.timeSincePlayerMoved + Time.fixedDeltaTime,
                timeSinceSeenPlayer = state.timeSinceSeenPlayer + Time.fixedDeltaTime,
            };
        }
    }
}

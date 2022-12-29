using OSBE.Controllers.Player.Interfaces;
using OSBE.Controllers.Player;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data.Events;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;
using UnityEngine;
using static OSCore.Data.Events.Controllers.Player.AnimationEmittedEvent;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer,
        IController<PlayerControllerInput>,
        IPlayerMainController,
        IStateReceiver<PlayerAnim> {
        [SerializeField] private PlayerCfgSO cfg;
        [SerializeField] private Transform tbdEffect;
        [SerializeField] private Transform tbdEffect2;

        private PlayerAnimator anim;
        private PlayerInput input;
        private IDictionary<PlayerInputControlMap, IPlayerInputController> controllers;
        public PlayerControllerState state { get; private set; } = new() {
            anim = PlayerAnim.crouch_idle,
            controls = PlayerInputControlMap.Standard,
            mouseLookTimer = 0f,
            tbdTimer = 0f,
            stance = PlayerStance.STANDING,
            attackMode = AttackMode.HAND,
            isMoving = false,
            isScoping = false,
            isGrounded = true,
            movement = Vector2.zero,
            facing = Vector2.zero,
            hangingPoint = Vector3.zero,
            ledge = default,
        };

        private Rigidbody rb;
        private GameObject stand;
        private GameObject crouch;
        private GameObject crawl;

        public void On(PlayerControllerInput e) {
            Controller().On(e);
        }

        public void Publish(IEvent e) {
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            if (prev.ToString().StartsWith("hang") && !curr.ToString().StartsWith("hang"))
                rb.isKinematic = false;

            PlayerControllerState prevState = state;
            UpdateState(state => ControllerUtils.TransitionControllerState(prev, curr, state));
            ManageAnim(prevState, prev, curr);

            Controller().OnStateTransition(prev, curr);

            if (state.controls != PlayerInputControlMap.None) {
                string controls = state.controls.ToString();
                if (input.currentActionMap.name != controls)
                    input.SwitchCurrentActionMap(controls);
            }
        }

        public PlayerControllerState UpdateState(Func<PlayerControllerState, PlayerControllerState> updateFn) {
            PlayerControllerState nextState = updateFn(state);

            if (nextState.tbdTimer == cfg.tbdCooldown && state.tbdTimer != nextState.tbdTimer) {
                StartCoroutine(InitiateTBD());
            }

            bool controlsChanged = state.controls != nextState.controls;
            if (controlsChanged) Controller().OnDeactivate();
            state = nextState;
            if (controlsChanged) Controller().OnActivate();

            return nextState;
        }

        private void ManageAnim(PlayerControllerState prevState, PlayerAnim prev, PlayerAnim curr) {
            anim.Transition(state => ControllerUtils.TransitionAnimState(prev, curr, state));

            switch (curr) {
                case PlayerAnim.stand_move:
                case PlayerAnim.crouch_move:
                case PlayerAnim.crouch_move_aim:
                case PlayerAnim.crouch_move_bino:
                case PlayerAnim.crawl_move:
                    anim.SetSpeed(1f);
                    break;
            }

            PublishChanged(prevState.stance, state.stance, new StanceChanged(state.stance));
            PublishChanged(prevState.attackMode, state.attackMode, new AttackModeChanged(state.attackMode));
            PublishChanged(prevState.isScoping, state.isScoping, new ScopingChanged(state.isScoping));
            ActivateStance();
        }

        private IPlayerInputController Controller() =>
            controllers.Get(state.controls);

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue))
                system.Send<IPubSub>(pubsub => pubsub.Publish(e));
        }

        private void ActivateStance() {
            stand.SetActive(state.stance == PlayerStance.STANDING);
            crouch.SetActive(state.stance == PlayerStance.CROUCHING);
            crawl.SetActive(state.stance == PlayerStance.CRAWLING);
        }

        private GameObject FindStance(string name) =>
            Transforms
                .FindInChildren(transform, xform => xform.name == name)
                .First()
                .gameObject;

        private IEnumerator<YieldInstruction> InitiateTBD() {
            while (Time.timeScale > cfg.tbdMinTime) {
                Time.timeScale = Mathf.Max(cfg.tbdMinTime, Time.timeScale - (cfg.tbdTransitionSpeed * (1 - Time.deltaTime)));
                tbdEffect.localScale = Vector3.Lerp(new(0, 0, 0), new(20, 20, 20), 1 - Time.timeScale);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(cfg.tbdMinTimeDuration);

            while (Time.timeScale < cfg.tbdMaxTime) {
                Time.timeScale = Mathf.Min(cfg.tbdMaxTime, Time.timeScale + (cfg.tbdTransitionSpeed * (1 - Time.deltaTime)));
                tbdEffect2.localScale = Vector3.Lerp(
                    new(0, 0, 0),
                    new(20, 20, 20),
                    1 - (cfg.tbdMaxTime - Time.timeScale));
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(cfg.tbdMaxTimeDuration);

            while (Time.timeScale > 1f) {
                Time.timeScale = Mathf.Max(1f, Time.timeScale - (cfg.tbdTransitionSpeed * (1 - Time.deltaTime)));
                tbdEffect.localScale = Vector3.Lerp(new(0, 0, 0), new(20, 20, 20), (Time.timeScale  - 1) / (cfg.tbdMaxTime - 1));
                tbdEffect2.localScale = Vector3.Lerp(new(0, 0, 0), new(20, 20, 20), (Time.timeScale  - 1) / (cfg.tbdMaxTime - 1));
                yield return new WaitForEndOfFrame();
            }
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            controllers = new Dictionary<PlayerInputControlMap, IPlayerInputController>() {
                { PlayerInputControlMap.None, new NoopInputController() },
                { PlayerInputControlMap.Standard, new StandardInputController(this, system, cfg, transform) },
                { PlayerInputControlMap.LedgeHang, new LedgeHangInputController(this, cfg, transform) }
            };

            anim = GetComponentInChildren<PlayerAnimator>();
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();

            stand = FindStance("stand");
            crouch = FindStance("crouch");
            crawl = FindStance("crawl");
        }

        private void Update() {
            if (Vectors.NonZero(state.facing)) {
                float rotationY = Vectors.AngleTo(Vector2.zero, state.facing);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, -rotationY, 0f),
                    cfg.crouching.rotationSpeed * Time.deltaTime);
            }

            Controller().OnUpdate();
        }

        private void FixedUpdate() =>
            Controller().OnFixedUpdate();
    }

    internal class NoopInputController : IPlayerInputController {
        public void On(PlayerControllerInput e) { }
    }
}

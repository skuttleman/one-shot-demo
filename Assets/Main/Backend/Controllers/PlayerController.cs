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
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSBE.Controllers {
    public class PlayerController : ASystemInitializer,
        IController<PlayerControllerInput>,
        IPlayerMainController,
        IStateReceiver<PlayerAnim> {
        [SerializeField] private PlayerCfgSO cfg;

        private PlayerSharedInputState state = new() {
            anim = PlayerAnim.crouch_idle,
            controls = PlayerInputControlMap.Standard,
            rotation = Vector2.zero,
        };
        private PlayerAnimator anim;
        private PlayerInput input;
        private IDictionary<PlayerInputControlMap, IPlayerInputController> controllers;

        private Rigidbody rb;

        public void On(PlayerControllerInput e) =>
            Controller().On(e);

        public void Publish(IEvent e) =>
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));

        public void OnStateExit(PlayerAnim curr) {
            switch (curr) {
                case PlayerAnim.stand_move:
                case PlayerAnim.crouch_move:
                case PlayerAnim.crouch_move_aim:
                case PlayerAnim.crouch_move_bino:
                case PlayerAnim.crawl_move:
                    anim.SetSpeed(1f);
                    break;
            }

            Controller().OnStateExit(curr);
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            if (prev.ToString().StartsWith("hang") && !curr.ToString().StartsWith("hang")) {
                rb.isKinematic = false;
                UpdateState(state => state with {
                    controls = PlayerInputControlMap.Standard,
                });
            }

            Controller().OnStateTransition(prev, curr);
        }

        public void OnStateEnter(PlayerAnim anim) {
            PlayerSharedInputState prevState = state;
            UpdateState(state => ControllerUtils.TransitionState(state, anim));

            if (state.controls != PlayerInputControlMap.None) {
                string controls = state.controls.ToString();
                if (input.currentActionMap.name != controls)
                    input.SwitchCurrentActionMap(controls);
            }

            Controller().OnStateEnter(anim);
        }

        public IPlayerMainController Notify(PlayerControllerInput msg) {
            switch (msg) {
                case Facing ev:
                    UpdateState(state => state with { rotation = ev.direction });
                    break;
            }

            controllers.Values.ForEach(controller =>
                controller.On(msg));

            return this;
        }

        private void Start() {
            controllers = new Dictionary<PlayerInputControlMap, IPlayerInputController>() {
                { PlayerInputControlMap.None, new NoopInputController() },
                { PlayerInputControlMap.Standard, new StandardInputController(this, system, cfg, transform) },
                { PlayerInputControlMap.LedgeHang, new LedgeHangInputController(this, cfg, transform) }
            };
            anim = GetComponentInChildren<PlayerAnimator>();
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();
        }

        private void Update() {
            if (Vectors.NonZero(state.rotation)) {
                float rotationZ = Vectors.AngleTo(Vector2.zero, state.rotation);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    cfg.crouching.rotationSpeed * Time.deltaTime);
            }

            Controller().OnUpdate(state);
        }

        private void FixedUpdate() =>
            Controller().OnFixedUpdate(state);

        private IPlayerInputController Controller() =>
            controllers.Get(state.controls);

        private PlayerSharedInputState UpdateState(Func<PlayerSharedInputState, PlayerSharedInputState> updateFn) {
            PlayerSharedInputState nextState = updateFn(state);

            bool controlsChanged = state.controls != nextState.controls;
            if (controlsChanged) Controller().OnDeactivate(state);
            state = nextState;
            if (controlsChanged) Controller().OnActivate(state);

            return state;
        }
    }

    internal class NoopInputController : IPlayerInputController {
        public void On(PlayerControllerInput e) { }
    }
}

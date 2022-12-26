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

        private PlayerControllerState state = new() {
            anim = PlayerAnim.crouch_idle,
            controls = PlayerInputControlMap.Standard,
            mouseLookTimer = 0f,
            stance = PlayerStance.STANDING,
            attackMode = AttackMode.HAND,
            isMoving = false,
            isSprinting = false,
            isScoping = false,
            isGrounded = true,
            movement = Vector2.zero,
            facing = Vector2.zero,
            hangingPoint = Vector3.zero,
            ledge = default,
        };
        private PlayerAnimator anim;
        private PlayerInput input;
        private IDictionary<PlayerInputControlMap, IPlayerInputController> controllers;

        private Rigidbody rb;

        public void On(PlayerControllerInput e) {
            Controller().On(e);
        }

        public void Publish(IEvent e) {
            system.Send<IPubSub>(pubsub => pubsub.Publish(e));
        }

        public void OnStateTransition(PlayerAnim prev, PlayerAnim curr) {
            switch (prev) {
                case PlayerAnim.stand_move:
                case PlayerAnim.crouch_move:
                case PlayerAnim.crouch_move_aim:
                case PlayerAnim.crouch_move_bino:
                case PlayerAnim.crawl_move:
                    anim.SetSpeed(1f);
                    break;
            }


            if (prev.ToString().StartsWith("hang") && !curr.ToString().StartsWith("hang")) {
                rb.isKinematic = false;
                UpdateState(state => state with {
                    controls = PlayerInputControlMap.Standard,
                });
            }

            Controller().OnStateTransition(prev, curr);


            PlayerControllerState prevState = state;
            UpdateState(state => ControllerUtils.TransitionState(state, curr));

            if (state.controls != PlayerInputControlMap.None) {
                string controls = state.controls.ToString();
                if (input.currentActionMap.name != controls)
                    input.SwitchCurrentActionMap(controls);
            }

            switch (curr) {
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
        }

        public PlayerControllerState UpdateState(Func<PlayerControllerState, PlayerControllerState> updateFn) {
            PlayerControllerState nextState = updateFn(state);

            bool controlsChanged = state.controls != nextState.controls;
            if (controlsChanged) Controller().OnDeactivate(state);
            state = nextState;
            if (controlsChanged) Controller().OnActivate(state);

            return state;
        }

        private IPlayerInputController Controller() =>
            controllers.Get(state.controls);

        private void PublishChanged<T>(T oldValue, T newValue, IEvent e) {
            if (!oldValue.Equals(newValue))
                system.Send<IPubSub>(pubsub => pubsub.Publish(e));
        }

        /*
         * Lifecycle Methods
         */

        private void Start() {
            controllers = new Dictionary<PlayerInputControlMap, IPlayerInputController>() {
                { PlayerInputControlMap.None, new NoopInputController() },
                { PlayerInputControlMap.Standard, new StandardInputController(this, system, cfg, transform, state) },
                { PlayerInputControlMap.LedgeHang, new LedgeHangInputController(this, cfg, transform) }
            };
            anim = GetComponentInChildren<PlayerAnimator>();
            input = GetComponent<PlayerInput>();
            rb = GetComponent<Rigidbody>();
        }

        private void Update() {
            if (Vectors.NonZero(state.facing)) {
                float rotationZ = Vectors.AngleTo(Vector2.zero, state.facing);
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    cfg.crouching.rotationSpeed * Time.deltaTime);
            }

            Controller().OnUpdate(state);
        }

        private void FixedUpdate() =>
            Controller().OnFixedUpdate(state);
    }

    internal class NoopInputController : IPlayerInputController {
        public void On(PlayerControllerInput e) { }
    }
}

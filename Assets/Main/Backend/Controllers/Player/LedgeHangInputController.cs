using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.Utils;
using System;
using UnityEngine;
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSBE.Controllers.Player {
    public class LedgeHangInputController : IPlayerInputController {
        private readonly IPlayerMainController controller;
        private readonly PlayerCfgSO cfg;
        private readonly Transform transform;
        private readonly PlayerAnimator anim;

        private PlayerLedgeHangingInputState state = new() {
            hangingPoint = Vector3.zero,
            ledge = default,
        };

        public LedgeHangInputController(IPlayerMainController controller, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.cfg = cfg;
            this.transform = transform;
            anim = transform.GetComponentInChildren<PlayerAnimator>();
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case LedgeTransition ev:
                    UpdateState(state => state with {
                        ledge = ev.ledge,
                        hangingPoint = ev.pt,
                    });
                    break;
                case ClimbInput ev:
                    if (ev.direction == ClimbDirection.UP) OnClimbUp();
                    else OnClimbDown();
                    break;
                case MovementInput ev:
                    break;
            }
        }

        public void OnUpdate(PlayerSharedInputState common) {
            RotatePlayer(common);
        }

        private void RotatePlayer(PlayerSharedInputState common) {
            
        }

        public void OnActivate(PlayerSharedInputState state) {
            controller.Notify(new Facing(Vector2.zero));
        }

        public void OnStateEnter(PlayerAnim anim) {
            state = ControllerUtils.TransitionState(state, anim);
        }

        private void OnClimbUp() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_CLIMB;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                Vector3 diff = (state.hangingPoint - transform.position) * 1.2f;
                transform.position += diff;
                controller.Notify(new Facing(Vector2.zero));
            }
        }

        private void OnClimbDown() {
            PlayerAnimSignal signal = PlayerAnimSignal.LEDGE_DROP;
            if (anim.CanTransition(signal)) {
                anim.Send(signal);

                float pointZ = state.hangingPoint.z;
                transform.position = transform.position.WithZ(pointZ + 0.55f);
                controller.Notify(new Facing(Vector2.zero));
            }
        }

        private PlayerLedgeHangingInputState UpdateState(Func<PlayerLedgeHangingInputState, PlayerLedgeHangingInputState> updateFn) =>
            state = updateFn(state);
    }
}

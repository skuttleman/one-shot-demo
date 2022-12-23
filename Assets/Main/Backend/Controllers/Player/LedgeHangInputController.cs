using OSBE.Controllers.Player.Interfaces;
using OSCore.Data.Controllers;
using OSCore.Data.Enums;
using OSCore.Data;
using OSCore.ScriptableObjects;
using OSCore.Utils;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerCfgSO;
using static OSCore.Data.Controllers.PlayerControllerInput;

namespace OSBE.Controllers.Player {
    public class LedgeHangInputController : IPlayerInputController {
        private readonly IPlayerMainController controller;
        private readonly PlayerCfgSO cfg;
        private readonly Transform transform;

        public LedgeHangInputController(IPlayerMainController controller, PlayerCfgSO cfg, Transform transform) {
            this.controller = controller;
            this.cfg = cfg;
            this.transform = transform;
        }

        public void On(PlayerControllerInput e) {
            switch (e) {
                case MovementInput ev:
                    Debug.Log("MOVE " + ev.direction.Directionify());
                    break;
            }
        }

        public void OnUpdate(PlayerState state) {
            RotatePlayer(state, PlayerControllerUtils.MoveCfg(cfg, state));
        }

        private void RotatePlayer(PlayerState state, MoveConfig moveCfg) {
            Vector2 direction;

            if (Vectors.NonZero(state.std.facing)
                && (state.std.stance != PlayerStance.CRAWLING || !Vectors.NonZero(state.std.movement)))
                direction = state.std.facing;
            else if (state.std.mouseLookTimer <= 0f && Vectors.NonZero(state.std.movement))
                direction = state.std.movement;
            else return;

            float rotationZ = Vectors.AngleTo(Vector2.zero, direction);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, rotationZ),
                moveCfg.rotationSpeed * Time.deltaTime);
        }
    }
}

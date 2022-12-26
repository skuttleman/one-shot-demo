using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using System;

namespace OSBE.Controllers.Player.Interfaces {
    public interface IPlayerMainController {
        public PlayerControllerState UpdateState(Func<PlayerControllerState, PlayerControllerState> updateFn);
    }

    public interface IPlayerInputController : IController<PlayerControllerInput>, IStateReceiver<PlayerAnim> {
        public void OnActivate(PlayerControllerState state) { }
        public void OnUpdate(PlayerControllerState state) { }
        public void OnFixedUpdate(PlayerControllerState state) { }
        public void OnDeactivate(PlayerControllerState state) { }
    }
}

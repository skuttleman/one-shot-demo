using OSCore.Data.Events;
using OSCore.Data;
using OSCore.System.Interfaces.Controllers;
using System;
using OSCore.Data.Controllers;

namespace OSBE.Controllers.Player.Interfaces {
    public interface IPlayerMainController {
        public void Publish(IEvent e);
        public PlayerState UpdateState(Func<PlayerState, PlayerState> updateFn);
    }

    public interface IPlayerInputController : IController<PlayerControllerInput> {
        public void OnActivate(PlayerState state) { }
        public void OnUpdate(PlayerState state) { }
        public void OnFixedUpdate(PlayerState state) { }
        public void OnDeactivate(PlayerState state) { }
    }
}

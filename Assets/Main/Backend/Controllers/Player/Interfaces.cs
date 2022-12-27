using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;
using System;

namespace OSBE.Controllers.Player.Interfaces {
    public interface IPlayerMainController {
        public PlayerControllerState state { get; }
        public PlayerControllerState UpdateState(Func<PlayerControllerState, PlayerControllerState> updateFn);
    }

    public interface IPlayerInputController :
        IController<PlayerControllerInput>,
        IStateReceiver<PlayerAnim>,
        IComponentLifecycle { }
}

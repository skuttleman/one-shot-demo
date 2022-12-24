using OSCore.Data.Animations;
using OSCore.Data.Controllers;
using OSCore.Data;
using OSCore.System.Interfaces.Controllers;
using OSCore.System.Interfaces;

namespace OSBE.Controllers.Player.Interfaces {
    public interface IPlayerMainController {
        public IPlayerMainController Notify(PlayerControllerInput msg);
    }

    public interface IPlayerInputController : IController<PlayerControllerInput>, IStateReceiver<PlayerAnim> {
        public void OnActivate(PlayerSharedInputState state) { }
        public void OnUpdate(PlayerSharedInputState state) { }
        public void OnFixedUpdate(PlayerSharedInputState state) { }
        public void OnDeactivate(PlayerSharedInputState state) { }
    }
}

using OSCore.Data.Enums;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

namespace OSCore.Data.Events {
    namespace Brains {
        public interface IEvent { }

        public record InitEvent<T>(T cfg) : IEvent
            where T : ScriptableObject;

        namespace Player {
            public record InputEvent : IEvent {
                public record MovementInput(Vector2 direction) : InputEvent();
                public record SprintInput(bool isSprinting) : InputEvent();
                public record LookInput(Vector2 direction, bool isMouse) : InputEvent();
                public record StanceInput(float holdDuration) : InputEvent();
                public record AimInput(bool isAiming) : InputEvent();
                public record AttackInput(bool isAttacking) : InputEvent();
                public record ScopeInput(bool isScoping) : InputEvent();

                private InputEvent() { }
            }

            public record AnimationEmittedEvent : IEvent {
                public record StanceChanged(PlayerStance stance) : AnimationEmittedEvent();
                public record AttackModeChanged(PlayerAttackMode mode) : AnimationEmittedEvent();
                public record MovementChanged(bool isMoving) : AnimationEmittedEvent();
                public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();
                public record PlayerStep() : AnimationEmittedEvent();

                private AnimationEmittedEvent() { }
            }
        }

        namespace SPA {
            public record SPAEvent : IEvent {
                public record InstallSPA(IControllerBrain spa) : SPAEvent();
                public record MoveSPA(Vector2 direction, float speed) : SPAEvent();
                public record FaceSPA(Vector2 direction, float speed) : SPAEvent();

                private SPAEvent() { }
            }
        }

        namespace Camera {
            public record CameraEvent : IEvent {
                public record CameraInitEvent(
                    CameraCfgSO cfg,
                    CinemachineCameraOffset offset)
                    : CameraEvent();

                private CameraEvent() { }
            }
        }
    }
}

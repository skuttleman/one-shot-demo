using OSCore.Data.Enums;

namespace OSCore.Data {
    namespace Enums {
        public enum IdTag {
            PLAYER
        }

        public enum PlayerStance {
            STANDING, CROUCHING, CRAWLING
        }

        public enum PlayerAttackMode {
            NONE, HAND, WEAPON, PUNCHING, FIRING
        }
    }

    namespace Events.Brains {
        public interface IEvent { }

        public record InitEvent<T>(T cfg) : IEvent;

        namespace Player {
            public record AnimationEmittedEvent : IEvent {
                public record StanceChanged(PlayerStance stance) : AnimationEmittedEvent();
                public record AttackModeChanged(PlayerAttackMode mode) : AnimationEmittedEvent();
                public record MovementChanged(bool isMoving) : AnimationEmittedEvent();
                public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();

                private AnimationEmittedEvent() { }
            }
        }
    }
}

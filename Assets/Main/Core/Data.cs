using OSCore.Data.Enums;
using UnityEngine;

namespace OSCore.Data {
    namespace Enums {
        public enum IdTag {
            PLAYER, ENEMY
        }

        public enum PlayerStance {
            STANDING, CROUCHING, CRAWLING
        }

        public enum AttackMode {
            NONE, HAND, WEAPON, MELEE, FIRING
        }

        public enum EnemyAwareness {
            PASSIVE, AWARE, AGGRESSIVE
        }
    }

    public record PlayerState {
        public Vector2 movement { get; init; }
        public Vector2 facing { get; init; }
        public PlayerStance stance { get; init; }
        public AttackMode attackMode { get; init; }
        public float mouseLookTimer { get; init; }
        public bool isMoving { get; init; }
        public bool isSprinting { get; init; }
        public bool isScoping { get; init; }
    }

    public record EnemyState {
        public EnemyAwareness awareness { get; init; }
        public bool seesPlayer { get; init; }
    }

    namespace Patrol {
        public record EnemyPatrol {
            public record PatrolWait(float seconds) : EnemyPatrol();
            public record PatrolGoto(Vector3 position) : EnemyPatrol();
            public record PatrolFace(Vector3 rotation) : EnemyPatrol();
            public record PatrolRotate(float rotation) : EnemyPatrol();

            private EnemyPatrol() { }
        }
    }


    namespace Events.Brains {
        public interface IEvent { }

        public record InitEvent<T>(T cfg) : IEvent;

        namespace Player {
            public record AnimationEmittedEvent : IEvent {
                public record StanceChanged(PlayerStance stance) : AnimationEmittedEvent();
                public record AttackModeChanged(AttackMode mode) : AnimationEmittedEvent();
                public record MovementChanged(bool isMoving) : AnimationEmittedEvent();
                public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();

                private AnimationEmittedEvent() { }
            }
        }
    }
}

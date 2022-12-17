using OSCore.Data.Enums;
using UnityEngine;

namespace OSCore.Data {
    namespace Enums {
        public enum IdTag {
            PLAYER, ENEMY,
        }

        public enum PlayerStance {
            STANDING, CROUCHING, CRAWLING,
        }

        public enum AttackMode {
            NONE, HAND, WEAPON, MELEE, FIRING,
        }
    }

    public record PlayerState {
        public Vector2 movement { get; init; }
        public Vector2 facing { get; init; }
        public PlayerStance stance { get; init; }
        public AttackMode attackMode { get; init; }
        public float mouseLookTimer { get; init; }
        public bool isMoving { get; init; }
        public bool isScoping { get; init; }
    }

    public record EnemyState {
        public bool isPlayerInView { get; init; }
    }

    namespace Animations {
        public enum PlayerAnim {
            stand_idle,
            stand_move,
            stand_punch,
            stand_fall,

            crouch_idle_bino,
            crouch_move_bino,
            crouch_tobino,
            crouch_idle,
            crouch_move,
            crouch_punch,
            crouch_toaim,
            crouch_idle_aim,
            crouch_move_aim,
            crouch_fire,

            crawl_idle_bino,
            crawl_tobino,
            crawl_idle,
            crawl_move,
            crawl_punch,
            crawl_toaim,
            crawl_idle_aim,
            crawl_fire,
        }

        public enum PlayerAnimSignal {
            FALLING,
            LAND_MOVING,
            LAND_STILL,
            STANCE,
            MOVE_ON,
            MOVE_OFF,
            SPRINT,
            SCOPE_ON,
            SCOPE_OFF,
            AIM_ON,
            AIM_OFF,
            ATTACK,
        }
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

    namespace Events {
        public interface IEvent { }

        namespace Controllers {
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
}

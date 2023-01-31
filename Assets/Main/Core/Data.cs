using System;
using OSCore.Data.Animations;
using OSCore.Data.Enums;
using UnityEngine;

namespace OSCore.Data {
    namespace Enums {
        public enum IdTag {
            // Unique Tags
            PLAYER,
            DEBUG_LAYER,

            // Non-Unique Tags
            ENEMY,
            PLATFORM_CATCHABLE,
        }

        public enum PlayerStance {
            STANDING, CROUCHING, CRAWLING,
        }

        public enum PlayerSpeed {
            STOPPED, SLOW, FAST
        }

        public enum AttackMode {
            NONE, HAND, WEAPON, MELEE, FIRING,
        }

        public enum PlayerInputControlMap {
            None, Standard, LedgeHang,
        }

        public enum ClimbDirection {
            UP, DOWN,
        }
    }

    public record PlayerControllerState {
        public PlayerInputControlMap controls { get; init; }
        public Vector2 movement { get; init; }
        public Vector2 facing { get; init; }
        public AttackMode attackMode { get; init; }
        public RaycastHit ground { get; init; }
        public Collider ledge { get; init; }
        public Vector3 hangingPoint { get; init; }
        public float mouseLookTimer { get; init; }
        public float tbdTimer { get; init; }

        public PlayerAnim anim { get; init; }
        public PlayerStance stance { get; init; }
        public bool isHanging { get; init; }
        public bool isMoving { get; init; }
        public bool isScoping { get; init; }
        public bool isAiming { get; init; }
        public bool isAttacking { get; init; }
        public bool isGrounded { get; init; }
    }

    namespace Controllers {
        public record PlayerControllerInput {
            public record MovementInput(Vector2 direction) : PlayerControllerInput();
            public record SprintInput(bool isSprinting) : PlayerControllerInput();
            public record LookInput(Vector2 direction, bool isMouse) : PlayerControllerInput();
            public record AimInput(bool isAiming) : PlayerControllerInput();
            public record AttackInput(bool isAttacking) : PlayerControllerInput();
            public record DamageInput(float damage) : PlayerControllerInput();
            public record StanceInput() : PlayerControllerInput();
            public record DiveInput() : PlayerControllerInput();
            public record ScopeInput(bool isScoping) : PlayerControllerInput();
            public record ClimbInput(ClimbDirection direction) : PlayerControllerInput();
            public record TBDInput() : PlayerControllerInput();

            private PlayerControllerInput() { }
        }

        public record EnemyControllerInput {
            public record MovementInput(Vector2 direction) : EnemyControllerInput();
            public record SprintInput(bool isSprinting) : EnemyControllerInput();
            public record LookInput(Vector2 direction) : EnemyControllerInput();
            public record AimInput(bool isAiming) : EnemyControllerInput();
            public record DamageInput(float damage) : EnemyControllerInput();
            public record PlayerLOS(float visibility, float distance, float angle, Vector3 location)
                : EnemyControllerInput();

            private EnemyControllerInput() { }
        }
    }

    namespace Events {
        public interface IEvent { }

        namespace Controllers {
            namespace Player {
                public record AnimationEmittedEvent : IEvent {
                    public record AnimationChanged(PlayerAnim prev, PlayerAnim curr) : AnimationEmittedEvent();
                    public record StanceChanged(PlayerStance stance) : AnimationEmittedEvent();
                    public record AttackModeChanged(AttackMode mode) : AnimationEmittedEvent();
                    public record MovementChanged(PlayerSpeed speed) : AnimationEmittedEvent();
                    public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();

                    private AnimationEmittedEvent() { }
                }
            }
        }
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
            crawl_dive,

            hang_lunge,
            hang_idle,
            hang_move,
            hang_climb,
        }

        public enum EnemyAnim {
            stand_idle,
            stand_move,
            stand_toaim,
            stand_idle_aim,
            stand_melee,
            stand_fire,
            stand_fall,
        }
    }

    namespace AI {
        public enum EnemyAwareness {
            PASSIVE, RETURN_PASSIVE, CURIOUS, INVESTIGATING, ALERT, RETURN_ALERT, ALERT_CURIOUS, ALERT_INVESTIGATING, AGGRESIVE, SEARCHING
        }

        [Serializable]
        public struct StateConfig {
            [field: SerializeField] public float moveSpeed { get; private set; }
            [field: SerializeField] public float rotationSpeed { get; private set; }

            [field: SerializeField] public float suspicionIncrease { get; private set; }

            [field: Header("FOV")]
            [field: SerializeField] public TripleThreshold visibility { get; private set; }
            [field: SerializeField] public TripleThreshold distance { get; private set; }
            [field: SerializeField] public TripleThreshold angle { get; private set; }
        }

        [Serializable]
        public struct TripleThreshold {
            [field: SerializeField] public float limit { get; private set; }
            [field: SerializeField] public float lowThresh { get; private set; }
            [field: SerializeField] public float highThresh { get; private set; }
        }
    }

}

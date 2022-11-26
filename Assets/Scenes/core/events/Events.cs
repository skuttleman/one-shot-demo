using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.Events {
    namespace Brains {
        public interface IEvent { }

        namespace Player {
            public interface IPlayerEvent : IEvent { }

            public enum PlayerStance {
                STANDING, CROUCHING, CRAWLING
            }

            public enum PlayerAttackMode {
                NONE, HAND, WEAPON, PUNCHING, FIRING
            }

            public record InputEvent : IPlayerEvent {
                public record MovementInput(Vector2 direction) : InputEvent();
                public record LookInput(Vector2 direction) : InputEvent();
                public record StanceInput(float holdDuration) : InputEvent();
                public record AimInput(bool isAiming) : InputEvent();
                public record AttackInput(bool isAttacking) : InputEvent();
                public record ScopeInput(bool isScoping) : InputEvent();

                private InputEvent() { }
            }

            public record AnimationEmittedEvent : IPlayerEvent {
                public record StanceChanged(PlayerStance stance) : AnimationEmittedEvent();
                public record AttackModeChanged(PlayerAttackMode mode) : AnimationEmittedEvent();
                public record MovementChanged(float speed) : AnimationEmittedEvent();
                public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();
                public record PlayerStep() : AnimationEmittedEvent();

                private AnimationEmittedEvent() { }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using OSCore.Data.Enums;
using UnityEngine;

namespace OSCore.Events {
    namespace Brains {
        public interface IEvent { }

        public record InitEvent<T>(T cfg) : IEvent
            where T : ScriptableObject;

        namespace Player {
            public interface IPlayerEvent : IEvent { }

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
                public record MovementChanged(bool isMoving) : AnimationEmittedEvent();
                public record ScopingChanged(bool isScoping) : AnimationEmittedEvent();
                public record PlayerStep() : AnimationEmittedEvent();

                private AnimationEmittedEvent() { }
            }
        }
    }
}

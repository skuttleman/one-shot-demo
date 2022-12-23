using OSCore.Data.Enums;
using OSCore.Data.Events;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSCore.System.Interfaces {
    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent;
    }

    public interface IGameSystemComponent {
        public void OnStart() { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnDestroy() { }
    }

    namespace Tagging {
        public interface ITagRegistry : IGameSystemComponent {
            public void Register(IdTag tag, GameObject obj);
            public void RegisterUnique(IdTag tag, GameObject obj);
            public ISet<GameObject> Get(IdTag tag);
            public GameObject GetUnique(IdTag tag);
        }
    }

    namespace Events {
        public interface IPubSub : IGameSystemComponent {
            void Publish<T>(T item) where T : IEvent;
            long Subscribe<T>(Action<T> action) where T : IEvent;
            void Unsubscribe(long id);
        }
    }

    public interface IStateReceiver<State> {
        public void OnStateExit(State state) { }
        public void OnStateTransition(State prev, State curr) { }
        public void OnStateEnter(State state) { }
    }

    namespace Controllers {
        public interface IPlayerController {
            public void OnMovementInput(Vector2 direction);
            public void OnSprintInput(bool isSprinting);
            public void OnLookInput(Vector2 direction, bool isMouse);
            public void OnStanceInput();
            public void OnAimInput(bool isAiming);
            public void OnAttackInput(bool isAttacking);
            public void OnScopeInput(bool isScoping);
            public void OnClimbInput(bool isClimbing);
            public void OnDropInput(bool isDropping);
            public void OnPlayerStep();
        }

        public interface IEnemyController {
            public void OnEnemyStep();
            public void OnPlayerSightChange(bool isInView);
            public void OnDamage(float damage);
        }

        public interface IDamage {
            public void OnAttack(float damage);
        }
    }
}

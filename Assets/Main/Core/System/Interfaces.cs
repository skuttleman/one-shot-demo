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
        public interface IController<E> {
            public void On(E e);
            public void OnStep() { }
        }

        public interface IDamage {
            public void OnAttack(float damage);
        }
    }
}

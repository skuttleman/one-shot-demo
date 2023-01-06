using OSCore.Data.Enums;
using OSCore.Data.Events;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSCore.System.Interfaces {
    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IComponentLifecycle;
        public R Send<T, R>(Func<T, R> action) where T : IComponentLifecycle;
        public void Register<T>(T component) where T : IComponentLifecycle;
        public void Unregister<T>() where T : IComponentLifecycle;
    }

    public interface IComponentLifecycle {
        public void OnActivate() { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }
        public void OnDeactivate() { }
    }

    namespace Tagging {
        public interface ITagRegistry : IComponentLifecycle {
            public void Register(IdTag tag, GameObject obj);
            public void RegisterUnique(IdTag tag, GameObject obj);
            public ISet<GameObject> Get(IdTag tag);
            public GameObject GetUnique(IdTag tag);
        }
    }

    namespace Events {
        public interface IPubSub : IComponentLifecycle {
            void Publish(IEvent item);
            long Subscribe(Action<IEvent> action);
            void Unsubscribe(long id);
        }
    }

    public interface IStateReceiver<State> {
        public void OnStateInit(State curr) { }
        public void OnStateTransition(State prev, State curr) { }
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

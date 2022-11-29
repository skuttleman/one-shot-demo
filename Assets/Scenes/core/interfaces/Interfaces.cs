using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Data.Enums;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent;
    }

    public interface IGameSystemComponent {
        public void Update();
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

    namespace Brains {
        public record BrainId(EControllerBrainTag tag) {
            public record UniqueId(EControllerBrainTag tag) : BrainId(tag);
            public record InstanceId(Transform transform, EControllerBrainTag tag) : BrainId(tag);
        }

        public enum EControllerBrainTag {
            PLAYER, CAMERA
        }

        public interface IControllerBrain : IGameSystemComponent {
            public void OnMessage(IEvent message);
        }

        public interface IControllerBrainManager : IGameSystemComponent {
            public IControllerBrain Ensure(BrainId id, Transform target);
            public void OnMessage(BrainId id, IEvent message);
        }
    }
}

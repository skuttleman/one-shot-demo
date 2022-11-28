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

    namespace Brains {
        public enum EControllerBrainTag {
            PLAYER
        }

        public interface IControllerBrain : IGameSystemComponent {
            public void OnMessage(IEvent message);
        }

        public interface IControllerBrainManager : IGameSystemComponent {
            public IControllerBrain Ensure(Transform target, EControllerBrainTag tag);
            public void OnMessage(EControllerBrainTag tag, IEvent message);
        }
    }
}

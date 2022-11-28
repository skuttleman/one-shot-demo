using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    namespace OSCore.Interfaces.Brains {
        public enum EControllerBrainTag {
            PLAYER
        }

        public interface IGameSystem {
            public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
            public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent;
        }
        public interface IGameSystemComponent {
            public void Update();
            public void OnDestroy() {}
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

using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    public enum EControllerBrainTag {
        PLAYER
    }

    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
    }
    public interface IGameSystemComponent {
        public void Update(IGameSystem session);
    }

    public interface IControllerBrain : IGameSystemComponent {
        public void OnMessage(IEvent message);
    }

    public interface IControllerBrainManager : IGameSystemComponent {
        // TODO - proper identifier
        public IControllerBrain Ensure(Transform transform, EControllerBrainTag tag);
        public void OnMessage(EControllerBrainTag tag, IEvent message);
    }
}

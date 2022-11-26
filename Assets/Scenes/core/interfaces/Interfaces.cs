using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    public interface IGameSystem {
        public IGameSystem With<T>(Action<T> action) where T : IGameSystemComponent;
    }
    public interface IGameSystemComponent {
        public void Update(IGameSystem session);
    }

    public interface IControllerBrain : IGameSystemComponent {
        public void OnMessage(IEvent message);
        public void OnMessageSync(IEvent message);
    }
    public interface IControllerBrainManager : IGameSystemComponent {
        // TODO - proper identifier
        public IControllerBrain Ensure(Transform transform, ISet<string> tags);
        public void OnMessage(ISet<string> tags, IEvent message);
        public void OnMessageSync(ISet<string> tags, IEvent message);
    }
}

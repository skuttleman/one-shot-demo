using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    public interface IGameSystem {
        public T Get<T>() where T : IGameSystemComponent;
    }
    public interface IGameSystemComponent {
        public void Update(IGameSystem session);
    }

    public interface IControllerBrain : IGameSystemComponent {
        public void OnMessage(IMessage message);
        public void OnMessageSync(IMessage message);
    }
    public interface IControllerBrainManager : IGameSystemComponent {
        // TODO - proper identifier
        public IControllerBrain Ensure(Transform transform, ISet<string> tags);
        public void OnMessage(ISet<string> tags, IMessage message);
        public void OnMessageSync(ISet<string> tags, IMessage message);
    }
}

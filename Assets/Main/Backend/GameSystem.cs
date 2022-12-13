using OSBE.Async.Core;
using OSBE.Async;
using OSBE.Controllers;
using OSBE.Tagging;
using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using OSCore.Utils;
using OSCore;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSBE {
    public class GameSystem : MonoBehaviour, IGameSystem {
        private IDictionary<Type, IGameSystemComponent> components;
        private GameController controller;

        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent {
            T component = (T)components.Get(typeof(T), null);

            if (component is not null) action(component);
            return this;
        }

        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent {
            T component = (T)components.Get(typeof(T), null);

            if (component is null) {
                Debug.Log("No Component Found for " + typeof(T));
                return default;
            }

            return action(component);
        }

        private void OnEnable() {
            controller = FindObjectOfType<GameController>();
            Init();
        }

        private void Start() =>
            components?.ForEach(component => component.Value.OnStart());

        private void Update() =>
            components?.ForEach(component => component.Value.OnUpdate());

        private void FixedUpdate() =>
            components?.ForEach(component => component.Value.OnFixedUpdate());

        private void OnDestroy() {
            components?.ForEach(component => component.Value.OnDestroy());
            components = new Dictionary<Type, IGameSystemComponent>();
        }

        private void Init() {
            components = new Dictionary<Type, IGameSystemComponent> {
                { typeof(IControllerManager), new ControllerManager(this) },
                { typeof(PromiseFactory) , new PromiseFactory() },
                { typeof(ITagRegistry), new TagRegistry() },
                { typeof(IPubSub), new DictionaryPubSub() }
            };

            controller.Init(this);
        }
    }
}

using OSBE.Async;
using OSBE.Tagging;
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
        private IDictionary<Type, IComponentLifecycle> components;
        private GameController controller;

        public IGameSystem Send<T>(Action<T> action) where T : IComponentLifecycle {
            T component = (T)components.Get(typeof(T), null);

            if (component is not null) action(component);
            return this;
        }

        public R Send<T, R>(Func<T, R> action) where T : IComponentLifecycle {
            T component = (T)components.Get(typeof(T), null);

            if (component is null) {
                Debug.Log("No Component Found for " + typeof(T));
                return default;
            }

            return action(component);
        }

        public void Register<T>(T component) where T : IComponentLifecycle {
            components[typeof(T)] = component;
        }

        public void Unregister<T>() where T : IComponentLifecycle {
            if (components.ContainsKey(typeof(T))) {
                components.Remove(typeof(T));
            }
        }

        private void OnEnable() {
            controller = FindObjectOfType<GameController>();
            Init();
        }

        private void Start() {
            components?.ForEach(component => component.Value.OnActivate());
        }

        private void Update() {
            components?.ForEach(component => component.Value.OnUpdate());
        }

        private void FixedUpdate() {
            components?.ForEach(component => component.Value.OnFixedUpdate());
        }

        private void OnDestroy() {
            components?.ForEach(component => component.Value.OnDeactivate());
            components = new Dictionary<Type, IComponentLifecycle>();
        }

        private void Init() {
            components = new Dictionary<Type, IComponentLifecycle> {
                { typeof(ITagRegistry), new TagRegistry() },
                { typeof(IPubSub), new DictionaryPubSub() }
            };

            controller.Init(this);
        }
    }
}

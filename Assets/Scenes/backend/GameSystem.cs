using System;
using System.Collections.Generic;
using OSCore;
using UnityEngine;
using OSCore.Utils;
using OSBE.Brains;
using OSCore.Interfaces.Brains;
using System.Collections.Concurrent;
using OSBE.Async.Core;
using OSBE.Tags;
using OSCore.Interfaces;
using OSCore.Interfaces.Tagging;
using OSCore.Interfaces.Events;
using OSBE.Async;

namespace OSBE {
    public class GameSystem : MonoBehaviour, IGameSystem {
        IDictionary<Type, IGameSystemComponent> components;
        GameController controller;
        ConcurrentQueue<Action> callbacks;

        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent {
            T component = (T)components.Get(typeof(T), null);
            if (component is not null) action(component);
            return this;
        }

        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent {
            T component = (T)components.Get(typeof(T), null);
            if (component is null) {
                return default;
            }
            return action(component);
        }

        void Awake() {
            callbacks = new();

            foreach (GameSystem obj in FindObjectsOfType<GameSystem>())
                if (obj.gameObject != gameObject) {
                    Destroy(obj.gameObject);
                } else {
                    DontDestroyOnLoad(obj.gameObject);
                }
        }

        void OnEnable() {
            //Debug.Log("ENABLE");
            controller = FindObjectOfType<GameController>();
            Init();
        }

        void Update() {
            components?.ForEach(component => component.Value.Update());

            while (callbacks.TryDequeue(out Action callback))
                callback();
        }

        void Init() {
            components = new Dictionary<Type, IGameSystemComponent> {
                { typeof(IControllerBrainManager), new ControllerBrainManager(this) },
                { typeof(PromiseFactory) , new PromiseFactory() },
                { typeof(ITagRegistry), new TagRegistry() },
                { typeof(IPubSub), new DictionaryPubSub() }
            };

            controller.Init(this);
        }
    }
}

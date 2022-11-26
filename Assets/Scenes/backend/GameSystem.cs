using System;
using System.Collections;
using System.Collections.Generic;
using OSCore;
using OSCore.Interfaces;
using UnityEngine;
using OSCore.Utils;
using OSBE.Brains;

namespace OSBE {
    public class GameSystem : MonoBehaviour, IGameSystem {
        IDictionary<Type, IGameSystemComponent> components;
        GameController controller;

        public T Get<T>() where T : IGameSystemComponent {
            return (T) components.Get(typeof(T));
        }

        void Awake() {
            if (FindObjectsOfType<GameSystem>().Length > 1) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            controller = FindObjectOfType<GameController>();
            Init();
        }

        void Init() {
            components = new Dictionary<Type, IGameSystemComponent> {
                { typeof(IControllerBrainManager), new ControllerBrainFactory() }
            };
            controller.Init(this);
        }

        void Update() {
            components?.ForEach(component => component.Value.Update(this));
        }
    }
}

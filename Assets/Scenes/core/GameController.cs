using System;
using System.Collections.Generic;
using OSCore.Interfaces;
using UnityEngine;

namespace System.Runtime.CompilerServices { public class IsExternalInit { } }


namespace OSCore {
    public class GameController : MonoBehaviour, IGameSystem {
        IGameSystem system;

        public void Init(IGameSystem system) {
            this.system = system;
        }

        public IGameSystem With<T>(Action<T> action) where T : IGameSystemComponent {
            system.With(action);
            return this;
        }

        void Awake() {
            if (FindObjectsOfType<GameController>().Length > 1) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

    }
}

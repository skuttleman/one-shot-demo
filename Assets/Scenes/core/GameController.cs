using System.Collections;
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

        public T Get<T>() where T : IGameSystemComponent => system.Get<T>();

        void Awake() {
            if (FindObjectsOfType<GameController>().Length > 1) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }

    }
}

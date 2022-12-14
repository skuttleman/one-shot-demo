using OSCore.System.Interfaces;
using OSCore.Utils;
using System;
using UnityEngine;

namespace System.Runtime.CompilerServices { public class IsExternalInit { } }

namespace OSCore {
    public class GameController : MonoBehaviour, IGameSystem {
        [SerializeField] private bool hideInvisible = true;

        private IGameSystem system;

        public void Init(IGameSystem system) {
            this.system = system;
        }

        public IGameSystem Send<T>(Action<T> action) where T : IComponentLifecycle {
            system.Send(action);
            return this;
        }

        public R Send<T, R>(Func<T, R> action) where T : IComponentLifecycle =>
            system.Send(action);

        public void Register<T>(T component) where T : IComponentLifecycle {
            system.Register(component);
        }

        public void Unregister<T>() where T : IComponentLifecycle {
            system.Unregister<T>();
        }

        /*
         * Lifecycle Methods
         */

        private void Awake() {
            if (FindObjectsOfType<GameController>().Length > 1) {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);

            if (hideInvisible)
                Sequences.Transduce(
                    GameObject.FindGameObjectsWithTag("Invisible"),
                    Fns.MapCat<GameObject, Renderer>(obj => obj.GetComponentsInChildren<Renderer>()),
                    rdr => rdr.enabled = false);
        }
    }
}

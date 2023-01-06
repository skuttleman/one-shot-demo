using OSCore.Data.Events;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using UnityEngine;

namespace OSCore.System {
    public abstract class ASystemInitializer : MonoBehaviour {
        protected IGameSystem system;

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
        }
    }

    public abstract class ASystemInitializer<A> : MonoBehaviour
        where A : IEvent {
        protected IGameSystem system;
        private long sub;

        protected abstract void OnEvent(A e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            sub = system.Send<IPubSub, long>(pubsub =>
                pubsub.Subscribe(e => {
                    switch (e) {
                        case A ev: OnEvent(ev); break;
                    }
                }));
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                system.Send<IPubSub>(pubsub => pubsub.Unsubscribe(sub)));
        }
    }

    public abstract class ASystemInitializer<A, B> : MonoBehaviour
        where A : IEvent
        where B : IEvent {
        protected IGameSystem system;
        private long sub;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            sub = system.Send<IPubSub, long>(pubsub =>
                pubsub.Subscribe(e => {
                    switch (e) {
                        case A ev: OnEvent(ev); break;
                        case B ev: OnEvent(ev); break;
                    }
                }));
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                system.Send<IPubSub>(pubsub => pubsub.Unsubscribe(sub)));
        }
    }

    public abstract class ASystemInitializer<A, B, C> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent {
        protected IGameSystem system;
        private long sub;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            sub = system.Send<IPubSub, long>(pubsub =>
                pubsub.Subscribe(e => {
                    switch (e) {
                        case A ev: OnEvent(ev); break;
                        case B ev: OnEvent(ev); break;
                        case C ev: OnEvent(ev); break;
                    }
                }));
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                system.Send<IPubSub>(pubsub => pubsub.Unsubscribe(sub)));
        }
    }

    public abstract class ASystemInitializer<A, B, C, D> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent
        where D : IEvent {
        protected IGameSystem system;
        private long sub;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);
        protected abstract void OnEvent(D e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            sub = system.Send<IPubSub, long>(pubsub =>
                pubsub.Subscribe(e => {
                    switch (e) {
                        case A ev: OnEvent(ev); break;
                        case B ev: OnEvent(ev); break;
                        case C ev: OnEvent(ev); break;
                        case D ev: OnEvent(ev); break;
                    }
                }));
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                system.Send<IPubSub>(pubsub => pubsub.Unsubscribe(sub)));
        }
    }
}

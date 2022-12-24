using OSCore.Data.Events;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.System {
    public abstract class ASystemInitializer : MonoBehaviour {
        protected IGameSystem system;

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
        }
    }

    public abstract class ASystemInitializer<A> : MonoBehaviour
        where A : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        protected abstract void OnEvent(A e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                });
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }

    }

    public abstract class ASystemInitializer<A, B> : MonoBehaviour
        where A : IEvent
        where B : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                });
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }
    }

    public abstract class ASystemInitializer<A, B, C> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                    pubsub.Subscribe<C>(OnEvent),
                });
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }
    }

    public abstract class ASystemInitializer<A, B, C, D> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent
        where D : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);
        protected abstract void OnEvent(D e);

        /*
         * Lifecycle Methods
         */

        protected virtual void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                    pubsub.Subscribe<C>(OnEvent),
                    pubsub.Subscribe<D>(OnEvent),
                });
        }

        protected virtual void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }
    }
}

using OSCore.Data.Events.Brains;
using OSCore.System.Interfaces.Events;
using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.System {
    public abstract class ASystemInitializer : MonoBehaviour {
        protected IGameSystem system;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
        }
    }

    public abstract class ASystemInitializer<A> : MonoBehaviour
        where A : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                });
        }

        private void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }

        protected abstract void OnEvent(A e);
    }

    public abstract class ASystemInitializer<A, B> : MonoBehaviour
        where A : IEvent
        where B : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                });
        }

        private void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
    }

    public abstract class ASystemInitializer<A, B, C> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                    pubsub.Subscribe<C>(OnEvent),
                });
        }

        private void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);
    }

    public abstract class ASystemInitializer<A, B, C, D> : MonoBehaviour
        where A : IEvent
        where B : IEvent
        where C : IEvent
        where D : IEvent {
        protected IGameSystem system;
        private IEnumerable<long> subs;

        private void OnEnable() {
            system = FindObjectOfType<GameController>();
            subs = system.Send<IPubSub, IEnumerable<long>>(pubsub =>
                new long[] {
                    pubsub.Subscribe<A>(OnEvent),
                    pubsub.Subscribe<B>(OnEvent),
                    pubsub.Subscribe<C>(OnEvent),
                    pubsub.Subscribe<D>(OnEvent),
                });
        }

        private void OnDisable() {
            system.Send<IPubSub>(pubsub =>
                subs.ForEach(sub => pubsub.Unsubscribe(sub)));
        }

        protected abstract void OnEvent(A e);
        protected abstract void OnEvent(B e);
        protected abstract void OnEvent(C e);
        protected abstract void OnEvent(D e);
    }
}

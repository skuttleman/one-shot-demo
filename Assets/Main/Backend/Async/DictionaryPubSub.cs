using OSCore.Data.Events;
using OSCore.System.Interfaces.Events;
using OSCore.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace OSBE.Async {
    public class DictionaryPubSub : IPubSub {
        private readonly ConcurrentQueue<(Type, IEvent)> messages;
        private readonly IDictionary<Type, ISet<Action<IEvent>>> subscribers;
        private readonly IDictionary<long, (Type, Action<IEvent>)> subscriptions;
        private long subId;

        public DictionaryPubSub() {
            messages = new();
            subscribers = new Dictionary<Type, ISet<Action<IEvent>>>();
            subscriptions = new Dictionary<long, (Type, Action<IEvent>)>();
            subId = 0;
        }

        public void Publish<T>(T message) where T : IEvent {
            messages.Enqueue((message.GetType(), message));
        }

        public long Subscribe<T>(Action<T> action) where T : IEvent {
            long id = ++subId;
            Type t = typeof(T);
            Action<IEvent> cb = e => action((T)e);
            subscriptions.Add(id, (t, cb));
            subscribers.Update(t,
                set => Colls.Add(set, cb),
                () => new HashSet<Action<IEvent>>());
            return id;
        }

        public void Unsubscribe(long id) {
            if (subscriptions.ContainsKey(id)) {
                (Type,Action<IEvent>) tpl = subscriptions[id];
                subscribers.Update(tpl.Item1,
                    set => Colls.Remove(set, tpl.Item2),
                    () => new HashSet<Action<IEvent>>());
                subscriptions.Remove(id);
            }
        }

        public void OnUpdate() {
            while (messages.TryDequeue(out (Type, IEvent) tpl)) {
                subscribers.Get(tpl.Item1)?.ForEach(action => action(tpl.Item2));
            }
        }
    }
}

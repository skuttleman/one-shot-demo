using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using OSCore.Events.Brains;
using OSCore.Interfaces;
using OSCore.Interfaces.Events;
using OSCore.Utils;

namespace OSBE.Async {
    public class DictionaryPubSub : IPubSub, IGameSystemComponent {
        ConcurrentQueue<(Type, IEvent)> messages;
        IDictionary<Type, ISet<Action<IEvent>>> subscribers;
        IDictionary<long, (Type, Action<IEvent>)> subscriptions;
        long subId;

        public DictionaryPubSub() {
            messages = new();
            subscribers = new Dictionary<Type, ISet<Action<IEvent>>>();
            subscriptions = new Dictionary<long, (Type, Action<IEvent>)>();
            subId = 0;
        }

        public void Publish<T>(T message) where T : IEvent {
            messages.Enqueue((typeof(T), message));
        }

        public long Subscribe<T>(Action<IEvent> action) where T : IEvent {
            long id = ++subId;
            Type t = typeof(T);
            subscriptions.Add(id, (t, action));
            subscribers.Update(t,
                set => Colls.Add(set, action),
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

        public void Update() {
            while (messages.TryDequeue(out (Type, IEvent) tpl))
                subscribers[tpl.Item1]?.ForEach(action => action(tpl.Item2));
        }
    }
}

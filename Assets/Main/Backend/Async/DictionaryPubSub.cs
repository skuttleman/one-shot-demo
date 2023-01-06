using OSCore.Data.Events;
using OSCore.System.Interfaces.Events;
using OSCore.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace OSBE.Async {
    public class DictionaryPubSub : IPubSub {
        private readonly ConcurrentQueue<(Type, IEvent)> messages;
        private readonly IDictionary<long, Action<IEvent>> subscriptions;
        private long subId;

        public DictionaryPubSub() {
            messages = new();
            subscriptions = new Dictionary<long, Action<IEvent>>();
            subId = 0;
        }

        public void Publish(IEvent message) {
            messages.Enqueue((message.GetType(), message));
        }

        public long Subscribe(Action<IEvent> action) {
            long id = ++subId;
            subscriptions.Add(id, action);
            return id;
        }

        public void Unsubscribe(long id) {
            if (subscriptions.ContainsKey(id)) {
                subscriptions.Remove(id);
            }
        }

        public void OnUpdate() {
            while (messages.TryDequeue(out (Type, IEvent) tpl)) {
                subscriptions.Values.ForEach(action => action(tpl.Item2));
            }
        }
    }
}

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSCore.Interfaces;
using UnityEngine;
using OSCore.Utils;
using System;
using OSCore.Events.Brains;

namespace OSBE.Brains {
    public class ControllerBrainFactory : IControllerBrainManager {
        readonly IDictionary<ISet<string>, IControllerBrain> brains;

        public ControllerBrainFactory() {
            brains = new Dictionary<ISet<string>, IControllerBrain>();
        }

        public IControllerBrain Ensure(Transform transform, ISet<string> tags) {
            IControllerBrain brain = brains.Get(tags);

            if (brain is null) {
                brain = Create(transform, tags);
                brains[tags] = brain;
            }

            return brain;
        }

        public void Update(IGameSystem session) =>
            brains.ForEach(brain => brain.Value.Update(session));

        public void OnMessage(ISet<string> tags, IMessage message) =>
            brains.Get(tags)?.OnMessage(message);

        public void OnMessageSync(ISet<string> tags, IMessage message) =>
            brains.Get(tags)?.OnMessageSync(message);

        IControllerBrain Create(Transform transform, ISet<string> tags) {
            if (tags.Contains("player")) {
                IControllerBrain brain = new PlayerControllerBrain(transform);
                return brain;
            }
            return default;
        }
    }

    public abstract class AControllerBrain<M> : IControllerBrain
        where M : IMessage {

        static readonly string EX_MSG =
            "This controller only accepts message of type " + typeof(M);
        readonly ConcurrentQueue<M> requests;
        readonly ConcurrentQueue<Action> responses;

        public AControllerBrain() {
            requests = new();
            responses = new();
            new Task(() => {
                while (requests.TryDequeue(out M message))
                    responses.Enqueue(ProcessMessage(message));
            }).Start();
        }

        public void OnMessage(IMessage message) =>
            OnMessageImpl(message, requests.Enqueue);

        public void OnMessageSync(IMessage message) =>
            OnMessageImpl(message, msg => ProcessMessage(msg)());

        public void Update(IGameSystem session) {
            while (responses.TryDequeue(out Action action))
                action();
        }

        void OnMessageImpl(IMessage message, Action<M> action) {
            if (message.GetType().IsSubclassOf(typeof(M)))
                action((M)message);
            else throw new InvalidOperationException(EX_MSG);
        }

        internal abstract Action ProcessMessage(M message);
    }
}



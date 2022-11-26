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
    public class ControllerBrainFactory : IControllerBrainFactory {
        ISet<IControllerBrain> brains;

        public ControllerBrainFactory() {
            brains = new HashSet<IControllerBrain>();
        }

        public IControllerBrain Create(Transform transform, ISet<string> tags) {
            if (tags.Contains("player")) {
                IControllerBrain brain = new PlayerControllerBrain(transform);
                brains.Add(brain);
                return brain;
            }
            return default;
        }

        public void Update(IGameSystem session) {
            brains.ForEach(brain => brain.Update(session));
        }
    }

    public abstract class AControllerBrain<M> : IControllerBrain
        where M : IMessage {

        static string EX_MSG = "This controller only accepts message of type " + typeof(M);

        ConcurrentQueue<M> requests;
        ConcurrentQueue<Action> responses;

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



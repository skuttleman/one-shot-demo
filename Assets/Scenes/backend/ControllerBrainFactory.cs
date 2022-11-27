using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OSCore.Interfaces;
using UnityEngine;
using OSCore.Utils;
using OSCore.Events.Brains;

namespace OSBE.Brains {
    public class ControllerBrainFactory : IControllerBrainManager {
        readonly IDictionary<EControllerBrainTag, IControllerBrain> brains;

        public ControllerBrainFactory() {
            brains = new Dictionary<EControllerBrainTag, IControllerBrain>();
        }

        public IControllerBrain Ensure(Transform transform, EControllerBrainTag tag) {
            if (brains.ContainsKey(tag))
                return brains[tag];

            Debug.Log("CREATING BRAIN");
            IControllerBrain brain = Create(transform, tag);
            brains[tag] = brain;
            return brain;
        }

        public void Update(IGameSystem session) =>
            brains.ForEach(brain => brain.Value.Update(session));

        public void OnMessage(EControllerBrainTag tag, IEvent message) =>
                    brains.Get(tag)?.OnMessage(message);

        IControllerBrain Create(Transform transform, EControllerBrainTag tag) =>
            tag switch {
                EControllerBrainTag.PLAYER => new PlayerControllerBrain(transform),
                _ => default
            };
    }
}

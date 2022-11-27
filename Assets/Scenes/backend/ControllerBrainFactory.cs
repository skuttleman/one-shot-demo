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

        public void Update(IGameSystem session) {
            brains.ForEach(brain => brain.Value.Update(session));
        }

        public void OnMessage(ISet<string> tags, IEvent message) =>
            brains.Get(tags)?.OnMessage(message);

        IControllerBrain Create(Transform transform, ISet<string> tags) {
            if (tags.Contains("player")) {
                IControllerBrain brain = new PlayerControllerBrain(transform);
                return brain;
            }
            return default;
        }
    }

    public abstract class AControllerBrain<M> {
    }
}



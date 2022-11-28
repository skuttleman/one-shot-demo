using System;
using System.Collections.Generic;
using UnityEngine;
using OSCore.Utils;
using OSCore.Events.Brains;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;

namespace OSBE.Brains {
    public class ControllerBrainManager : IControllerBrainManager {
        readonly IDictionary<EControllerBrainTag, IControllerBrain> brains;
        IGameSystem system;

        public ControllerBrainManager(IGameSystem system) {
            brains = new Dictionary<EControllerBrainTag, IControllerBrain>();
            this.system = system;
        }

        public IControllerBrain Ensure(Transform target, EControllerBrainTag tag) {
            if (brains.ContainsKey(tag))
                return brains[tag];

            IControllerBrain brain = Create(target, tag);
            brains[tag] = brain;
            return brain;
        }

        public void Update() =>
            brains.ForEach(brain => brain.Value.Update());

        public void OnMessage(EControllerBrainTag tag, IEvent message) =>
            brains.Get(tag)?.OnMessage(message);

        IControllerBrain Create(Transform target, EControllerBrainTag tag) =>
            tag switch {
                EControllerBrainTag.PLAYER => new PlayerControllerBrain(system, target),
                EControllerBrainTag.CAMERA => new CameraControllerBrain(system, target),
                _ => default
            };

        public void OnDestroy() {
            brains.ForEach(brain => brains.Remove(brain.Key));
        }
    }
}

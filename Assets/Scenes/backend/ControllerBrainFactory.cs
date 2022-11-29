using System;
using System.Collections.Generic;
using UnityEngine;
using OSCore.Utils;
using OSCore.Events.Brains;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;

namespace OSBE.Brains {
    public class ControllerBrainManager : IControllerBrainManager {
        readonly IDictionary<BrainId, IControllerBrain> brains;
        IGameSystem system;

        public ControllerBrainManager(IGameSystem system) {
            brains = new Dictionary<BrainId, IControllerBrain>();
            this.system = system;
        }

        public IControllerBrain Ensure(BrainId id, Transform target) {
            if (brains.ContainsKey(id))
                return brains.Get(id);

            IControllerBrain brain = Create(id, target);
            brains.Add(id, brain);
            return brain;
        }

        public void Update() =>
            brains.ForEach(brain => brain.Value.Update());

        public void OnMessage(BrainId id, IEvent message) =>
            brains.Get(id)?.OnMessage(message);

        IControllerBrain Create(BrainId id, Transform target) =>
            id.tag switch {
                EControllerBrainTag.PLAYER => new PlayerControllerBrain(system, target),
                EControllerBrainTag.CAMERA => new CameraControllerBrain(system, target),
                _ => default
            };

        public void OnDestroy() =>
            brains.ForEach(brain => brains.Remove(brain.Key));
    }
}

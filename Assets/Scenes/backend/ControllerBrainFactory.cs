using System;
using System.Collections.Generic;
using UnityEngine;
using OSCore.Utils;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;

namespace OSBE.Brains {
    public class ControllerBrainManager : IControllerBrainManager {
        readonly IDictionary<(Transform, EControllerBrainTag), IControllerBrain> brains;
        readonly IGameSystem system;

        public ControllerBrainManager(IGameSystem system) {
            brains = new Dictionary<(Transform, EControllerBrainTag), IControllerBrain>();
            this.system = system;
        }

        public IControllerBrain Ensure(EControllerBrainTag tag, Transform target) {
            (Transform, EControllerBrainTag) id = (target, tag);
            if (brains.ContainsKey(id))
                return brains.Get(id);

            IControllerBrain brain = Create(tag, target);
            brains.Add(id, brain);
            return brain;
        }

        public void Update() =>
            brains.ForEach(brain => brain.Value.Update());

        IControllerBrain Create(EControllerBrainTag tag, Transform target) =>
            tag switch {
                EControllerBrainTag.PLAYER => new PlayerControllerBrain(system, target),
                EControllerBrainTag.CAMERA => new CameraControllerBrain(system, target),
                EControllerBrainTag.SPA => new SPABrain(system, target),
                _ => default
            };

        public void OnDestroy() =>
            brains.ForEach(brain => brains.Remove(brain.Key));
    }
}

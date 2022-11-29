using System;
using System.Collections.Generic;
using UnityEngine;
using OSCore.Utils;
using OSCore.Interfaces.Brains;
using OSCore.Interfaces;

namespace OSBE.Brains {
    public class ControllerBrainManager : IControllerBrainManager {
        internal record BrainId(EControllerBrainTag tag) {
            internal record UniqueId(EControllerBrainTag tag) : BrainId(tag);
            internal record InstanceId(Transform transform, EControllerBrainTag tag) : BrainId(tag);
        }

        readonly IDictionary<BrainId, IControllerBrain> brains;
        IGameSystem system;

        public ControllerBrainManager(IGameSystem system) {
            brains = new Dictionary<BrainId, IControllerBrain>();
            this.system = system;
        }

        public IControllerBrain EnsureUnique(EControllerBrainTag tag, Transform target) =>
            Ensure(new BrainId.UniqueId(tag), target);

        public IControllerBrain Ensure(EControllerBrainTag tag, Transform target) =>
            Ensure(new BrainId.InstanceId(target, tag), target);

        IControllerBrain Ensure(BrainId id, Transform target) {
            if (brains.ContainsKey(id))
                return brains.Get(id);

            IControllerBrain brain = Create(id.tag, target);
            brains.Add(id, brain);
            return brain;
        }

        public void Update() =>
            brains.ForEach(brain => brain.Value.Update());

        IControllerBrain Create(EControllerBrainTag tag, Transform target) =>
            tag switch {
                EControllerBrainTag.PLAYER => new PlayerControllerBrain(system, target),
                EControllerBrainTag.CAMERA => new CameraControllerBrain(system, target),
                _ => default
            };

        public void OnDestroy() =>
            brains.ForEach(brain => brains.Remove(brain.Key));
    }
}

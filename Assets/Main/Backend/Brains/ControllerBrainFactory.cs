using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSBE.Brains {
    public class ControllerBrainManager : IControllerBrainManager {
        readonly IDictionary<(int, Type), IGameSystemComponent> brains;
        readonly IGameSystem system;

        public ControllerBrainManager(IGameSystem system) {
            brains = new Dictionary<(int, Type), IGameSystemComponent>();
            this.system = system;
        }

        public T Ensure<T>(Transform target) where T : IGameSystemComponent {
            (int, Type) id = (target.GetHashCode(), typeof(T));

            if (brains.ContainsKey(id))
                return (T)brains.Get(id);

            T brain = (T)Create<T>(target);
            brains.Add(id, brain);
            return brain;
        }

        public void Update() =>
            brains.ForEach(brain => brain.Value.Update());

        public void FixedUpdate() =>
            brains.ForEach(brain => brain.Value.FixedUpdate());

        IGameSystemComponent Create<T>(Transform target) {
            Type t = typeof(T);
            if (t == typeof(IPlayerControllerBrain))
                return new PlayerControllerBrain(system, target);
            if (t == typeof(IPlayerFOVBrain))
                return new PlayerFOVBrain(system, target);

            if (t == typeof(IEnemyControllerBrain))
                return new EnemyControllerBrain(system, target);

            if (t == typeof(ICameraControllerBrain))
                return new CameraControllerBrain(system, target);
            throw new Exception("Unknown Brain Type: " + t);
        }

        public void OnDestroy() =>
            brains.ForEach(brain => brains.Remove(brain.Key));
    }
}

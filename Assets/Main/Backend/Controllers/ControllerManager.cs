using OSCore.System.Interfaces.Brains;
using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSBE.Controllers {
    public class ControllerManager : IControllerManager {
        readonly IDictionary<(int, Type), IGameSystemComponent> brains;
        readonly IGameSystem system;

        public ControllerManager(IGameSystem system) {
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

        IGameSystemComponent Create<T>(Transform target) {
            Type t = typeof(T);
            if (t == typeof(IPlayerStateReducer))
                return new PlayerStateReducer(system, target);
            if (t == typeof(IPlayerFOVController))
                return new PlayerFOVController(system, target);

            if (t == typeof(IEnemyStateReducer))
                return new EnemyStateReducer(system, target);

            if (t == typeof(ICameraController))
                return new CameraController(system, target);
            if (t == typeof(ICameraOverlayController))
                return new CameraOverlayController(system, target);


            throw new Exception("Unknown Brain Type: " + t);
        }

        public void OnStart() =>
            brains?.ForEach(brain => brain.Value.OnStart());

        public void OnUpdate() =>
            brains?.ForEach(brain => brain.Value.OnUpdate());

        public void OnFixedUpdate() =>
            brains?.ForEach(brain => brain.Value.OnFixedUpdate());

        public void OnDestroy() {
            brains?.ForEach(brain => brain.Value.OnDestroy());
        }
    }
}

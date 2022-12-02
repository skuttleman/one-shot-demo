using System;
using System.Collections.Generic;
using OSCore;
using OSCore.Events.Brains;
using OSCore.Interfaces;
using OSCore.Interfaces.Brains;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Events.Brains.SPA.SPAEvent;

namespace OSBE {
    public class SPABrain : AControllerBrain<MoveSPA, FaceSPA, InitEvent<GravityCfgSO>, InitEvent<CollisionCfgSO>> {
        readonly IGameSystem system;
        readonly Transform target;
        GravityCfgSO gravity;
        CollisionCfgSO collision;

        public SPABrain(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
        }

        public override void Handle(MoveSPA e) {
            if (Vectors.NonZero(e.direction))
                target.position += e.speed * Time.deltaTime * Vectors.Upgrade(e.direction);
        }

        public override void Handle(FaceSPA e) {
            float rotationZ = Vectors.AngleTo(Vector2.zero, e.direction);

            target.rotation = Quaternion.Lerp(
                    target.rotation,
                    Quaternion.Euler(0f, 0f, rotationZ),
                    e.speed * Time.deltaTime);
        }

        public override void Handle(InitEvent<GravityCfgSO> e) =>
            gravity = e.cfg;

        public override void Handle(InitEvent<CollisionCfgSO> e) =>
            collision = e.cfg;

        public override void Update() { }
    }
}

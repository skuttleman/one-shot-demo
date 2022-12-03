using OSCore.Data.Events.Brains;
using OSCore.ScriptableObjects;
using OSCore.System.Interfaces;
using OSCore.System;
using OSCore.Utils;
using UnityEngine;
using static OSCore.Data.Events.Brains.SPA.SPAEvent;

namespace OSBE.Brains {
    public class SPABrain : AControllerBrain
        <MoveSPA, FaceSPA, InitEvent<GravityCfgSO>, InitEvent<CollisionCfgSO>> {
        readonly IGameSystem system;
        readonly Transform target;
        GravityCfgSO gravity;
        CollisionCfgSO collision;
        Rigidbody rb = null;

        public SPABrain(IGameSystem system, Transform target) {
            this.system = system;
            this.target = target;
        }

        public override void Handle(MoveSPA e) {
            if (rb is null)
                rb = target.GetComponent<Rigidbody>();

            Vector3 dir = e.speed * /*Time.fixedDeltaTime **/ e.direction.Upgrade();

            if (Vectors.NonZero(e.direction))
                rb.AddForce(dir);
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

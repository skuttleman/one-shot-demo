using OSCore.System;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public class BNodeGoto : AStateNode {
        private readonly EnemyNavAgent nav;
        private readonly Vector3 location;
        private bool isStarted;

        public BNodeGoto(Transform transform, Vector3 location) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.location = location;
        }

        protected override StateNodeStatus ProcessImpl() {
            if (Vector3.Distance(transform.position, location) < 0.2f) {
                return StateNodeStatus.SUCCESS;
            } else if (!isStarted && nav.Goto(location)) {
                isStarted = true;
                return StateNodeStatus.RUNNING;
            } else if (!isStarted) {
                return StateNodeStatus.FAILURE;
            } else if (!nav.isMoving) {
                return StateNodeStatus.SUCCESS;
            }
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            isStarted = false;
            nav.Stop();
        }
    }

    public abstract class ABNodeLookAt : AStateNode {
        private readonly EnemyNavAgent nav;
        private readonly Func<Vector3> locationFn;
        private bool isStarted;

        public ABNodeLookAt(Transform transform, Func<Vector3> locationFn) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.locationFn = locationFn;
        }

        protected override StateNodeStatus ProcessImpl() {
            if (!isStarted) {
                isStarted = true;
                nav.Face(locationFn());
            } else if (!nav.isTurning) {
                return StateNodeStatus.SUCCESS;
            }
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            isStarted = false;
            nav.Stop();
        }
    }

    public class BNodeLookAtTransform : ABNodeLookAt {
        public BNodeLookAtTransform(Transform transform, Transform target)
            : base(transform, () => target.position) { }
    }

    public class BNodeLookAtLocation : ABNodeLookAt {
        public BNodeLookAtLocation(Transform transform, Vector3 location)
            : base(transform, () => location) { }
    }

    public class BNodeLookAtDirection : ABNodeLookAt {
        public BNodeLookAtDirection(Transform transform, Vector3 direction)
            : base(transform, () => transform.position + direction.normalized) { }
    }
}

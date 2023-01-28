using OSCore.System;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public class BNodeGoto : AStateNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Vector3 location;
        private bool isStarted;

        public BNodeGoto(Transform transform, Vector3 location) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.location = location;
        }

        public override StateNodeStatus Process(EnemyAIStateDetails details) {
            if (Vector3.Distance(transform.position, location) < 0.2f) {
                return StateNodeStatus.SUCCESS;
            } else if (!isStarted && nav.Goto(location, details.cfg)) {
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

    public class BNodeLookAt : AStateNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private bool isStarted;

        public BNodeLookAt(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        public override StateNodeStatus Process(EnemyAIStateDetails details) {
            if (!isStarted) {
                isStarted = true;
                nav.Face(toLocation(details), details.cfg);
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

    public class BNodeLookAtTransform : BNodeLookAt {
        public BNodeLookAtTransform(Transform transform, Transform target)
            : base(transform, _ => target.position) { }
    }

    public class BNodeLookAtLocation : BNodeLookAt {
        public BNodeLookAtLocation(Transform transform, Vector3 location)
            : base(transform, _ => location) { }
    }

    public class BNodeLookAtDirection : BNodeLookAt {
        public BNodeLookAtDirection(Transform transform, Vector3 direction)
            : base(transform, _ => transform.position + direction.normalized) { }
    }

    public class BNodeLookAtLKL : BNodeLookAt {
        public BNodeLookAtLKL(Transform transform)
            : base(transform, details => details.lastKnownPosition) { }
    }
}

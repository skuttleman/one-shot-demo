using OSCore.System;
using OSCore.Utils;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public class BNodeGoto : ABehaviorNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private float destElapsed = 0f;
        private Vector3 destination;

        public BNodeGoto(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Start(EnemyAIStateDetails details) {
            StartWalk(details, toLocation(details));
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!nav.isMoving || Vector3.Distance(transform.position, destination) < 0.1f) {
                nav.Stop();
                status = StateNodeStatus.SUCCESS;
            } else if (destElapsed <= 0f) {
                Vector3 loc = toLocation(details);
                destElapsed = 0.5f;

                if (Vectors.NonZero(loc - destination) || !nav.isMoving) {
                    StartWalk(details, loc);
                }
            }

            destElapsed -= Time.deltaTime;
        }

        protected override void Stop() {
            destination = Vector3.negativeInfinity;
            nav.Stop();
        }

        private void StartWalk(EnemyAIStateDetails details, Vector3 location) {
            destination = location;

            if (!nav.Goto(destination, details.cfg)) {
                status = StateNodeStatus.FAILURE;
            }
        }
    }

    public class BNodeGotoStatic : ABehaviorNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private Vector3 destination;

        public BNodeGotoStatic(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Start(EnemyAIStateDetails details) {
            destination = toLocation(details);

            if (!nav.Goto(destination, details.cfg)) {
                status = StateNodeStatus.FAILURE;
            }
        }

        protected override void Continue(EnemyAIStateDetails details) {
            float distance = Vector3.Distance(transform.position, destination);

            if (!nav.isMoving || distance < 0.1f) {
                Stop();
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Stop() {
            nav.Stop();
        }
    }

    public class BNodeGotoLocation : BNodeGoto {
        public BNodeGotoLocation(Transform transform, Vector3 location)
            : base(transform, _ => location) { }
    }

    public class BNodeLookAt : ABehaviorNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private bool isStarted;

        public BNodeLookAt(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation)
            : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Start(EnemyAIStateDetails details) {
            nav.Face(toLocation(details), details.cfg);
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!nav.isTurning) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Stop() {
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

    public class BNodeLookAtLKP : BNodeLookAt {
        public BNodeLookAtLKP(Transform transform)
            : base(transform, details => details.lastKnownPosition) { }
    }

    public class BNodeSpeak : ABehaviorNode<EnemyAIStateDetails> {
        private readonly EnemySpeechAgent speech;
        private readonly string message;

        public BNodeSpeak(Transform transform, string message) : base(transform) {
            speech = transform.GetComponent<EnemySpeechAgent>();
            this.message = message;
        }

        protected override void Start(EnemyAIStateDetails details) {
            speech.Say(message);
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!speech.isSpeaking) {
                Stop();
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Stop() {
            speech.Stop();
        }
    }
}

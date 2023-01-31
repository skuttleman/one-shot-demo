using OSCore.System;
using OSCore.Utils;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public class BNodeGoto : AStateNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private float destElapsed = 0f;
        private Vector3 destination;

        public BNodeGoto(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Init() {
            destination = Vector3.negativeInfinity;
            nav.Stop();
        }

        protected override void Process(EnemyAIStateDetails details) {
            status = StateNodeStatus.RUNNING;

            if (Vector3.Distance(transform.position, destination) < 0.1f) {
                nav.Stop();
                status = StateNodeStatus.SUCCESS;
            } else if (destElapsed <= 0f) {
                Vector3 loc = toLocation(details);
                destElapsed = 0.5f;

                if (Vectors.NonZero(loc - destination) || !nav.isMoving) {
                    destination = loc;

                    if (!nav.Goto(destination, details.cfg)) {
                        status = StateNodeStatus.FAILURE;
                    }
                }
            }

            destElapsed -= Time.deltaTime;
        }
    }

    public class BNodeGotoLocation : BNodeGoto {
        public BNodeGotoLocation(Transform transform, Vector3 location)
            : base(transform, _ => location) { }
    }

    public class BNodeLookAt : AStateNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private bool isStarted;

        public BNodeLookAt(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Process(EnemyAIStateDetails details) {
            status = StateNodeStatus.RUNNING;
            if (!isStarted) {
                isStarted = true;
                nav.Face(toLocation(details), details.cfg);
            } else if (!nav.isTurning) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Init() {
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

    public class BNodeLookAtLKP : BNodeLookAt {
        public BNodeLookAtLKP(Transform transform)
            : base(transform, details => details.lastKnownPosition) { }
    }

    public class BNodeSpeak : AStateNode<EnemyAIStateDetails> {
        private readonly EnemySpeechAgent speech;
        private readonly string message;
        private bool isStarted;

        public BNodeSpeak(Transform transform, string message) : base(transform) {
            speech = transform.GetComponent<EnemySpeechAgent>();
            this.message = message;
        }

        protected override void Process(EnemyAIStateDetails details) {
            status = StateNodeStatus.RUNNING;

            if (!isStarted) {
                speech.Say(message);
                isStarted = true;
            } else if (!speech.isSpeaking) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Init() {
            isStarted = false;
            speech.Stop();
        }
    }
}

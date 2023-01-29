using OSCore.System;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public class BNodeGoto : AStateNode<EnemyAIStateDetails> {
        private readonly EnemyNavAgent nav;
        private readonly Func<EnemyAIStateDetails, Vector3> toLocation;
        private bool isStarted;

        public BNodeGoto(Transform transform, Func<EnemyAIStateDetails, Vector3> toLocation) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
            this.toLocation = toLocation;
        }

        protected override void Process(EnemyAIStateDetails details) {
            status = StateNodeStatus.RUNNING;
            if (Vector3.Distance(transform.position, toLocation(details)) < 0.2f) {
                status = StateNodeStatus.SUCCESS;
            } else if (!isStarted && nav.Goto(toLocation(details), details.cfg)) {
                isStarted = true;
                status = StateNodeStatus.RUNNING;
            } else if (!isStarted) {
                status = StateNodeStatus.FAILURE;
            } else if (!nav.isMoving) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            isStarted = false;
            nav.Stop();
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

        public override void Init() {
            status = StateNodeStatus.RUNNING;
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
            } else if (!speech.isSpeaking || speech.message != message) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            isStarted = false;
            speech.Stop();
        }
    }
}

using OSCore.System;
using OSCore.Utils;
using System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Actions {
    public abstract class ABNodeGoto : ABehaviorNode<EnemyAIStateDetails> {
        private EnemyNavAgent nav;
        private Vector3 destination;
        private float destElapsed;

        protected ABNodeGoto(Transform transform) : base(transform) {
            nav = transform.GetComponent<EnemyNavAgent>();
        }

        protected abstract Vector3 ToLocation(EnemyAIStateDetails details);

        protected override void Start(EnemyAIStateDetails details) {
            destElapsed = 0f;
            StartWalk(details, ToLocation(details));
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!nav.isMoving || Vector3.Distance(transform.position, destination) < 0.1f) {
                nav.Stop();
                status = StateNodeStatus.SUCCESS;
            } else if (destElapsed <= 0f) {
                Vector3 loc = ToLocation(details);
                destElapsed = 0.5f;

                if (Vectors.NonZero(loc - destination) || !nav.isMoving) {
                    StartWalk(details, loc);
                }
            }

            destElapsed -= Time.deltaTime;
        }

        protected override void Stop() {
            transform.GetComponent<EnemyNavAgent>().Stop();
        }

        private void StartWalk(EnemyAIStateDetails details, Vector3 location) {
            destination = location;

            if (!nav.Goto(destination, details.cfg)) {
                status = StateNodeStatus.FAILURE;
            }
        }
    }

    public class BNodeGoto : ABNodeGoto {
        public static IBehaviorNodeFactory<EnemyAIStateDetails> Of(
            Func<Transform, EnemyAIStateDetails, Vector3> toLocation) =>
                new BehaviorNodeFactory<EnemyAIStateDetails>(
                    transform => new BNodeGoto(transform, toLocation));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> Of(Vector3 location) =>
            new BehaviorNodeFactory<EnemyAIStateDetails>(
                transform => new BNodeGoto(transform, (_, _) => location));

        private readonly Func<Transform, EnemyAIStateDetails, Vector3> toLocation;

        protected BNodeGoto(Transform transform, Func<Transform, EnemyAIStateDetails, Vector3> toLocation)
            : base(transform) {
            this.toLocation = toLocation;
        }

        protected override Vector3 ToLocation(EnemyAIStateDetails details) =>
            toLocation(transform, details);

    }

    public class BNodeGotoStatic : ABNodeGoto {
        public static IBehaviorNodeFactory<EnemyAIStateDetails> Of(
            Func<Transform, EnemyAIStateDetails, Vector3> toLocation) =>
                new BehaviorNodeFactory<EnemyAIStateDetails>(
                    transform => new BNodeGotoStatic(transform, toLocation));

        private readonly Func<Transform, EnemyAIStateDetails, Vector3> toLocation;
        private Vector3 destination;

        protected BNodeGotoStatic(Transform transform, Func<Transform, EnemyAIStateDetails, Vector3> toLocation)
            : base (transform) {
            destination = Vector3.negativeInfinity;
            this.toLocation = toLocation;
        }

        protected override Vector3 ToLocation(EnemyAIStateDetails details) =>
            destination.IsNegativeInfinity()
                ? destination = toLocation(transform, details)
                : destination;
    }

    public class BNodeGotoLocation : BNodeGoto {
        public static new IBehaviorNodeFactory<EnemyAIStateDetails> Of(Vector3 location) =>
            new BehaviorNodeFactory<EnemyAIStateDetails>(
                transform => new BNodeGotoLocation(transform, location));

        protected BNodeGotoLocation(Transform transform, Vector3 location) : base(transform, (_, _) => location) { }
    }

    public abstract class ABNodeLookAt : ABehaviorNode<EnemyAIStateDetails> {
        private EnemyNavAgent nav;

        protected ABNodeLookAt(Transform transform) : base(transform) { }

        protected abstract Vector3 ToLocation(EnemyAIStateDetails details);

        protected override void Start(EnemyAIStateDetails details) {
            nav = transform.GetComponent<EnemyNavAgent>();
            nav.Face(ToLocation(details), details.cfg);
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!nav.isTurning) {
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Stop() {
            if (nav is not null) nav.Stop();
        }
    }

    public class BNodeLookAt : ABNodeLookAt {
        public static IBehaviorNodeFactory<EnemyAIStateDetails> Of(
            Func<Transform, EnemyAIStateDetails, Vector3> toLocation) =>
                new BehaviorNodeFactory<EnemyAIStateDetails>(
                    transform => new BNodeLookAt(transform, toLocation));

        public static IBehaviorNodeFactory<EnemyAIStateDetails> LKP() =>
            new BehaviorNodeFactory<EnemyAIStateDetails>(transform =>
                new BNodeLookAt(transform, (transform, details) => details.lastKnownPosition));

        private readonly Func<Transform, EnemyAIStateDetails, Vector3> toLocation;

        protected BNodeLookAt(Transform transform, Func<Transform, EnemyAIStateDetails, Vector3> toLocation)
            : base(transform) {
            this.toLocation = toLocation;
        }

        protected override Vector3 ToLocation(EnemyAIStateDetails details) =>
            toLocation(transform, details);
    }

    public class BNodeSpeak : ABehaviorNode<EnemyAIStateDetails> {
        public static IBehaviorNodeFactory<EnemyAIStateDetails> Of(string message) =>
            new BehaviorNodeFactory<EnemyAIStateDetails>(
                transform => new BNodeSpeak(transform, message));

        private readonly string message;
        private EnemySpeechAgent speech;

        protected BNodeSpeak(Transform transform, string message) : base(transform) {
            this.message = message;
        }

        protected override void Start(EnemyAIStateDetails details) {
            speech = transform.GetComponent<EnemySpeechAgent>();
            speech.Say(message);
        }

        protected override void Continue(EnemyAIStateDetails details) {
            if (!speech.isSpeaking) {
                Stop();
                status = StateNodeStatus.SUCCESS;
            }
        }

        protected override void Stop() {
            if (speech is not null) speech.Stop();
        }
    }

    public class BNodeWait<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(float time) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeWait<T>(transform, time));

        private readonly float time;
        private float elapsed;

        protected BNodeWait(Transform transform, float time) : base(transform) {
            this.time = time;
            elapsed = 0f;
        }

        protected override void Continue(T details) {
            status = StateNodeStatus.RUNNING;

            if (elapsed >= time) {
                status = StateNodeStatus.SUCCESS;
            }

            elapsed += Time.deltaTime;
        }

        protected override void Stop() {
            elapsed = 0f;
        }
    }
}

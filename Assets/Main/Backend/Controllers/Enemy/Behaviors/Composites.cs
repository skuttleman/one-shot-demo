using System;
using System.Linq;
using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(params IBehaviorNodeFactory<T>[] nodes) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeAnd<T>(
                    transform,
                    BehaviorNodeFactory<T>.CreateAll(transform, nodes)));

        protected readonly ABehaviorNode<T>[] nodes;
        protected int curr;

        protected BNodeAnd(Transform transform, params ABehaviorNode<T>[] nodes)
            : base(transform) {
            this.nodes = nodes;
            curr = 0;
        }

        protected override void Continue(T details) {
            if (curr >= nodes.Length) {
                status = StateNodeStatus.SUCCESS;
                return;
            }

            ABehaviorNode<T> node = nodes[curr];
            node.Process(details);

            switch (node.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    curr++;
                    break;
            }
        }

        protected override void Stop() {
            curr = 0;
            foreach (ABehaviorNode<T> node in nodes) node.ReInit();
        }
    }

    public class BNodeOr<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(params IBehaviorNodeFactory<T>[] nodes) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeOr<T>(
                    transform,
                    BehaviorNodeFactory<T>.CreateAll(transform, nodes)));

        private readonly ABehaviorNode<T>[] nodes;
        private int curr;

        protected BNodeOr(Transform transform, params ABehaviorNode<T>[] nodes)
            : base(transform) {
            this.nodes = nodes;
            curr = 0;
        }

        protected override void Continue(T details) {
            if (curr >= nodes.Length) {
                status = StateNodeStatus.FAILURE;
                return;
            }

            ABehaviorNode<T> node = nodes[curr];
            node.Process(details);

            switch (node.status) {
                case StateNodeStatus.SUCCESS:
                    status = StateNodeStatus.SUCCESS;
                    break;
                case StateNodeStatus.FAILURE:
                    curr++;
                    break;
            }

            if (curr >= nodes.Length) status = StateNodeStatus.FAILURE;
        }

        protected override void Stop() {
            curr = 0;
            foreach (ABehaviorNode<T> node in nodes) node.ReInit();
        }
    }

    public class BNodeOptional<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(IBehaviorNodeFactory<T> child) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeOptional<T>(
                    transform,
                    child.Create(transform)));

        private readonly ABehaviorNode<T> child;

        protected BNodeOptional(Transform transform, ABehaviorNode<T> child)
            : base(transform) {
            this.child = child;
        }

        protected override void Continue(T details) {
            child.Process(details);
            status = child.status == StateNodeStatus.FAILURE
                ? StateNodeStatus.SUCCESS
                : child.status;
        }

        protected override void Stop() {
            child.ReInit();
        }
    }

    public class BNodeDoFor<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(float time, IBehaviorNodeFactory<T> child) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeDoFor<T>(
                    transform,
                    time,
                    child.Create(transform)));

        private readonly ABehaviorNode<T> child;
        private readonly float time;
        private float elapsed;

        protected BNodeDoFor(Transform transform, float time, ABehaviorNode<T> node)
            : base(transform) {
            child = node;
            this.time = time;
            elapsed = 0f;
        }

        protected override void Continue(T details) {
            if (elapsed >= time) return;

            elapsed += Time.deltaTime;
            child.Process(details);
            status = child.status;
        }

        protected override void Stop() {
            elapsed = 0f;
            child.ReInit();
        }
    }

    public class BNodeDoWhile<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(Predicate<Transform, T> pred, IBehaviorNodeFactory<T> child) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeDoWhile<T>(
                    transform,
                    pred,
                    child.Create(transform)));

        private readonly ABehaviorNode<T> child;
        private readonly Predicate<Transform, T> pred;

        protected BNodeDoWhile(Transform transform, Predicate<Transform, T> pred, ABehaviorNode<T> child)
            : base(transform) {
            this.pred = pred;
            this.child = child;
        }

        protected override void Continue(T details) {
            if (!pred(transform, details)) {
                status = StateNodeStatus.SUCCESS;
                return;
            }

            child.Process(details);
            status = child.status;
        }

        protected override void Stop() {
            child.ReInit();
        }
    }

    public class BNodeRepeat<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(IBehaviorNodeFactory<T> child) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeRepeat<T>(
                    transform,
                    child.Create(transform)));

        private readonly ABehaviorNode<T> child;

        protected BNodeRepeat(Transform transform, ABehaviorNode<T> node)
            : base(transform) {
            child = node;
        }

        protected override void Continue(T details) {
            status = StateNodeStatus.RUNNING;
            child.Process(details);

            switch (child.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    child.ReInit();
                    break;
            }
        }

        protected override void Stop() {
            child.ReInit();
        }
    }

    public class BNodeParallelAll<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(params IBehaviorNodeFactory<T>[] nodes) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeParallelAll<T>(
                    transform,
                    BehaviorNodeFactory<T>.CreateAll(transform, nodes)));

        private readonly ABehaviorNode<T>[] children;

        protected BNodeParallelAll(Transform transform, params ABehaviorNode<T>[] children)
            : base(transform) {
            this.children = children;
        }

        protected override void Continue(T details) {
            if (status == StateNodeStatus.FAILURE) return;
            bool running = false;

            foreach (ABehaviorNode<T> child in children) {
                child.Process(details);

                if (child.status == StateNodeStatus.FAILURE) {
                    status = StateNodeStatus.FAILURE;
                    return;
                } else if (child.status == StateNodeStatus.RUNNING) {
                    running = true;
                }
            }

            status = running ? StateNodeStatus.RUNNING : StateNodeStatus.SUCCESS;
        }

        protected override void Stop() {
            foreach (ABehaviorNode<T> child in children) child.ReInit();
        }
    }

    public class BNodeParallelAny<T> : ABehaviorNode<T> {
        public static IBehaviorNodeFactory<T> Of(params IBehaviorNodeFactory<T>[] nodes) =>
            new BehaviorNodeFactory<T>(
                transform => new BNodeParallelAny<T>(
                    transform,
                    BehaviorNodeFactory<T>.CreateAll(transform, nodes)));

        private readonly ABehaviorNode<T>[] children;

        protected BNodeParallelAny(Transform transform, params ABehaviorNode<T>[] children)
            : base(transform) {
            this.children = children;
        }

        protected override void Continue(T details) {
            foreach (ABehaviorNode<T> child in children) {
                child.Process(details);
                status = child.status;

                if (status != StateNodeStatus.RUNNING) return;
            }
        }

        protected override void Stop() {
            foreach (ABehaviorNode<T> child in children) child.ReInit();
        }
    }

    namespace Experimental {
        public class BNodeStepLockedAnd<T> : BNodeAnd<T> {
            public static new IBehaviorNodeFactory<T> Of(params IBehaviorNodeFactory<T>[] nodes) =>
                new BehaviorNodeFactory<T>(
                    transform => new BNodeStepLockedAnd<T>(
                        transform,
                        BehaviorNodeFactory<T>.CreateAll(transform, nodes)));

            protected BNodeStepLockedAnd(Transform transform, params ABehaviorNode<T>[] nodes)
                : base(transform, nodes) { }

            protected override void Stop() {
                curr++;
                if (curr >= nodes.Length) curr = 0;
                foreach (ABehaviorNode<T> node in nodes) node.ReInit();
            }
        }
    }
}
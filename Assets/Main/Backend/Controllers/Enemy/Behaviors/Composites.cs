using System;
using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd<T> : ABehaviorNode<T> {
        protected readonly ABehaviorNode<T>[] nodes;
        protected int curr;

        public BNodeAnd(Transform transform, params ABehaviorNode<T>[] nodes) : base(transform) {
            this.nodes = nodes;
            curr = 0;
        }

        protected override void Continue(T details) {
            if (curr >= nodes.Length) {
                status = StateNodeStatus.SUCCESS;
                return;
            }

            ABehaviorNode<T> node = nodes[curr];
            Process(node, details);

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
            foreach (ABehaviorNode<T> node in nodes) ReInit(node);
        }
    }

    public class BNodeOr<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T>[] nodes;
        private int curr;

        public BNodeOr(Transform transform, params ABehaviorNode<T>[] nodes) : base(transform) {
            this.nodes = nodes;
            curr = 0;
        }

        protected override void Continue(T details) {
            if (curr >= nodes.Length) {
                status = StateNodeStatus.FAILURE;
                return;
            }

            ABehaviorNode<T> node = nodes[curr];
            Process(node, details);

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
            foreach (ABehaviorNode<T> node in nodes) ReInit(node);
        }
    }

    public class BNodeOptional<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T> child;

        public BNodeOptional(Transform transform, ABehaviorNode<T> child)
            : base(transform) {
            this.child = child;
        }

        protected override void Continue(T details) {
            Process(child, details);
            status = child.status == StateNodeStatus.FAILURE
                ? StateNodeStatus.SUCCESS
                : child.status;
        }

        protected override void Stop() {
            ReInit(child);
        }
    }

    public class BNodeWait<T> : ABehaviorNode<T> {
        private readonly float time;
        private float elapsed;

        public BNodeWait(Transform transform, float time) : base(transform) {
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

    public class BNodeDoFor<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T> child;
        private readonly float time;
        private float elapsed;

        public BNodeDoFor(Transform transform, ABehaviorNode<T> node, float time)
            : base(transform) {
            child = node;
            this.time = time;
            elapsed = 0f;
        }

        protected override void Continue(T details) {
            if (elapsed >= time) return;

            elapsed += Time.deltaTime;
            Process(child, details);
            status = child.status;
        }

        protected override void Stop() {
            elapsed = 0f;
            ReInit(child);
        }
    }

    public class BNodeDoWhile<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T> child;
        private readonly Predicate<T> pred;

        public BNodeDoWhile(Transform transform, Predicate<T> pred, ABehaviorNode<T> child)
            : base(transform) {
            this.pred = pred;
            this.child = child;
        }

        protected override void Continue(T details) {
            if (!pred(details)) {
                status = StateNodeStatus.SUCCESS;
                return;
            }

            Process(child, details);
            status = child.status;
        }

        protected override void Stop() {
            ReInit(child);
        }
    }

    public class BNodeRepeat<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T> child;

        public BNodeRepeat(Transform transform, ABehaviorNode<T> node) : base(transform) {
            child = node;
        }

        protected override void Continue(T details) {
            status = StateNodeStatus.RUNNING;
            Process(child, details);

            switch (child.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    ReInit(child);
                    break;
            }
        }

        protected override void Stop() {
            ReInit(child);
        }
    }

    public class BNodeParallelAll<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T>[] children;

        public BNodeParallelAll(Transform transform, params ABehaviorNode<T>[] children) : base(transform) {
            this.children = children;
        }

        protected override void Continue(T details) {
            if (status == StateNodeStatus.FAILURE) return;
            bool running = false;

            foreach (ABehaviorNode<T> child in children) {
                Process(child, details);

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
            foreach (ABehaviorNode<T> child in children) ReInit(child);
        }
    }

    public class BNodeParallelAny<T> : ABehaviorNode<T> {
        private readonly ABehaviorNode<T>[] children;

        public BNodeParallelAny(Transform transform, params ABehaviorNode<T>[] children)
            : base(transform) {
            this.children = children;
        }

        protected override void Continue(T details) {
            foreach (ABehaviorNode<T> child in children) {
                Process(child, details);
                status = child.status;

                if (status != StateNodeStatus.RUNNING) return;
            }
        }

        protected override void Stop() {
            foreach (ABehaviorNode<T> child in children) ReInit(child);
        }
    }

    namespace Experimental {
        public class BNodeStepLockedAnd<T> : BNodeAnd<T> {
            public BNodeStepLockedAnd(Transform transform, params ABehaviorNode<T>[] nodes)
                : base(transform, nodes) { }

            protected override void Stop() {
                curr++;
                if (curr >= nodes.Length) curr = 0;
                foreach (ABehaviorNode<T> node in nodes) ReInit(node);
            }
        }
    }
}
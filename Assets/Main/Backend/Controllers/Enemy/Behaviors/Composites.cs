using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd<T> : AStateNode<T> {
        private readonly AStateNode<T>[] nodes;
        private int curr;

        public BNodeAnd(Transform transform, params AStateNode<T>[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override void Process(T details) {
            AStateNode<T> node = nodes[curr];
            Process(node, details);
            status = StateNodeStatus.RUNNING;

            switch (node.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    curr++;
                    break;
            }

            if (curr >= nodes.Length) status = StateNodeStatus.SUCCESS;
        }

        protected override void Init() {
            curr = 0;
        }
    }

    public class BNodeOr<T> : AStateNode<T> {
        private readonly AStateNode<T>[] nodes;
        private int curr;

        public BNodeOr(Transform transform, params AStateNode<T>[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override void Process(T details) {
            AStateNode<T> node = nodes[curr];
            Process(node, details);
            status = StateNodeStatus.RUNNING;

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

        protected override void Init() {
            curr = 0;
        }
    }

    public class BNodeWait<T> : AStateNode<T> {
        private readonly float time;
        private float elapsed;

        public BNodeWait(Transform transform, float time) : base(transform) {
            this.time = time;
        }

        protected override void Process(T details) {
            status = StateNodeStatus.RUNNING;

            if (elapsed >= time) {
                status = StateNodeStatus.SUCCESS;
            }

            elapsed += Time.deltaTime;
        }

        protected override void Init() {
            elapsed = 0f;
        }
    }

    public class BNodeDoFor<T> : AStateNode<T> {
        private readonly AStateNode<T> child;
        private readonly float time;
        private float elapsed;

        public BNodeDoFor(Transform transform, AStateNode<T> node, float time)
            : base(transform) {
            child = node;
            this.time = time;
        }

        protected override void Process(T details) {
            if (elapsed >= time) {
                return;
            }

            elapsed += Time.deltaTime;
            Process(child, details);
            status = child.status;
        }

        protected override void Init() {
            elapsed = 0f;
        }
    }

    public class BNodeRepeat<T> : AStateNode<T> {
        private readonly AStateNode<T> child;

        public BNodeRepeat(Transform transform, AStateNode<T> node) : base(transform) {
            child = node;
        }

        protected override void Process(T details) {
            Process(child, details);
            status = StateNodeStatus.RUNNING;
            switch (child.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    Init(child);
                    break;
            }
        }
    }

    public class BNodeParallel<T> : AStateNode<T> {
        private readonly AStateNode<T>[] children;

        public BNodeParallel(Transform transform, params AStateNode<T>[] children) : base(transform) {
            this.children = children;
        }

        protected override void Process(T details) {
            if (status == StateNodeStatus.FAILURE) return;
            bool running = false;

            foreach (AStateNode<T> child in children) {
                Process(child, details);


                if (status == StateNodeStatus.FAILURE) {
                    status = StateNodeStatus.FAILURE;
                    break;
                } else if (status == StateNodeStatus.RUNNING) {
                    running = true;
                }
            }

            status = running ? StateNodeStatus.RUNNING : StateNodeStatus.SUCCESS;
        }
    }
}
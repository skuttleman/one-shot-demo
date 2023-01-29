using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails>[] nodes;
        private int curr;

        public BNodeAnd(Transform transform, params AStateNode<EnemyAIStateDetails>[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            StateNodeStatus x = Process(nodes[curr], details);
            switch (x) {
                case StateNodeStatus.FAILURE: return StateNodeStatus.FAILURE;
                case StateNodeStatus.SUCCESS: curr++; break;
            }

            if (curr >= nodes.Length) return StateNodeStatus.SUCCESS;
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            curr = 0;
            foreach (AStateNode<EnemyAIStateDetails> node in nodes) node.Init();
        }
    }

    public class BNodeOr : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails>[] nodes;
        private int curr;

        public BNodeOr(Transform transform, params AStateNode<EnemyAIStateDetails>[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            switch (Process(nodes[curr], details)) {
                case StateNodeStatus.SUCCESS: return StateNodeStatus.SUCCESS;
                case StateNodeStatus.FAILURE: curr++; break;
            }

            if (curr >= nodes.Length) return StateNodeStatus.FAILURE;
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            curr = 0;
            foreach (AStateNode<EnemyAIStateDetails> node in nodes) node.Init();
        }
    }

    public class BNodeWait : AStateNode<EnemyAIStateDetails> {
        private readonly float time;
        private float elapsed;

        public BNodeWait(Transform transform, float time) : base(transform) {
            this.time = time;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            if (elapsed >= time) {
                return StateNodeStatus.SUCCESS;
            }

            elapsed += Time.deltaTime;
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            elapsed = 0f;
        }
    }

    public class BNodeDoFor : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> child;
        private readonly float time;
        private float elapsed;
        private StateNodeStatus childStatus;

        public BNodeDoFor(Transform transform, AStateNode<EnemyAIStateDetails> node, float time) : base(transform) {
            child = node;
            this.time = time;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            if (elapsed >= time) {
                return childStatus == StateNodeStatus.RUNNING ? StateNodeStatus.SUCCESS : childStatus;
            }

            elapsed += Time.deltaTime;
            childStatus = Process(child, details);

            return childStatus;
        }

        public override void Init() {
            elapsed = 0f;
            childStatus = StateNodeStatus.RUNNING;
            child.Init();
        }
    }

    public class BNodeRepeat : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> child;

        public BNodeRepeat(Transform transform, AStateNode<EnemyAIStateDetails> node) : base(transform) {
            child = node;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            switch (Process(child, details)) {
                case StateNodeStatus.FAILURE:
                    return StateNodeStatus.FAILURE;
                case StateNodeStatus.SUCCESS:
                    child.Init();
                    break;
            }

            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            child.Init();
        }
    }

    public class BNodeParallel : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails>[] children;
        private bool failed;

        public BNodeParallel(Transform transform, params AStateNode<EnemyAIStateDetails>[] children) : base(transform) {
            this.children = children;
            failed = false;
        }

        protected override StateNodeStatus Process(EnemyAIStateDetails details) {
            if (failed) return StateNodeStatus.FAILURE;
            bool running = false;

            foreach (AStateNode<EnemyAIStateDetails> child in children) {
                StateNodeStatus status = Process(child, details);

                if (status == StateNodeStatus.FAILURE) {
                    failed = true;
                    return StateNodeStatus.FAILURE;
                } else if (status == StateNodeStatus.RUNNING) {
                    running = true;
                }
            }

            return running ? StateNodeStatus.RUNNING : StateNodeStatus.SUCCESS;
        }

        public override void Init() {
            foreach (AStateNode<EnemyAIStateDetails> child in children) child.Init();
        }
    }
}
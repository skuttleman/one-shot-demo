using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails>[] nodes;
        private int curr;

        public BNodeAnd(Transform transform, params AStateNode<EnemyAIStateDetails>[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override void Process(EnemyAIStateDetails details) {
            AStateNode<EnemyAIStateDetails> node = nodes[curr];
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

        public override void Init() {
            status = StateNodeStatus.RUNNING;
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

        protected override void Process(EnemyAIStateDetails details) {
            AStateNode<EnemyAIStateDetails> node = nodes[curr];
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

        public override void Init() {
            status = StateNodeStatus.RUNNING;
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

        protected override void Process(EnemyAIStateDetails details) {
            status = StateNodeStatus.RUNNING;

            if (elapsed >= time) {
                status = StateNodeStatus.SUCCESS;
            }

            elapsed += Time.deltaTime;
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            elapsed = 0f;
        }
    }

    public class BNodeDoFor : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> child;
        private readonly float time;
        private float elapsed;

        public BNodeDoFor(Transform transform, AStateNode<EnemyAIStateDetails> node, float time) : base(transform) {
            child = node;
            this.time = time;
        }

        protected override void Process(EnemyAIStateDetails details) {
            if (elapsed >= time) {
                return;
            }

            elapsed += Time.deltaTime;
            Process(child, details);
            status = child.status;
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            elapsed = 0f;
            child.Init();
        }
    }

    public class BNodeRepeat : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails> child;

        public BNodeRepeat(Transform transform, AStateNode<EnemyAIStateDetails> node) : base(transform) {
            child = node;
        }

        protected override void Process(EnemyAIStateDetails details) {
            Process(child, details);
            status = StateNodeStatus.RUNNING;
            switch (child.status) {
                case StateNodeStatus.FAILURE:
                    status = StateNodeStatus.FAILURE;
                    break;
                case StateNodeStatus.SUCCESS:
                    child.Init();
                    break;
            }
        }

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            child.Init();
        }
    }

    public class BNodeParallel : AStateNode<EnemyAIStateDetails> {
        private readonly AStateNode<EnemyAIStateDetails>[] children;

        public BNodeParallel(Transform transform, params AStateNode<EnemyAIStateDetails>[] children) : base(transform) {
            this.children = children;
        }

        protected override void Process(EnemyAIStateDetails details) {
            if (status == StateNodeStatus.FAILURE) return;
            bool running = false;

            foreach (AStateNode<EnemyAIStateDetails> child in children) {
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

        public override void Init() {
            status = StateNodeStatus.RUNNING;
            foreach (AStateNode<EnemyAIStateDetails> child in children) child.Init();
        }
    }
}
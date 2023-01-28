using OSCore.System;
using UnityEngine;

namespace OSBE.Controllers.Enemy.Behaviors.Composites {
    public class BNodeAnd : AStateNode {
        private readonly AStateNode[] nodes;
        private int curr;

        public BNodeAnd(Transform transform, params AStateNode[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override StateNodeStatus ProcessImpl() {
            StateNodeStatus x = nodes[curr].Process();
            switch (x) {
                case StateNodeStatus.FAILURE: return StateNodeStatus.FAILURE;
                case StateNodeStatus.SUCCESS: curr++; break;
            }

            if (curr >= nodes.Length) return StateNodeStatus.SUCCESS;
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            curr = 0;
            foreach (AStateNode node in nodes) node.Init();
        }
    }

    public class BNodeOr : AStateNode {
        private readonly AStateNode[] nodes;
        private int curr;

        public BNodeOr(Transform transform, params AStateNode[] nodes) : base(transform) {
            this.nodes = nodes;
        }

        protected override StateNodeStatus ProcessImpl() {
            switch (nodes[curr].Process()) {
                case StateNodeStatus.SUCCESS: return StateNodeStatus.SUCCESS;
                case StateNodeStatus.FAILURE: curr++; break;
            }

            if (curr >= nodes.Length) return StateNodeStatus.FAILURE;
            return StateNodeStatus.RUNNING;
        }

        public override void Init() {
            curr = 0;
            foreach (AStateNode node in nodes) node.Init();
        }
    }

    public class BNodeWait : AStateNode {
        private readonly float time;
        private float elapsed;

        public BNodeWait(Transform transform, float time) : base(transform) {
            this.time = time;
        }

        protected override StateNodeStatus ProcessImpl() {
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

    public class BNodeDoFor : AStateNode {
        private readonly AStateNode child;
        private readonly float time;
        private float elapsed;

        public BNodeDoFor(Transform transform, AStateNode node, float time) : base(transform) {
            child = node;
            this.time = time;
        }

        protected override StateNodeStatus ProcessImpl() {
            if (elapsed >= time) {
                return StateNodeStatus.SUCCESS;
            }

            elapsed += Time.deltaTime;
            return child.Process();
        }

        public override void Init() {
            elapsed = 0f;
            child.Init();
        }
    }

    public class BNodeRepeat : AStateNode {
        private readonly AStateNode child;

        public BNodeRepeat(Transform transform, AStateNode node) : base(transform) {
            child = node;
        }

        protected override StateNodeStatus ProcessImpl() {
            switch (child.Process()) {
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
}
using UnityEngine;

namespace OSCore.System {
    public enum StateNodeStatus {
        INIT, RUNNING, SUCCESS, FAILURE
    }

    public abstract class ABehaviorNode<T> {
        public readonly Transform transform;
        public StateNodeStatus status { get; protected set; } = StateNodeStatus.INIT;
        public bool isFinished =>
            status == StateNodeStatus.SUCCESS
                || status == StateNodeStatus.FAILURE;

        public ABehaviorNode(Transform transform) {
            this.transform = transform;
        }

        protected virtual void Start(T details) {
            Continue(details);
        }

        protected abstract void Continue(T details);

        protected virtual void Stop() { }

        public static void Process(ABehaviorNode<T> node, T details) {
            if (node.status == StateNodeStatus.INIT) node.Start(details);
            else if (!node.isFinished) node.Continue(details);

            if (node.status == StateNodeStatus.INIT) node.status = StateNodeStatus.RUNNING;

            switch (node.status) {
                //case StateNodeStatus.RUNNING:
                //    Debug.Log(node.GetType().ToString() + " -> " + node.status);
                //    break;
                //case StateNodeStatus.FAILURE:
                //    Debug.Log(node.GetType().ToString() + " -> " + node.status);
                //    break;
                //case StateNodeStatus.SUCCESS:
                //    Debug.Log(node.GetType().ToString() + " -> " + node.status);
                //    break;
                default:
                    break;
            }
        }

        public static void ReInit(ABehaviorNode<T> node) {
            node.Stop();
            node.status = StateNodeStatus.INIT;
        }
    }
}

using UnityEngine;

namespace OSCore.System {
    public enum StateNodeStatus {
        RUNNING, SUCCESS, FAILURE
    }

    public abstract class AStateNode<T> {
        public readonly Transform transform;
        public StateNodeStatus status { get; protected set; } = StateNodeStatus.RUNNING;

        public AStateNode(Transform transform) {
            this.transform = transform;
        }

        protected abstract void Process(T details);

        public abstract void Init();

        public static void Process(AStateNode<T> node, T details) {
            if (node == null) return;
            if (node.status != StateNodeStatus.RUNNING) return;

            node.Process(details);
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
    }
}

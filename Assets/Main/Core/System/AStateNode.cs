using UnityEngine;

namespace OSCore.System {
    public enum StateNodeStatus {
        RUNNING, SUCCESS, FAILURE
    }

    public abstract class AStateNode<T> {
        public readonly Transform transform;

        public AStateNode(Transform transform) {
            this.transform = transform;
        }

        protected abstract StateNodeStatus Process(T details);

        public abstract void Init();

        public static StateNodeStatus Process(AStateNode<T> node, T details) {
            if (node == null) return StateNodeStatus.FAILURE;

            StateNodeStatus status = node.Process(details);
            switch (status) {
                //case StateNodeStatus.RUNNING:
                //    Debug.Log(node.GetType().ToString() + " -> " + status);
                //    break;
                //case StateNodeStatus.FAILURE:
                //    Debug.Log(node.GetType().ToString() + " -> " + status);
                //    break;
                //case StateNodeStatus.SUCCESS:
                //    Debug.Log(node.GetType().ToString() + " -> " + status);
                //    break;
            }

            return status;
        }
    }
}

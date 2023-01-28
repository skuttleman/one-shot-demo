using UnityEngine;

namespace OSCore.System {
    public enum StateNodeStatus {
        RUNNING, SUCCESS, FAILURE
    }

    public abstract class AStateNode {
        public readonly Transform transform;

        public AStateNode(Transform transform) {
            this.transform = transform;
        }

        public StateNodeStatus Process() {
            StateNodeStatus status = ProcessImpl();
            //Debug.Log("Processing " + GetType() + " -> " + status);
            return status;
        }

        protected abstract StateNodeStatus ProcessImpl();

        public abstract void Init();
    }
}

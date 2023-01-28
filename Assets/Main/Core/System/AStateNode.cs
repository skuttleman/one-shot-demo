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

        public abstract StateNodeStatus Process(T details);

        public abstract void Init();
    }
}

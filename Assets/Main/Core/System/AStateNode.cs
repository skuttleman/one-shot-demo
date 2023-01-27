using UnityEngine;
using System.Collections.Generic;

namespace OSCore.System {
    public enum StateMachineStatus {
        RUNNING, SUCCESS, FAILURE
    }

    public abstract class AStateNode : MonoBehaviour {
        public virtual StateMachineStatus Process() =>
            StateMachineStatus.FAILURE;
    }
}

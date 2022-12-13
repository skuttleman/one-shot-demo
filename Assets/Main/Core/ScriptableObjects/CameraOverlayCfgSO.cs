using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam/overlay")]
    public class CameraOverlayCfgSO : ScriptableObject {
        [field: Range(0f, 1f)]
        [field: SerializeField]
        public float maxOverlayAlpha { get; private set; }
    }
}

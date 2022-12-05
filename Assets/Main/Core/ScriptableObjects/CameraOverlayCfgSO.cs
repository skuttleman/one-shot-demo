using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam/overlay")]
    public class CameraOverlayCfgSO : ScriptableObject {
        [Range(0f, 1f)] public float maxOverlayAlpha;
    }
}

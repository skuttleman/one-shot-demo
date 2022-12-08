using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/fov")]
    public class PlayerFOVCfgSO : ScriptableObject {
        [field: SerializeField] public int RAY_COUNT { get; private set; }

        [field: SerializeField] public LayerMask layerMask { get; private set; }
        [field: SerializeField] public float viewDistance { get; private set; }
        [field: SerializeField] public float lookZ { get; private set; }
        [field: SerializeField] public float angleDither { get; private set; }
    }
}

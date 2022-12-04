using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/fov")]
    public class PlayerFOVCfgSO : ScriptableObject {
        public readonly int RAY_COUNT = 50;

        public LayerMask layerMask;
        public float fov;
        public float viewDistance;
        public float startingAngle;
    }
}

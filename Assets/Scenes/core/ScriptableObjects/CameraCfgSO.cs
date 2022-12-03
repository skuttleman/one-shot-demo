using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam")]
    public class CameraCfgSO : ScriptableObject {
        public float orbitSpeed;
        public float moveOffset;
        public float scopeOffset;
        public float aimOffset;
        public float maxLookAhead;

        [Header("Attacking")]
        public float punchOffset;
        public float fireOffset;
    }
}

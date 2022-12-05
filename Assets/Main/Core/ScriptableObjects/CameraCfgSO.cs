using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam/general")]
    public class CameraCfgSO : ScriptableObject {
        public float orbitSpeed;
        public float moveOffset;
        public float aimOffset;
        public float maxLookAhead;

        [Header("Attacking")]
        public float punchOffset;
        public float fireOffset;
    }
}

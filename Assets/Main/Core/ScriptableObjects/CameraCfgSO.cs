using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam/general")]
    public class CameraCfgSO : ScriptableObject {
        [field: SerializeField] public float orbitSpeed { get; private set; }
        [field: SerializeField] public float moveOffset { get; private set; }
        [field: SerializeField] public float aimOffset { get; private set; }
        [field: SerializeField] public float maxLookAhead { get; private set; }

        [field: Header("Attacking")]
        [field: SerializeField] public float punchOffset { get; private set; }
        [field: SerializeField] public float fireOffset { get; private set; }
    }
}

using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/basic")]
    public class EnemyCfgSO : ScriptableObject {
        [field: SerializeField] public float moveSpeed { get; private set; }
        [field: SerializeField] public float rotationSpeed { get; private set; }

        [field: Header("FOV")]
        [field: SerializeField] public float fovAngle { get; private set; }
        [field: SerializeField] public float fovDistance { get; private set; }
    }
}

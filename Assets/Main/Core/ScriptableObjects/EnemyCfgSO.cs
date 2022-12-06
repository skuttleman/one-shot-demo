using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/enemy/basic")]
    public class EnemyCfgSO : ScriptableObject {
        public float moveSpoeed;
        public float rotationSpeed;
    }
}

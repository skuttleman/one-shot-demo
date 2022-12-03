using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "geometry/building")]
    public class BuildingParallaxCfgSO : ScriptableObject {
        public float customFactor;
        public Vector2 center;
        public Vector2 maxOffset;
    }
}

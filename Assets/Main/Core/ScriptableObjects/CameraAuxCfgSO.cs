using System;
using UnityEngine;
namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/cam/aux")]
    public class CameraAuxCfgSO : ScriptableObject {
        [field: Header("Shake Config")]
        [field: SerializeField] public ShakeConfig fire { get; private set; }
        [field: SerializeField] public ShakeConfig punch { get; private set; }
        [field: SerializeField] public ShakeConfig fallLand { get; private set; }
        [field: SerializeField] public ShakeConfig diveLand { get; private set; }

        [Serializable]
        public struct ShakeConfig {
            [field: SerializeField] public float amp { get; private set; }
            [field: SerializeField] public float freq { get; private set; }
            [field: SerializeField] public float duration { get; private set; }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore {
    [CreateAssetMenu(menuName = "cfg/cam")]
    public class CameraCfgSO : ScriptableObject {
        public float orbitSpeed;
        public float moveOffset;
        public float scopeOffset;
        public float aimOffset;
        public float maxLookAhead;
    }
}

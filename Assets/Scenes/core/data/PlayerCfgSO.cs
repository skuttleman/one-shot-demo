using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore {
    [CreateAssetMenu(menuName = "cfg/player")]
    public class PlayerCfgSO : ScriptableObject {
        [Header("Input")]
        public float stanceChangeHeldThreshold;
        public float punchingSpeed;
        public float firingSpeed;

        [Header("Movement")]
        public MoveConfig standing;
        public MoveConfig crouching;
        public MoveConfig crawling;
        public float aimFactor;
        public float scopeFactor;

        [Serializable]
        public struct MoveConfig {
            public float moveSpeed;
            public float rotationSpeed;
            public float animFactor;
        }
    }
}

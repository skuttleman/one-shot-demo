using System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player")]
    public class PlayerCfgSO : ScriptableObject {
        [Header("Input")]
        public float stanceChangeHeldThreshold;
        public float mouseLookReset;

        [Header("Stance")]
        public MoveConfig sprinting;
        public MoveConfig standing;
        public MoveConfig crouching;
        public MoveConfig crawling;

        [Header("Scoping")]
        public float scopingSpeed;
        public float scopeFactor;

        [Header("Aiming")]
        public float aimFactor;
        public float aimingSpeed;

        [Header("Attacking")]
        public float punchingSpeed;
        public float firingSpeed;

        [Serializable]
        public struct MoveConfig {
            public float moveSpeed;
            public float rotationSpeed;
            public float animFactor;
            public float maxVelocity;
            public float maxVelocitydamper;
        }
    }
}

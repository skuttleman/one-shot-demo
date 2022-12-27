using System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    [CreateAssetMenu(menuName = "cfg/player/general")]
    public class PlayerCfgSO : ScriptableObject {
        [field: Header("Input")]
        [field: SerializeField] public float mouseLookReset { get; private set; }
        [field: Range(0.1f, 1f)]
        [field: SerializeField] public float groundedDist { get; private set; }

        [field: Header("Stance")]
        [field: SerializeField] public MoveConfig sprinting { get; private set; }
        [field: SerializeField] public MoveConfig crouching { get; private set; }
        [field: SerializeField] public MoveConfig crawling { get; private set; }

        [field: Header("Movement")]
        [field: SerializeField] public float scopeFactor { get; private set; }
        [field: SerializeField] public float aimFactor { get; private set; }
        [field: SerializeField] public float diveForce { get; private set; }

        [field: Header("LedgeHang")]
        [field: SerializeField] public float hangMoveAmount { get; private set; }

        [Serializable]
        public struct MoveConfig {
            [field: SerializeField] public float moveSpeed { get; private set; }
            [field: SerializeField] public float rotationSpeed { get; private set; }
            [field: SerializeField] public float animFactor { get; private set; }
            [field: SerializeField] public float maxVelocity { get; private set; }
            [field: SerializeField] public float maxVelocitydamper { get; private set; }
            [field: Range(0f, 1f)] [field: SerializeField] public float lookSpeedInhibiter { get; private set; }
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "cfg/player")]
public class PlayerCfgSO : ScriptableObject
{
    [Header("Movement")]
    public MoveConfig standing;
    public MoveConfig crouching;
    public MoveConfig crawling;

    [Header("Misc")]
    public float aimFactor;
    public float scopeFactor;
    public float stanceChangeHeldThreshold;

    [Serializable]
    public struct MoveConfig {
        public float moveSpeed;
        public float rotationSpeed;
        public float animFactor;
    }
}

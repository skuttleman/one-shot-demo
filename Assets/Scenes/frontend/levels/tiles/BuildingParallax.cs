using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "geometry/building")]
public class BuildingParallax : ScriptableObject {
    public float customFactor;
    public Vector2 center;
    public Vector2 maxOffset;
}

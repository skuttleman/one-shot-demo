using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.Utils {
    public static class Maths {
        public static bool NonZero(float value) {
            return Mathf.Abs(value) > Mathf.Epsilon;
        }

        public static float Distance(Vector3 pos1, Vector3 pos2) {
            float deltaX = pos1.x - pos2.x;
            float deltaY = pos1.y - pos2.y;
            float deltaZ = pos1.z - pos2.z;
            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        }
    }
}

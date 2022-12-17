using UnityEngine;

namespace OSCore.Utils {
    public static class Maths {
        public static bool NonZero(float value) =>
            NonZero(value, Mathf.Epsilon);

        public static bool NonZero(float value, float tolerance) =>
            Mathf.Abs(value) > tolerance;

        public static float RoundTo(this float value, float offset) =>
            Mathf.Round(value * offset) / offset;
    }
}

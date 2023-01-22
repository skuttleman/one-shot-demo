using UnityEngine;

namespace OSCore.Utils {
    public static class Maths {
        public static bool NonZero(float value) =>
            NonZero(value, Mathf.Epsilon);

        public static bool NonZero(float value, float tolerance) =>
            Mathf.Abs(value) > tolerance;

        public static float RoundTo(this float value, float offset) =>
            Mathf.Round(value * offset) / offset;

        public static float AngleDiff(float angle1, float angle2) {
            angle1 = (angle1 + 360f) % 360f;
            angle2 = (angle2 + 360f) % 360f;
            if (angle1 < 0 || angle1 >= 360f || angle2 < 0f || angle2 >= 360f) {
                return AngleDiff(angle1, angle2);
            }

            float diff = Mathf.Abs(angle1 - angle2);
            return diff >= 180f ? Mathf.Abs(diff - 360f) : diff;
        }
    }
}

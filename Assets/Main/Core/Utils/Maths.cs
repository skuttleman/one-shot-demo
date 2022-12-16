using UnityEngine;

namespace OSCore.Utils {
    public static class Maths {
        public static bool NonZero(float value) =>
            Mathf.Abs(value) > Mathf.Epsilon;

        public static float RoundTo(this float value, float offset) =>
            Mathf.Round(value * offset) / offset;
    }
}

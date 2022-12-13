using UnityEngine;

namespace OSCore.Utils {
    public static class Maths {
        public static bool NonZero(float value) =>
            Mathf.Abs(value) > Mathf.Epsilon;
    }
}

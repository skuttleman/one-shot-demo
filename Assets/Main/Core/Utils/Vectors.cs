using UnityEngine;

namespace OSCore.Utils {
    public static class Vectors {
        public static Vector3 UP = new(0f, 0f, -1f);
        public static Vector3 DOWN = new(0f, 0f, 1f);
        public static Vector3 LEFT = new(-1f, 0f, 0f);
        public static Vector3 RIGHT = new(1f, 0f, 0f);
        public static Vector3 FORWARD = new(0f, 1f, 0f);
        public static Vector3 BACK = new(0f, -1f, 0f);

        public static Vector3 Sign(this Vector3 vector) =>
            new(Mathf.Sign(vector.x), Mathf.Sign(vector.y), Mathf.Sign(vector.z));

        public static Vector2 Sign(this Vector2 vector) =>
            new(Mathf.Sign(vector.x), Mathf.Sign(vector.y));

        public static Vector3 Upgrade(this Vector2 vector) =>
            new(vector.x, vector.y, 0f);

        public static Vector3 Upgrade(this Vector2 vector, float z) =>
            new(vector.x, vector.y, z);

        public static Vector2 Downgrade(this Vector3 vector) =>
            new(vector.x, vector.y);

        public static Vector3 Clamp(this Vector3 vector, Vector3 bound1, Vector3 bound2) =>
            new(Mathf.Clamp(vector.x, Mathf.Min(bound1.x, bound2.x), Mathf.Max(bound1.x, bound2.x)),
                Mathf.Clamp(vector.y, Mathf.Min(bound1.y, bound2.y), Mathf.Max(bound1.y, bound2.y)),
                Mathf.Clamp(vector.z, Mathf.Min(bound1.z, bound2.z), Mathf.Max(bound1.z, bound2.z)));

        public static Vector3 Range(float minValue, float maxValue) =>
            new(Random.Range(minValue, maxValue),
                Random.Range(minValue, maxValue),
                Random.Range(minValue, maxValue));

        public static bool IsInside(this Vector3 pos, Vector3 lowerLeft, Vector3 upperRight) =>
            pos.x > lowerLeft.x
                && pos.x <= upperRight.x
                && pos.y >= lowerLeft.y
                && pos.y <= upperRight.y
                && pos.z >= lowerLeft.z
                && pos.z <= upperRight.z;

        public static bool IsInside(this Vector2 pos, Vector2 lowerLeft, Vector2 upperRight) =>
            IsInside(pos.Upgrade(), lowerLeft.Upgrade(), upperRight.Upgrade());

        public static float AngleTo(Vector2 direction) {
            Vector2 dir = direction.normalized;
            return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        }

        public static float AngleTo(Vector2 origin, Vector2 position) =>
            AngleTo(origin - position);

        public static bool NonZero(Vector2 vector) => NonZero(Upgrade(vector));

        public static bool NonZero(Vector3 vector) {
            return Maths.NonZero(vector.x)
                || Maths.NonZero(vector.y)
                || Maths.NonZero(vector.z);
        }

        public static Vector3 ToVector3(float angle) {
            float angleRad = angle * (Mathf.PI / 180f);
            return new(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static Vector2 ToVector2(float angle) =>
            Downgrade(ToVector3(angle));
    }
}

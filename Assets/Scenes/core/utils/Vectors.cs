using UnityEngine;

namespace OSCore.Utils {
    public static class Vectors {
        public static Vector3 Upgrade(this Vector2 vector) =>
            new(vector.x, vector.y, 0f);
        public static Vector3 Upgrade(this Vector2 vector, float z) =>
            new(vector.x, vector.y, z);
        public static Vector2 Downgrade(this Vector3 vector) =>
            new(vector.x, vector.y);
        public static Vector3 Clamp(Vector3 vector, Vector3 lower, Vector3 upper) =>
            new Vector3(
                Mathf.Clamp(vector.x, lower.x, upper.x),
                Mathf.Clamp(vector.y, lower.y, upper.y),
                Mathf.Clamp(vector.z, lower.z, upper.z));
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
            return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        }

        public static Vector2 ToVector2(float angle) =>
            Downgrade(ToVector3(angle));
    }
}

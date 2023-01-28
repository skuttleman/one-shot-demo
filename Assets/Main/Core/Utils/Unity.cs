using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSCore.Utils {
    public static class Transforms {
        public static ISet<Transform> FindInChildren(
            Transform parent, Predicate<Transform> pred) =>
            FindInChildren(new HashSet<Transform>(), parent, pred);

        public static Transform Body(Transform transform) {
            Transform entity = transform;
            while (entity is not null && entity.name != "body") entity = entity.parent;
            return entity;
        }

        public static ISet<Transform> FindInActiveChildren(
            Transform parent, Predicate<Transform> pred) =>
            FindInChildren(
                new HashSet<Transform>(),
                parent,
                child => child.gameObject.activeInHierarchy && pred(child));

        private static ISet<Transform> FindInChildren(
            ISet<Transform> results, Transform parent, Predicate<Transform> pred) {
            foreach (Transform child in parent) {
                if (pred(child)) results.Add(child);
                FindInChildren(results, child, pred);
            }
            return results;
        }

        public static float VisibilityFrom(Vector3 position, CapsuleCollider collider) {
            float height = (collider.height / 2) - 0.1f;
            Vector3 offset = new(
                collider.direction == 0 ? height : 0,
                collider.direction == 1 ? height : 0,
                collider.direction == 2 ? height : 0);
            Vector3 center = collider.center + collider.transform.position;
            Vector3 head = center + offset;
            Vector3 feet = center - offset;

            float result = 0f;
            if (!IsHit(position, center)) result += 0.65f;
            if (!IsHit(position, head)) result += 0.25f;
            if (!IsHit(position, feet)) result += 0.1f;

            return result;
        }

        private static bool IsHit(Vector3 origin, Vector3 position) =>
            Physics.Raycast(
                origin,
                position - origin,
                Vector3.Distance(origin, position),
                LayerMask.GetMask("Opaque", "InsideOpaque"));
    }

    public static class Monos {
        public static IEnumerator<YieldInstruction> After(float seconds, Action cb) {
            yield return new WaitForSeconds(seconds);
            cb();
            yield break;
        }
    }

    public static class LogUtils {
        public static T FindNull<T>(string id, T item)
            where T : class {

            if (item == null) {
                Debug.Log(id + " is Null!");
            }
            return item;
        }
    }
}

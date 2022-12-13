using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSCore.Utils {
    public static class Transforms {
        public static ISet<Transform> FindInChildren(
            Transform parent, Predicate<Transform> pred) =>
            FindInChildren(new HashSet<Transform>(), parent, pred);

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
    }
}

using System.Collections.Generic;

namespace OSCore.Utils {
    public static class Sets {
        public static bool ContainsAny<T>(ISet<T> set, params T[] coll) {
            foreach (T item in coll)
                if (set.Contains(item)) return true;
            return false;
        }

        public static ISet<T> Of<T>(params T[] items) =>
            Of(items);

        public static ISet<T> Of<T>(IEnumerable<T> coll) {
            ISet<T> set = new HashSet<T>();
            foreach (T item in coll) set.Add(item);
            return set;
        }
    }
}

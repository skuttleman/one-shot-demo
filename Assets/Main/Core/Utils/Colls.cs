using System.Collections.Generic;
using System;

namespace OSCore.Utils {
    public static class Colls {
        public static L Add<L, T>(L coll, T item) where L : ICollection<T> {
            coll.Add(item);
            return coll;
        }

        public static ISet<T> Remove<T>(ISet<T> set, T item) {
            if (set != null && set.Contains(item)) set.Remove(item);
            return set;
        }

        public static IDictionary<K, V> Remove<K, V>(this IDictionary<K, V> dict, K key) {
            if (dict != null && dict.ContainsKey(key)) dict.Remove(key);
            return dict;
        }

        public static IDictionary<K, V> Update<K, V>(
            this IDictionary<K, V> dict, K key, Func<V, V> fn, Func<V> gen) {

            dict[key] = fn(dict.ContainsKey(key) ? dict[key] : gen());
            return dict;
        }

        public static IDictionary<K, V> Update<K, V>(
            IDictionary<K, V> dict, K key, Func<V, V> fn) =>
            Update(dict, key, fn, () => default);

        public static Func<T, U> Fn<T, U>(this IDictionary<T, U> dict) =>
            item => dict.ContainsKey(item) ? dict[item] : default;

        public static Predicate<T> Pred<T, U>(this IDictionary<T, U> dict) =>
            item => dict.ContainsKey(item);

        public static Predicate<T> Pred<T>(this ISet<T> set) =>
            item => set.Contains(item);

        public static U Get<T, U>(this IDictionary<T, U> dict, T key) =>
            Get(dict, key, default);

        public static U Get<T, U>(this IDictionary<T, U> dict, T key, U otherwise) {
            if (dict.ContainsKey(key)) return dict[key];
            return otherwise;
        }

        public static IEnumerable<long> Range(long end) => Range(0L, end);

        public static IEnumerable<long> Range(long start, long end) => Range(start, end, 1L);

        public static IEnumerable<long> Range(long start, long end, long jump) {
            for (long i = start; i < end; i += jump)
                yield return i;
        }
    }
}

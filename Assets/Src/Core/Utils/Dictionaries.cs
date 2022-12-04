using System.Collections.Generic;
using System;

namespace OSCore.Utils {
    public static class Dictionaries {
        public static IDictionary<K, V> Of<K, V>(params (K, V)[] items) =>
            Of(items);

        public static IDictionary<K, V> Of<K, V>(IEnumerable<(K, V)> coll) {
            IDictionary<K, V> result = new Dictionary<K, V>();
            coll.ForEach(tpl => result.Add(tpl.Item1, tpl.Item2));
            return result;
        }

        public static IDictionary<K, V> Of<K, V>(IEnumerable<KeyValuePair<K, V>> entries) {
            IDictionary<K, V> result = new Dictionary<K, V>();
            entries.ForEach(result.Add);
            return result;
        }
    }
}

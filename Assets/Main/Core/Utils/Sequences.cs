using System.Collections.Generic;
using System;

namespace OSCore.Utils {
    public static class Sequences {
        public static void DoAll<T>(this IEnumerable<T> coll) {
            ForEach(coll, _ => { });
        }

        public static void ForEach<T>(this IEnumerable<T> coll, Action<T> action) {
            foreach (T item in coll ?? Empty<T>()) action(item);
        }

        public static U Reduce<T, U>(this IEnumerable<T> coll, Func<U, T, U> reducer, U init) {
            U result = init;
            foreach (T item in coll ?? Empty<T>()) result = reducer(result, item);
            return result;
        }

        public static U ReduceUntil<T, U>(this IEnumerable<T> coll, Func<U, T, Reduction<U>> reducer, U init) {
            U result = init;
            foreach (T item in coll ?? Empty<T>()) {
                Reduction<U> next = reducer(result, item);
                result = next.Get();
                if (next.IsReduced()) return result;
            }
            return result;
        }

        public static IEnumerable<T> Empty<T>() {
            yield break;
        }

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> coll, params IEnumerable<T>[] seqs) {
            foreach (IEnumerable<T> seq in Cons(coll, seqs))
                foreach (T item in seq ?? Empty<T>())
                    yield return item;
        }

        public static IEnumerable<T> Cons<T>(T head, IEnumerable<T> tail) {
            yield return head;
            foreach (T item in tail ?? Empty<T>()) yield return item;
        }

        public static IEnumerable<T> Cons<T>(this IEnumerable<T> tail, T head) =>
            ConsIf(tail, true, head);

        public static IEnumerable<T> ConsIf<T>(this IEnumerable<T> tail, bool condition, T head) {
            if (condition) yield return head;
            foreach (T item in tail ?? Empty<T>()) yield return item;
        }

        public static IEnumerable<T> Cycle<T>(this IEnumerable<T> coll) {
            while (true)
                foreach (T item in coll ?? Empty<T>())
                    yield return item;
        }

        public static T First<T>(this IEnumerable<T> coll) {
            IEnumerator<T> iter = (coll ?? Empty<T>()).GetEnumerator();
            iter.MoveNext();
            return iter.Current;
        }

        public static O First<I, O>(this IEnumerable<I> coll, IXForm<I, O> xform) {
            RF<O, I> rf = xform.XForm<O>((acc, item) =>
                acc.IsReduced() ? acc : Reduction<O>.Reduced(item));
            Reduction<O> result = Reduction<O>.UnReduced(default);
            foreach (I item in coll) {
                result = rf(result, item);
                if (result.IsReduced()) return result.Get();
            }
            return default;
        }

        public static bool IsEmpty<T>(this IEnumerable<T> coll) =>
            !coll.GetEnumerator().MoveNext();

        public static A Transduce<A, I, O>(
            this IEnumerable<I> coll,
            IXForm<I, O> xform,
            Func<A, O, A> reducer,
            A init) {
            RF<A, I> reduceFn = xform.XForm<A>((red, item) => red.IsReduced() ?
                red :
                Reduction<A>.UnReduced(reducer(red.Get(), item)));
            return coll.ReduceUntil(
                (acc, item) => reduceFn(Reduction<A>.UnReduced(acc), item),
                init);
        }

        public static IList<O> Transduce<I, O>(this IEnumerable<I> coll, IXForm<I, O> xform) =>
            Transduce<IList<O>, I, O>(coll, xform, Colls.Add, new List<O>());

        public static void Transduce<I, O>(this IEnumerable<I> coll, IXForm<I, O> xform, Action<O> consumer) {
            Transduce<dynamic, I, O>(
                coll,
                xform,
                (_, item) => { consumer(item); return default; },
                default);
        }
    }

    public class Reduction<T> {
        private readonly T item;
        private readonly bool isReduced;

        private Reduction(T item, bool isReduced) {
            this.item = item;
            this.isReduced = isReduced;
        }

        public static Reduction<T> Reduced(T item) => new(item, true);
        public static Reduction<T> UnReduced(T item) => new(item, false);
        public bool IsReduced() => isReduced;
        public T Get() => item;
    }
}

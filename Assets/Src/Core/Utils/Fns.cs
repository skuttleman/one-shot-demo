using System.Collections.Generic;
using System;

namespace OSCore.Utils {
    public delegate Reduction<A> RF<A, I>(Reduction<A> acc, I item);

    public static class Fns {
        public static Predicate<T> Compliment<T>(Predicate<T> pred) =>
            t => !pred(t);
        public static Func<T, V> Comp<T, U, V>(Func<U, V> fn1, Func<T, U> fn2) =>
            t => fn1(fn2(t));
        public static Action<T> Comp<T, U>(Action<U> action, Func<T, U> fn) =>
            t => action(fn(t));
        public static T Identity<T>(T item) => item;

        public static IXForm<I, O> MapCat<I, O>(Func<I, IEnumerable<O>> fn) =>
            new MapCatXF<I, O>(fn);
        public static IXForm<I, O> Map<I, O>(Func<I, O> fn) => new MapXF<I, O>(fn);
        public static IXForm<I, I> Filter<I>(Predicate<I> pred) =>
            new FilterXF<I>(pred);
        public static IXForm<I, I> Remove<A, I>(Predicate<I> pred) =>
            new FilterXF<I>(Compliment(pred));
        public static IXForm<I, I> Take<I>(long n) => new TakeXF<I>(n);
        public static IXForm<I, I> Drop<I>(long n) => new DropXF<I>(n);
    }

    public interface IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf);
        public IXForm<I, R> Comp<R>(IXForm<O, R> xform)
            => new CompXF<I, O, R>(this, xform);
    }

    public record MapCatXF<I, O>(Func<I, IEnumerable<O>> mapFn) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) =>
            (acc, item) => mapFn(item).Reduce((a, i) => rf(a, i), acc);
    }

    public record MapXF<I, O>(Func<I, O> mapFn) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) =>
            (acc, item) => rf(acc, mapFn(item));
    }

    public record FilterXF<I>(Predicate<I> pred) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) =>
            (acc, item) => pred(item) ? acc : rf(acc, item);
    }

    public record TakeXF<I>(long n) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) {
            long items = n;
            return (acc, item) =>
                items-- > 0 ? rf(acc, item) : Reduction<A>.Reduced(acc.Get());
        }
    }

    public record DropXF<I>(long n) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) {
            long items = n;
            return (acc, item) =>
                items-- <= 0 ? rf(acc, item) : Reduction<A>.Reduced(acc.Get());
        }
    }

    public record CompXF<I, M, O>(IXForm<I, M> xf1, IXForm<M, O> xf2) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) => xf1.XForm(xf2.XForm(rf));
    }
}

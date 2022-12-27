using System.Collections.Generic;
using System;

namespace OSCore.Utils {
    public delegate Reduction<A> RF<A, I>(Reduction<A> acc, I item);

    public interface IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf);
    }

    public static class Fns {
        public static Predicate<T> Compliment<T>(Predicate<T> pred) =>
            t => !pred(t);

        public static T Identity<T>(T item) => item;

        public static IXForm<I, O> MapCat<I, O>(Func<I, IEnumerable<O>> fn) =>
            new MapCatXF<I, O>(fn);

        public static IXForm<I, O> Map<I, O>(Func<I, O> fn) =>
            new MapXF<I, O>(fn);

        public static IXForm<I, I> Filter<I>(Predicate<I> pred) =>
            new FilterXF<I>(pred);

        public static IXForm<I, I> Remove<A, I>(Predicate<I> pred) =>
            new FilterXF<I>(Compliment(pred));

        public static IXForm<I, I> Take<I>(long n) =>
            new TakeXF<I>(n);

        public static IXForm<I, I> Drop<I>(long n) =>
            new DropXF<I>(n);

        public static IXForm<I, R> Comp<I, O, R>(this IXForm<I, O> xform1, IXForm<O, R> xform2) =>
            new CompXF<I, O, R>(xform1, xform2);
    }

    public record MapXF<I, O>(Func<I, O> mapFn) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) =>
            (acc, item) => rf(acc, mapFn(item));
    }

    public record MapCatXF<I, O>(Func<I, IEnumerable<O>> mapFn) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) =>
            (acc, item) => mapFn(item).Reduce((a, i) => rf(a, i), acc);
    }

    public record FilterXF<I>(Predicate<I> pred) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) =>
            (acc, item) => pred(item) ? rf(acc, item) : acc;
    }

    public record TakeXF<I>(long n) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) {
            long items = n;
            return (acc, item) => items-- > 0 ? rf(acc, item) : acc;
        }
    }

    public record DropXF<I>(long n) : IXForm<I, I> {
        public RF<A, I> XForm<A>(RF<A, I> rf) {
            long items = n;
            return (acc, item) => items-- <= 0 ? rf(acc, item) : acc;
        }
    }

    public record CompXF<I, M, O>(IXForm<I, M> xf1, IXForm<M, O> xf2) : IXForm<I, O> {
        public RF<A, I> XForm<A>(RF<A, O> rf) => xf1.XForm(xf2.XForm(rf));
    }
}

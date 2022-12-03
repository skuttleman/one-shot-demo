using OSCore.System.Interfaces;
using OSCore.Utils;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace OSBE.Async.Core {
    public interface IPromise<T> {
        public IPromise<R> AndThen<R>(
            Func<T, IPromise<R>> onSuccess,
            Func<Exception, IPromise<R>> onError);
    }

    public static class Promises {
        public static IPromise<T> Resolve<T>(T val) =>
            new ResolvedPromise<T>(val);

        public static IPromise<T> Reject<T>(Exception ex) =>
            new RejectedPromise<T>(ex);

        public static IPromise<R> Then<T, R>(
            this IPromise<T> promise, Func<T, IPromise<R>> onSuccess) =>
            promise.AndThen(onSuccess, Reject<R>);

        public static IPromise<R> Then<T, R>(
            this IPromise<T> promise, Func<T, R> onSuccess) =>
                promise.AndThen(val => Resolve(onSuccess(val)), Reject<R>);

        public static IPromise<dynamic> Then<T>(
            this IPromise<T> promise, Action<T> onSuccess) =>
                promise.AndThen(
                    val => { onSuccess(val); return Resolve<dynamic>(default); },
                    Reject<dynamic>);

        public static IPromise<dynamic> Then<T>(
            this IPromise<T> promise, Action onSuccess) =>
                promise.AndThen(
                    val => { onSuccess(); return Resolve<dynamic>(default); },
                    Reject<dynamic>);
    }

    public class ResolvedPromise<T> : IPromise<T> {
        readonly T val;

        public ResolvedPromise(T val) =>
            this.val = val;

        IPromise<R> IPromise<T>.AndThen<R>(
            Func<T, IPromise<R>> onSuccess,
            Func<Exception, IPromise<R>> onError) =>
                onSuccess(val);
    }

    public class RejectedPromise<T> : IPromise<T> {
        readonly Exception ex;

        public RejectedPromise(Exception ex) =>
            this.ex = ex;

        IPromise<R> IPromise<T>.AndThen<R>(
            Func<T, IPromise<R>> onSuccess,
            Func<Exception, IPromise<R>> onError) =>
                onError(ex);
    }

    public class PromiseFactory : IGameSystemComponent {
        IDictionary<long, IGameSystemComponent> promises;
        long id = 0;

        public PromiseFactory() {
            promises = new Dictionary<long, IGameSystemComponent>();
        }

        public IPromise<T> Create<T>(Action<Action<T>, Action<Exception>> creator) {
            long thisId = ++id;
            SubroutinePromise<T> promise = new(this, thisId, creator);
            promises.Add(thisId, promise);

            return promise;
        }

        public IPromise<T> Await<T>(float seconds, T val) {
            long thisId = ++id;
            AwaitPromise<T> promise = new(this, thisId, seconds, val);
            promises.Add(thisId, promise);

            return promise;
        }

        public IPromise<dynamic> Await(float seconds) =>
            Await<dynamic>(seconds, default);

        public void OnDestroy() {
            promises.ForEach(promise => promise.Value.OnDestroy());
        }

        public void Update() {
            promises.ForEach(promise => promise.Value.Update());
        }

        public void Remove(long thisId) {
            promises = Dictionaries
                .Of(promises.Filter(prom => prom.Key != thisId));
        }

        class AwaitPromise<T> : IPromise<T>, IGameSystemComponent {
            readonly Queue<Action> actions;
            readonly PromiseFactory factory;
            readonly long id;
            readonly T val;
            float timeLeft;

            public AwaitPromise(PromiseFactory factory, long id, float timeLeft, T val) {
                actions = new Queue<Action>();
                this.factory = factory;
                this.id = id;
                this.timeLeft = timeLeft;
                this.val = val;
            }

            public IPromise<R> AndThen<R>(
                Func<T, IPromise<R>> onSuccess,
                Func<Exception, IPromise<R>> onError) {

                if (timeLeft <= 0f) return onSuccess(val);

                return factory.Create<R>((resolve, reject) => {
                    actions.Enqueue(() => {
                        try {
                            onSuccess(val).AndThen<R>(
                                r => { resolve(r); return default; },
                                ex => { reject(ex); return default; });
                        } catch (Exception ex) {
                            onError(ex).AndThen<R>(
                                r => { resolve(r); return default; },
                                ex => { reject(ex); return default; });
                        }
                    });
                });
            }

            public void Update() {
                if (timeLeft <= 0f) {
                    while (!actions.IsEmpty())
                        actions.Dequeue()();
                    factory.Remove(id);
                } else {
                    timeLeft -= Time.deltaTime;
                }
            }
        }

        class SubroutinePromise<T> : IPromise<T>, IGameSystemComponent {
            readonly Queue<Action> actions;
            readonly PromiseFactory factory;
            readonly long id;
            bool isCompleted = false;
            bool? wasSuccessful;
            T val;
            Exception ex;

            internal SubroutinePromise(
                PromiseFactory factory,
                long id,
                Action<Action<T>, Action<Exception>> creator) {
                actions = new Queue<Action>();
                this.factory = factory;
                this.id = id;

                creator(val => {
                    isCompleted = true;
                    wasSuccessful = true;
                    this.val = val;
                }, ex => {
                    isCompleted = true;
                    wasSuccessful = false;
                    this.ex = ex;
                });
            }

            public void Update() {
                if (isCompleted) {
                    while (!actions.IsEmpty())
                        actions.Dequeue()();
                    factory.Remove(id);
                }
            }

            public IPromise<R> AndThen<R>(
                Func<T, IPromise<R>> onSuccess,
                Func<Exception, IPromise<R>> onError) {

                if (isCompleted) {
                    if (wasSuccessful ?? false) return onSuccess(val);
                    return onError(ex);
                }

                return factory.Create<R>((resolve, reject) => {
                    actions.Enqueue(() => {
                        if (wasSuccessful ?? false) {
                            try {
                                onSuccess(val).AndThen<R>(
                                    r => { resolve(r); return default; },
                                    ex => { reject(ex); return default; });
                            } catch (Exception ex) {
                                onError(ex).AndThen<R>(
                                    r => { resolve(r); return default; },
                                    ex => { reject(ex); return default; });
                            }
                        } else {
                            try {
                                onError(ex).AndThen<R>(
                                    r => { resolve(r); return default; },
                                    ex => { reject(ex); return default; });
                            } catch (Exception e) {
                                onError(e).AndThen<R>(
                                    r => { resolve(r); return default; },
                                    ex => { reject(e); return default; });
                            }
                        }
                    });
                });
            }

        }
    }
}

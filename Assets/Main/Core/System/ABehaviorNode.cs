using System;
using System.Linq;
using UnityEngine;

namespace OSCore.System {
    public enum StateNodeStatus {
        INIT, RUNNING, SUCCESS, FAILURE
    }

    public interface IBehaviorNodeFactory<T> {
        public ABehaviorNode<T> Create(Transform transform);
    }

    public delegate bool Predicate<in T, in U>(T obj, U obj2);

    public abstract class ABehaviorNode<T> {
        protected readonly Transform transform;
        public StateNodeStatus status { get; protected set; } = StateNodeStatus.INIT;
        public bool isFinished =>
            status == StateNodeStatus.SUCCESS
                || status == StateNodeStatus.FAILURE;

        public ABehaviorNode(Transform transform) {
            this.transform = transform;
        }

        public void Process(T details) {
            if (transform is null) throw new Exception($"`Process` cannot be called before `Init`: {GetType()}");
            if (status == StateNodeStatus.INIT) Start(details);
            else if (!isFinished) Continue(details);

            if (status == StateNodeStatus.INIT) status = StateNodeStatus.RUNNING;

            switch (status) {
                //case StateNodeStatus.RUNNING:
                    //Debug.Log(GetType().ToString() + " -> " + status);
                    //break;
                //case StateNodeStatus.FAILURE:
                //    Debug.Log(GetType().ToString() + " -> " + status);
                //    break;
                //case StateNodeStatus.SUCCESS:
                //    Debug.Log(GetType().ToString() + " -> " + status);
                //    break;
                default:
                    break;
            }
        }

        public void ReInit() {
            Stop();
            status = StateNodeStatus.INIT;
        }

        protected virtual void Start(T details) {
            Continue(details);
        }

        protected abstract void Continue(T details);

        protected virtual void Stop() { }
    }

    public class BehaviorNodeFactory<T> : IBehaviorNodeFactory<T> {
        private readonly Func<Transform, ABehaviorNode<T>> createFn;

        public BehaviorNodeFactory(Func<Transform, ABehaviorNode<T>> createFn) {
            this.createFn = createFn;
        }

        public ABehaviorNode<T> Create(Transform transform) =>
            createFn(transform);

        public static ABehaviorNode<T>[] CreateAll(
            Transform transform,
            params IBehaviorNodeFactory<T>[] factories) =>
                factories.Select(factory => factory.Create(transform)).ToArray();
    }
}

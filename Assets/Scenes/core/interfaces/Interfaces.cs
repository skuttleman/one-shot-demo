using System;
using System.Collections;
using System.Collections.Generic;
using OSCore.Data.Enums;
using OSCore.Events.Brains;
using UnityEngine;

namespace OSCore.Interfaces {
    public interface IGameSystem {
        public IGameSystem Send<T>(Action<T> action) where T : IGameSystemComponent;
        public R Send<T, R>(Func<T, R> action) where T : IGameSystemComponent;
    }

    public interface IGameSystemComponent {
        public void Update();
        public void OnDestroy() { }
    }

    namespace Tagging {
        public interface ITagRegistry : IGameSystemComponent {
            public void Register(IdTag tag, GameObject obj);
            public void RegisterUnique(IdTag tag, GameObject obj);
            public ISet<GameObject> Get(IdTag tag);
            public GameObject GetUnique(IdTag tag);
        }
    }

    namespace Events {
        public interface IPubSub : IGameSystemComponent {
            void Publish<T>(T item) where T : IEvent;
            long Subscribe<T>(Action<T> action) where T : IEvent;
            void Unsubscribe(long id);
        }
    }

    namespace Brains {
        public enum EControllerBrainTag {
            PLAYER, CAMERA, SPA
        }

        public interface IControllerBrain : IGameSystemComponent {
            public void Handle(IEvent message);
        }

        public interface IControllerBrainManager : IGameSystemComponent {
            public IControllerBrain Ensure(EControllerBrainTag tag, Transform target);
        }


        public abstract class AControllerBrain<A> : IControllerBrain
            where A : IEvent {
            public void Handle(IEvent ev) {
                switch (ev) {
                    case A e: Handle(e); break;
                    default: Debug.Log("Unhandled event: " + ev); break;
                }
            }
            public abstract void Handle(A e);

            public abstract void Update();
        }

        public abstract class AControllerBrain<A, B, C, D> : IControllerBrain
            where A : IEvent
            where B : IEvent
            where C : IEvent
            where D : IEvent {
            public void Handle(IEvent ev) {
                switch (ev) {
                    case A e: Handle(e); break;
                    case B e: Handle(e); break;
                    case C e: Handle(e); break;
                    case D e: Handle(e); break;
                    default: Debug.Log("Unhandled event: " + ev); break;
                }
            }
            public abstract void Handle(A e);
            public abstract void Handle(B e);
            public abstract void Handle(C e);
            public abstract void Handle(D e);

            public abstract void Update();
        }
    }
}

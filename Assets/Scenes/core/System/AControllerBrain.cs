using OSCore.Data.Events.Brains;
using OSCore.System.Interfaces.Brains;
using UnityEngine;

namespace OSCore.System {
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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.Events {
    namespace Brains {
        public interface IMessage { }

        namespace Player {
            public record PlayerBrainMessage : IMessage {
                public record StringMessage(string message) : PlayerBrainMessage();

                private PlayerBrainMessage() { }
            }
        }
    }
}

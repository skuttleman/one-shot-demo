using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSCore.Events {
    namespace Brains {
        public interface IMessage { }

        namespace Player {
            public static class Messages {
                public static Message Move(Vector2 input) => new Message.Movement(input);
            }

            public record Message : IMessage {
                public record Movement(Vector2 direction) : Message();

                private Message() { }

            }
        }
    }
}

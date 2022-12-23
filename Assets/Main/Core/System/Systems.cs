using OSCore.Data.Enums;
using OSCore.System.Interfaces.Tagging;
using OSCore.System.Interfaces;
using UnityEngine;

namespace OSCore.System {
public static class Systems {
        public static bool HasTag(this IGameSystem system, GameObject obj, IdTag tag) =>
            system.Send<ITagRegistry, bool>(reg =>
                reg.GetUnique(tag) == obj || reg.Get(tag).Contains(obj));

        public static GameObject Player(this IGameSystem system) =>
            system.Send<ITagRegistry, GameObject>(reg =>
                reg.GetUnique(IdTag.PLAYER));
    }
}

using System;
using System.Collections.Generic;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    public abstract class ACharacterAnimatorCfgSO<State, Details> : ScriptableObject
        where Details : AnimStateDetails<State> {
        public readonly IEnumerable<State> states;

        public ACharacterAnimatorCfgSO() {
            states = EnumList<State>();
        }

        public abstract AStateNode<State, Details> Init();

        private static IEnumerable<T> EnumList<T>() {
            Array values = Enum.GetValues(typeof(T));
            IList<T> result = new List<T>();
            foreach (object item in values)
                result.Add((T)item);
            return result;
        }
    }

    public abstract class ACharacterAnimatorCfgSOOld<State, Signal> : ScriptableObject {
        public readonly IEnumerable<State> states;
        public readonly IEnumerable<Signal> signals;

        public ACharacterAnimatorCfgSOOld() {
            states = EnumList<State>();
            signals = EnumList<Signal>();
        }

        public abstract AStateNodeOld<State, Signal> Init();

        private static IEnumerable<T> EnumList<T>() {
            Array values = Enum.GetValues(typeof(T));
            IList<T> result = new List<T>();
            foreach (object item in values)
                result.Add((T)item);
            return result;
        }
    }
}

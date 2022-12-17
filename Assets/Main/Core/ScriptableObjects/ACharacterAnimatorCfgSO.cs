using System;
using System.Collections.Generic;
using OSCore.System;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    public abstract class ACharacterAnimatorCfgSO<State, Signal> : ScriptableObject {
        public readonly IEnumerable<State> states;
        public readonly IEnumerable<Signal> signals;

        public ACharacterAnimatorCfgSO() {
            states = EnumList<State>();
            signals = EnumList<Signal>();
        }

        public abstract AStateNode<State, Signal> Init();

        private static IEnumerable<T> EnumList<T>() {
            Array values = Enum.GetValues(typeof(T));
            IList<T> result = new List<T>();
            foreach (object item in values)
                result.Add((T)item);
            return result;
        }
    }
}

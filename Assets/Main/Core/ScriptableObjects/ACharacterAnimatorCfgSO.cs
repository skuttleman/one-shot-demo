using System;
using System.Collections.Generic;
using OSCore.System;
using UnityEditor;
using UnityEngine;

namespace OSCore.ScriptableObjects {
    public abstract class ACharacterAnimatorCfgSO<State, Details> : ScriptableObject
        where Details : AnimStateDetails<State> {
        [field: SerializeField] public RuntimeAnimatorController animator { get; private set; }

        public readonly IEnumerable<State> states;

        public ACharacterAnimatorCfgSO() {
            states = EnumList<State>();
        }

        public abstract AnimNode<State, Details> Init();

        private static IEnumerable<T> EnumList<T>() {
            Array values = Enum.GetValues(typeof(T));
            IList<T> result = new List<T>();
            foreach (object item in values)
                result.Add((T)item);
            return result;
        }
    }
}

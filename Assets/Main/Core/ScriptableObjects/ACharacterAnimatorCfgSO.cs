using System;
using System.Collections.Generic;
using OSCore.System;
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

    public abstract class AnimSONode<State> : ScriptableObject {
        [HideInInspector] public string id;
        [HideInInspector] public Vector2 position;
        public State state;
        public float animSpeed;
    }

    public abstract class AnimSOEdge : ScriptableObject {
        [HideInInspector] public string from;
        [HideInInspector] public string to;
        public List<AndCondition> conditions;

        [Serializable] public class AndCondition : List<PropComparator> { }

        [Serializable]
        public struct PropComparator {
            public string prop;
            public Comparator comparator;
            public float floatValue;
            public bool boolValue;
        }

        [Serializable]
        public enum Comparator {
            EQ, NE, GT, GTE, LT, LTE
        }
    }
}

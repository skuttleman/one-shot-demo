using System;
using System.Collections.Generic;
using System.Reflection;
using OSCore.System;
using OSCore.Utils;
using UnityEditor;
using UnityEngine;
using static OSCore.ScriptableObjects.AnimSOEdge;

namespace OSCore.ScriptableObjects {
    public abstract class ACharacterAnimatorCfgSO<State, Details> : ScriptableObject
        where Details : AnimStateDetails<State> {
        [field: SerializeField] public RuntimeAnimatorController animator { get; private set; }
        [field: SerializeField] public List<AnimSONode<State>> nodes { get; private set; }
        [field: SerializeField] public List<AnimSOEdge> edges { get; private set; }

        public readonly IEnumerable<State> states;

        public ACharacterAnimatorCfgSO() {
            states = EnumList<State>();
        }

        public abstract AnimNode<State, Details> Init();

        public T CreateNode<T>(Vector2 position) where T : AnimSONode<State> {
            string id = CreateNode<T>(default, position);
            return (T)nodes.Find(node => node.id == id);
        }

        public void DeleteNode(string id) {
            DeleteFrom(edges, edge => edge.from == id || edge.to == id);
            DeleteFrom(nodes, node => node.id == id);
        }

        public void DeleteEdge(string from, string to) {
            DeleteFrom(edges, edge => edge.from == from && edge.to == to);
        }

        public T SetEdge<T>(
            string from,
            string to,
            params AndCondition[] animConditions
        ) where T : AnimSOEdge {
            T edge = Edge<T>(from, to);

            if (edge != null) {
                edge.conditions = new(animConditions);
            } else {
                edge = CreateInstance<T>();
                edge.from = from;
                edge.to = to;
                edge.conditions = new(animConditions);
                edges.Add(edge);
                AssetDatabase.AddObjectToAsset(edge, this);
            }

            return edge;
        }

        public T Node<T>(string id) where T : AnimSONode<State> {
            return (T)nodes.Find(node => node.id == id);
        }

        public T Edge<T>(string from, string to) where T : AnimSOEdge {
            return (T)edges.Find(edge => edge.from == from && edge.to == to);
        }

        protected IDictionary<string, AnimNode<State, Details>> BuildNodes() {
            IDictionary<string, AnimNode<State, Details>> dict =
                nodes.Reduce(
                    (acc, item) => {
                        return acc.With(
                            new KeyValuePair<string, AnimNode<State, Details>>(
                                item.id,
                                new AnimNode<State, Details>(
                                    item.state,
                                    item.animSpeed)));
                    },
                    new Dictionary<string, AnimNode<State, Details>>());

            edges.ForEach(edge => {
                AnimSONode<State> from = nodes.Find(node => node.id == edge.from);
                AnimSONode<State> to = nodes.Find(node => node.id == edge.to);
                if (from == null || to == null) throw new NotSupportedException("BAD");

                dict[from.id].To(ToPred(edge.conditions), dict[to.id]);
            });

            return dict;
        }

        private static void DeleteFrom<T>(List<T> objs, Predicate<T> pred) where T : ScriptableObject {
            objs.RemoveAll(obj => {
                if (pred(obj)) {
                    AssetDatabase.RemoveObjectFromAsset(obj);
                    return true;
                }
                return false;
            });
        }

        protected string CreateNode<T>(State state, Vector2 position, float animSpeed = 1f) where T : AnimSONode<State> {
            string id = GUID.Generate().ToString();
            T node = CreateInstance<T>();
            node.id = id;
            node.position = position;
            node.title = state.ToString();
            node.state = state;
            node.animSpeed = animSpeed;
            nodes.Add(node);
            AssetDatabase.AddObjectToAsset(node, this);

            return id;
        }

        private bool PropMatches(
            object record,
            (string prop, Comparator comparator, float floatValue, bool boolValue) comp
            ) {
            IDictionary<string, PropertyInfo> props = record
                .GetType()
                .GetProperties()
                .Reduce(
                    (m, prop) => { m.Add(prop.Name, prop); return m; },
                    new Dictionary<string, PropertyInfo>());

            object propValue = props[comp.prop].GetValue(record);
            int comparisonResult = propValue switch {
                bool b => b.CompareTo(comp.boolValue),
                _ => comp.floatValue.CompareTo(Convert.ChangeType(propValue, typeof(float))),
            };

            return comp.comparator switch {
                Comparator.EQ => comparisonResult == 0,
                Comparator.NE => comparisonResult != 0,
                Comparator.GT => comparisonResult < 0,
                Comparator.LT => comparisonResult > 0,
                Comparator.GTE => comparisonResult <= 0,
                Comparator.LTE => comparisonResult >= 0,
                _ => false,
            };
        }

        private Predicate<Details> ToPred(List<AndCondition> conditions) {
            return state => {
                foreach (AndCondition group in conditions) {
                    bool all = true;
                    foreach (PropComparator condition in group) {
                        if (!PropMatches(state, (condition.prop, condition.comparator, condition.floatValue, condition.boolValue))) {
                            all = false;
                            break;
                        }
                    }
                    if (all) return true;
                }

                return false;
            };
        }

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
        public string title;
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

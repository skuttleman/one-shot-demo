using UnityEditor;
using UnityEngine;
using static OSCore.ScriptableObjects.AnimSOEdge;

namespace OSEditor.Anim {
    [CustomPropertyDrawer(typeof(OrConditions))]
    public class OrConditionsEditor : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            OrConditions target = (OrConditions)fieldInfo.GetValue(property.serializedObject.targetObject);

            EditorGUI.BeginProperty(position, label, property);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect pos = new(position.x, position.y, position.width, 25);

            Rect change = new(0, 30, 0, 0);

            for (int orIdx = 0; orIdx < target.Count; orIdx ++) {
                AndConditions conditions = target[orIdx];
                EditorGUI.LabelField(pos = Add(pos, change), "----");

                for (int andIdx = 0; andIdx < conditions.Count; andIdx++) {
                    pos = Add(pos, change);
                    float third = pos.width / 3;
                    Rect left = new(pos.x, pos.y, third, pos.height);
                    Rect mid = new(pos.x + third, pos.y, third, pos.height);
                    Rect right = new(pos.x + third + third, pos.y, third, pos.height);
                    PropComparator condition = conditions[andIdx];
                    EditorGUI
                        .LabelField(left, condition.prop);
                    condition.comparator = (Comparator)EditorGUI
                        .EnumPopup(mid, condition.comparator);
                    condition.floatValue = EditorGUI
                        .FloatField(right, condition.floatValue);
                }
            }

            EditorGUI.EndProperty();
        }

        private Rect Add(Rect a, Rect b) =>
            new(a.x + b.x, a.y + b.y, a.width + b.width, a.height + b.height);
    }
}

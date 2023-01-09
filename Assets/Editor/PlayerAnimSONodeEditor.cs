using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using UnityEditor;

namespace OSEditor {
    [CustomEditor(typeof(PlayerAnimSONode))]
    public class PlayerAnimSONodeEditor : Editor {
        public override void OnInspectorGUI() {
            PlayerAnimSONode script = (PlayerAnimSONode)target;

            script.state = (PlayerAnim)EditorGUILayout.EnumPopup("Anim State", script.state);
            script.animSpeed = EditorGUILayout.FloatField("Anim Speed", script.animSpeed);
        }
    }
}

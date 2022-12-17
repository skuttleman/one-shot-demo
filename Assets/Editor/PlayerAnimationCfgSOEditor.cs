using OSCore.ScriptableObjects;
using UnityEditor;

namespace OSEditor {
    [CustomEditor(typeof(PlayerAnimationCfgSO))]
    public class PlayerAnimationCfgSOEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            PlayerAnimationCfgSO script = (PlayerAnimationCfgSO)target;

        }
    }
}

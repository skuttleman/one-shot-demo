using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using OSEditor.TreeGraph;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using static OSCore.ScriptableObjects.PlayerAnimationCfgSO;

namespace OSEditor.Anim {
    [CustomEditor(typeof(PlayerAnimationCfgSO))]
    public class PlayerAnimationCfgSOEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Edit State Graph")) {
                PlayerAnimationCfgSO cfg = (PlayerAnimationCfgSO)target;
                TreeGraphEditorWindow window =
                    EditorWindow.GetWindow<TreeGraphEditorWindow>();
                ITreeGraphAPI api =
                    new PlayerAnimationCfgTreeGraphAPI(window.rootVisualElement, cfg);

                window.Init(api);
                window.Show();
            }
        }
    }

    public class PlayerAnimationCfgTreeGraphAPI :
        ACharacterAnimatorTreeGraphAPI<PlayerAnim, PlayerAnimState, PlayerAnimSONode, PlayerAnimSOEdge> {
        public PlayerAnimationCfgTreeGraphAPI(VisualElement root, PlayerAnimationCfgSO cfg) : base(root, cfg) { }
    }
}

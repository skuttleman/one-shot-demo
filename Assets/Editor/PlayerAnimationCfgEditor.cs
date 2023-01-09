using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using OSCore.ScriptableObjects;

namespace OSEditor {
    public class PlayerAnimationCfgEditor : EditorWindow {
        [SerializeField] private VisualTreeAsset tree;

        [MenuItem("Window/cfg/Mecanim2.D")]
        static void Init() {
            PlayerAnimationCfgEditor window = GetWindow<PlayerAnimationCfgEditor>();
            window.Show();
        }

        private void CreateGUI() {
            tree.CloneTree(rootVisualElement);

            PlayerAnimatorEditorInspectorView inspector =
                rootVisualElement.Q<PlayerAnimatorEditorInspectorView>();
            PlayerAnimationCfgSO cfg = AssetDatabase
                .LoadAssetAtPath<PlayerAnimationCfgSO>("Assets/Config/PlayerAnimatorCfg.asset");

            rootVisualElement.Q<PlayerAnimatorGraphView>()
                .Init(inspector, cfg);
        }
    }
}

using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using OSCore.ScriptableObjects;
using System;

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
            Button element = rootVisualElement.Q<Button>();
            element.clickable.clickedWithEventInfo += OnSave;

            PlayerAnimatorEditorInspectorView inspector =
                rootVisualElement.Q<PlayerAnimatorEditorInspectorView>();
            rootVisualElement.Q<PlayerAnimatorGraphView>()
                .Init(inspector);

        }

        private void OnSave(EventBase obj) {
            if (obj.target is Button) {
                Debug.Log("SAVE");
            }
        }
    }
}

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor.TreeGraph {
    public class TreeGraphEditorWindow : EditorWindow {
        [SerializeField] private VisualTreeAsset tree;
        private ITreeGraphAPI api;

        public void Init(ITreeGraphAPI api) {
            this.api = api;
            Draw();
        }

        private void Draw() {
            rootVisualElement.Clear();
            tree.CloneTree(rootVisualElement);
            rootVisualElement.Q<TreeGraphView>().Init(api);
            rootVisualElement.Q<TreeGraphInspector>().Init(api);
            Button element = rootVisualElement.Q<Button>();
            element.clickable.clickedWithEventInfo += OnSave;
        }

        private void OnSave(EventBase evt) {
            if (evt.target is Button) {
                api.Save();
            }
        }
    }
}

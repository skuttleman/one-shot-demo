using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor {
    public class TreeGraphEditorWindow : EditorWindow {
        [SerializeField] private VisualTreeAsset tree;

        public void Init(ITreeGraphAPI api) {
            tree.CloneTree(rootVisualElement);
            ReInit(api);
        }

        public void ReInit(ITreeGraphAPI api) {
            rootVisualElement.Q<TreeGraphView>().Init(api);
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor {
    public class TreeGraphEditorWindow : EditorWindow {
        [SerializeField] private VisualTreeAsset tree;

        public void Init(ITreeGraphAPI api) {
            rootVisualElement.Clear();
            tree.CloneTree(rootVisualElement);
            rootVisualElement.Q<TreeGraphView>().Init(api);
            rootVisualElement.Q<TreeGraphInspector>().Init(api);
        }
    }
}

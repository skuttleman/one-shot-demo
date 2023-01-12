using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace OSEditor {
    public class TreeGraphInspector : VisualElement {
        private Editor editor;
        private GraphElement target = null;
        private ITreeGraphAPI api;

        public void Init(ITreeGraphAPI api) {
            this.api = api;
        }

        public void UpdateSelection(GraphElement nodeView) {
            switch (target) {
                case TreeGrapStateNode node:
                    api.UnSelect(node);
                    break;
                case Edge edge:
                    api.UnSelect(edge);
                    break;
            }
            Clear();
            Object.DestroyImmediate(editor);
            target = nodeView;

            if (nodeView != null) {
                switch (nodeView) {
                    case TreeGrapStateNode node:
                        editor = Editor.CreateEditor(api.Script(node));
                        Add(new IMGUIContainer(editor.OnInspectorGUI));
                        break;
                    case Edge edge:
                        editor = Editor.CreateEditor(api.Script(edge));
                        Add(new IMGUIContainer(editor.OnInspectorGUI));
                        break;
                }
            }
        }

        public new class UxmlFactory : UxmlFactory<TreeGraphInspector, UxmlTraits> { }
    }
}

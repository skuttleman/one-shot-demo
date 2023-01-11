using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor.Deprecated {
    public class PlayerAnimatorEditorInspectorView : VisualElement {
        private Editor editor;

        public void UpdateSelection(GraphElement nodeView) {
            Clear();
            Object.DestroyImmediate(editor);

            switch (nodeView) {
                case PlayerAnimatorGraphNodeView n:
                    editor = Editor.CreateEditor(n.node);
                    Add(new IMGUIContainer(editor.OnInspectorGUI));
                    break;
                case PlayerAnimatorGraphEdgeView e:
                    editor = Editor.CreateEditor(e.edge);
                    Add(new IMGUIContainer(editor.OnInspectorGUI));
                    break;
            }
        }

        public new class UxmlFactory : UxmlFactory<PlayerAnimatorEditorInspectorView, UxmlTraits> { }
    }
}
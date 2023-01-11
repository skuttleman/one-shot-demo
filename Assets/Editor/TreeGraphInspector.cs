using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor.Experimental.GraphView;

namespace OSEditor {
    public class TreeGraphInspector : VisualElement {
        private Editor editor = null;

        public void UpdateSelection(GraphElement nodeView) {
            Clear();
            Object.DestroyImmediate(editor);

            if (nodeView != null) {

                //switch (nodeView) {
                //    case TreeGraphViewNode n:
                //        editor = Editor.CreateEditor(n);
                //        Add(new IMGUIContainer(editor.OnInspectorGUI));
                //        break;
                //    case PlayerAnimatorGraphEdgeView e:
                //        editor = Editor.CreateEditor(e.edge);
                //        Add(new IMGUIContainer(editor.OnInspectorGUI));
                //        break;
                //}
            }
        }

        public new class UxmlFactory : UxmlFactory<TreeGraphInspector, UxmlTraits> { }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorEditorInspectorView : VisualElement {
    private Editor editor;

    public void UpdateSelection(PlayerAnimatorGraphNodeView nodeView) {
        Clear();
        Object.DestroyImmediate(editor);

        if (nodeView != null) {
            editor = Editor.CreateEditor(nodeView.node);
            Add(new IMGUIContainer(editor.OnInspectorGUI));
        }
    }

    public new class UxmlFactory : UxmlFactory<PlayerAnimatorEditorInspectorView, UxmlTraits> { }
}

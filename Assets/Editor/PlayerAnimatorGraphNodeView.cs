using System;
using OSCore.ScriptableObjects;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorGraphNodeView : Node {
    public readonly PlayerAnimSONode node;

    private readonly Action<GraphElement> remove;
    private readonly PlayerAnimatorEditorInspectorView inspector;
    private readonly Port input;
    private readonly Port output;

    public PlayerAnimatorGraphNodeView(
        PlayerAnimatorEditorInspectorView inspector,
        Vector2 position,
        Action<GraphElement> remove,
        PlayerAnimSONode node) {
        this.inspector = inspector;
        this.remove = remove;
        this.node = node;

        title = node.state.ToString();
        expanded = true;
        SetPosition(new Rect(position, Vector2.zero));

        input = InstantiatePort(
            Orientation.Vertical,
            Direction.Input,
            Port.Capacity.Multi,
            typeof(object));
        output = InstantiatePort(
            Orientation.Vertical,
            Direction.Output,
            Port.Capacity.Multi,
            typeof(object));
        input.portName = "IN";
        output.portName = "OUT";

        inputContainer.Add(input);
        outputContainer.Add(output);
    }

    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
        evt.menu.AppendAction("Delete Node", action => {
            foreach (VisualElement child in input.connections) {
                remove((GraphElement)child);
            }

            foreach (VisualElement child in output.connections) {
                remove((GraphElement)child);
            }

            remove(this);
        });
    }

    public override void OnSelected() {
        base.OnSelected();

        inspector.UpdateSelection(this);
    }
}

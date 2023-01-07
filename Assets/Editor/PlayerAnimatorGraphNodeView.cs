using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerAnimatorGraphNodeView : Node {
    private readonly Action<GraphElement> remove;
    private readonly Port input;
    private readonly Port output;

    public PlayerAnimatorGraphNodeView(Vector2 position, Action<GraphElement> remove) {
        this.remove = remove;
        title = "Anim State";
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
                remove((GraphElement) child);
            }

            foreach (VisualElement child in output.connections) {
                remove((GraphElement)child);
            }

            remove(this);
        });
    }
}

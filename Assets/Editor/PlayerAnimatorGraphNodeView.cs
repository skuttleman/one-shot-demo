using System;
using OSCore.ScriptableObjects;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor {
    public class PlayerAnimatorGraphNodeView : Node {
        public readonly PlayerAnimSONode node;
        public readonly Port input;
        public readonly Port output;

        private readonly Action<GraphElement> remove;
        private readonly PlayerAnimatorEditorInspectorView inspector;

        public PlayerAnimatorGraphNodeView(
            PlayerAnimatorEditorInspectorView inspector,
            Action<GraphElement> remove,
            PlayerAnimSONode node) {
            this.inspector = inspector;
            this.remove = remove;
            this.node = node;

            title = node.state.ToString();
            expanded = true;
            SetPosition(new Rect(node.position, Vector2.zero));

            input = InstantiatePort(
                Orientation.Horizontal,
                Direction.Input,
                Port.Capacity.Multi,
                typeof(object));
            output = InstantiatePort(
                Orientation.Horizontal,
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

    public class PlayerAnimatorGraphEdgeView : Edge {
        public PlayerAnimSOEdge edge;
    }
}
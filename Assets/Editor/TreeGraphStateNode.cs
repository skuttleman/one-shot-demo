using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace OSEditor {
    public class TreeGrapStateNode : Node {
        public readonly Port input;
        public readonly Port output;

        private readonly Action<GraphElement> select;
        private readonly Action<GraphElement> remove;

        public TreeGrapStateNode(
            Action<GraphElement> select,
            Action<GraphElement> remove
        ) {
            this.select = select;
            this.remove = remove;
            expanded = true;

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

            select(this);
        }
    }
}

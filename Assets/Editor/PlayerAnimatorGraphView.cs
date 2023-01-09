using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using OSCore.ScriptableObjects;
using System;

namespace OSEditor {
    public class PlayerAnimatorGraphView : GraphView {
        private PlayerAnimatorEditorInspectorView inspector;

        public PlayerAnimatorGraphView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet style = AssetDatabase
                .LoadAssetAtPath<StyleSheet>("Assets/Editor/PlayerAnimatorGraphView.uss");
            styleSheets.Add(style);

            graphViewChanged += OnGraphViewChanged;
        }

        public void Init(PlayerAnimatorEditorInspectorView inspector) {
            this.inspector = inspector;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            List<Port> result = new();
            ports.ForEach(port => {
                Debug.Log("PORT " + port.node.GetType());
                if (startPort.node != port.node && startPort.direction != port.direction) {
                    result.Add(port);
                }
            });

            return result;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction("Add State", action => {
                AddElement(new PlayerAnimatorGraphNodeView(
                    inspector,
                    action.eventInfo.localMousePosition,
                    RemoveElement,
                    default));
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            Debug.Log("SOMETHING HAPPENED");
            graphViewChange.edgesToCreate?.ForEach(edge => {
                if (edge.input.node is Node e) {
                    Debug.Log("IT IS! " + e);
                }
            });

            return graphViewChange;
        }

        public new class UxmlFactory : UxmlFactory<PlayerAnimatorGraphView, UxmlTraits> { }
    }
}

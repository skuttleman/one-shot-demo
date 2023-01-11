using OSCore.ScriptableObjects;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using OSCore.Data.Animations;

namespace OSEditor.Deprecated {
    public class PlayerAnimatorGraphView : GraphView {
        private PlayerAnimatorEditorInspectorView inspector;
        private PlayerAnimationCfgSO cfg;
        private IDictionary<string, PlayerAnimatorGraphNodeView> nodeViews;
        private IDictionary<(string, string), PlayerAnimatorGraphEdgeView> edgeViews;

        public PlayerAnimatorGraphView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet style = AssetDatabase
                .LoadAssetAtPath<StyleSheet>("Assets/Editor/Deprecated/PlayerAnimatorGraphView.uss");
            styleSheets.Add(style);

            graphViewChanged += OnGraphViewChanged;
        }

        public void Init(PlayerAnimatorEditorInspectorView inspector, PlayerAnimationCfgSO cfg) {
            this.inspector = inspector;
            this.cfg = cfg;

            Draw();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            List<Port> result = new();
            ports.ForEach(port => {
                if (startPort.node != port.node && startPort.direction != port.direction) {
                    result.Add(port);
                }
            });

            return result;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {
            evt.menu.AppendAction("Add State", action => {
                AddNode(cfg.CreateNode<PlayerAnimSONode>(action.eventInfo.localMousePosition));
                AssetDatabase.SaveAssets();
            });
        }






        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.edgesToCreate?.ForEach(edge => {
                if (edge.input.node is PlayerAnimatorGraphNodeView nin
                    && edge.output.node is PlayerAnimatorGraphNodeView nout
                ) {
                    PlayerAnimSOEdge e = cfg.SetEdge<PlayerAnimSOEdge>(nout.node.id, nin.node.id);
                    edge.RegisterCallback<MouseDownEvent>(EdgeClick);
                    edgeViews[(e.from, e.to)] = new PlayerAnimatorGraphEdgeView() { edge = e };
                }
            });
            graphViewChange.elementsToRemove?.ForEach(RemoveGraphElement);
            graphViewChange.movedElements?.ForEach(el => {
                if (el is PlayerAnimatorGraphNodeView n) {
                    n.node.position = el.GetPosition().position;
                }
            });

            AssetDatabase.SaveAssets();
            return graphViewChange;
        }

        private void AddNode(AnimSONode<PlayerAnim> node) {
            PlayerAnimatorGraphNodeView el = new(
                inspector,
                RemoveGraphElement,
                (PlayerAnimSONode)node);
            AddElement(el);
            nodeViews.Add(node.id, el);
        }

        private void AddEdge(AnimSOEdge edge) {
            PlayerAnimatorGraphNodeView from = nodeViews[edge.from];
            PlayerAnimatorGraphNodeView to = nodeViews[edge.to];

            Edge e = from.output.ConnectTo(to.input);
            e.RegisterCallback<MouseDownEvent>(EdgeClick);
            edgeViews[(edge.from, edge.to)] = new PlayerAnimatorGraphEdgeView() { edge = (PlayerAnimSOEdge)edge };
            AddElement(e);
        }

        private void Draw() {
            graphElements.ForEach(RemoveElement);
            nodeViews = new Dictionary<string, PlayerAnimatorGraphNodeView>();
            edgeViews = new Dictionary<(string, string), PlayerAnimatorGraphEdgeView>();
            cfg.nodes.ForEach(AddNode);
            cfg.edges.ForEach(AddEdge);
        }

        private void RemoveGraphElement(GraphElement element) {
            switch (element) {
                case Edge e:
                    if (e.input.node is PlayerAnimatorGraphNodeView nin
                        && e.output.node is PlayerAnimatorGraphNodeView nout
                    ) {
                        cfg.DeleteEdge(nin.node.id, nout.node.id);
                        edgeViews.Remove((nin.node.id, nout.node.id));
                    }
                    break;
                case PlayerAnimatorGraphNodeView n:
                    cfg.DeleteNode(n.node.id);
                    nodeViews.Remove(n.node.id);
                    break;
            }
            RemoveElement(element);
            AssetDatabase.SaveAssets();
        }

        private void EdgeClick(MouseDownEvent evt) {
            Edge edge = (Edge)evt.target;
            if (edge.input.node is PlayerAnimatorGraphNodeView nin
                && edge.output.node is PlayerAnimatorGraphNodeView nout
            ) {
                inspector.UpdateSelection(edgeViews[(nout.node.id, nin.node.id)]);
            }
        }

        public new class UxmlFactory : UxmlFactory<PlayerAnimatorGraphView, UxmlTraits> { }
    }
}

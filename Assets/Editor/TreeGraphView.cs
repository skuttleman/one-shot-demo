using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace OSEditor {
    public interface ITreeGraphAPI {
        public IList<TreeGrapStateNode> nodeViews { get; }
        public IList<Edge> edgeViews { get; }

        public TreeGrapStateNode CreateNode(Vector2 position);
        public Edge CreateEdge(TreeGrapStateNode from, TreeGrapStateNode to);

        public void Select(TreeGrapStateNode node);
        public void Select(Edge edge);
        public void UnSelect(TreeGrapStateNode node);
        public void UnSelect(Edge edge);
        public void OnMove(TreeGrapStateNode node, Vector2 position);

        public void Delete(TreeGrapStateNode node);
        public void Delete(Edge edge);

        public void Save();

        public ScriptableObject Script(Edge edge);
        public ScriptableObject Script(TreeGrapStateNode node);
    }

    public class TreeGraphView : GraphView {
        private ITreeGraphAPI api;

        public TreeGraphView() {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            StyleSheet style = AssetDatabase
                .LoadAssetAtPath<StyleSheet>("Assets/Editor/TreeGraphView.uss");
            styleSheets.Add(style);

            graphViewChanged += OnGraphViewChanged;
        }

        public void Init(ITreeGraphAPI api) {
            this.api = api;
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
            evt.menu.AppendAction("Add Node", action => {
                AddElement(api.CreateNode(action.eventInfo.localMousePosition));
                AssetDatabase.SaveAssets();
            });
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.edgesToCreate?.ForEach(edge =>
                api.CreateEdge(
                    (TreeGrapStateNode)edge.input.node,
                    (TreeGrapStateNode)edge.output.node));
            graphViewChange.elementsToRemove?.ForEach(RemoveGraphElement);
            graphViewChange.movedElements?.ForEach(el => {
                if (el is TreeGrapStateNode n) api.OnMove(n, el.GetPosition().position);
            });

            AssetDatabase.SaveAssets();
            return graphViewChange;
        }

        private void Draw() {
            graphElements.ForEach(RemoveElement);
            foreach (TreeGrapStateNode node in api.nodeViews) AddElement(node);
            foreach (Edge edge in api.edgeViews) AddElement(edge);
        }

        private void RemoveGraphElement(GraphElement element) {
            switch (element) {
                case TreeGrapStateNode n: api.Delete(n); break;
                case Edge e: api.Delete(e); break;
            }
            RemoveElement(element);
        }

        public new class UxmlFactory : UxmlFactory<TreeGraphView, UxmlTraits> { }
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

namespace OSEditor {
    public interface ITreeGraphAPI {
        public IList<TreeGraphViewNode> nodeViews { get; }
        public IList<Edge> edgeViews { get; }

        public TreeGraphViewNode CreateNode(Vector2 position);
        public Edge CreateEdge(TreeGraphViewNode from, TreeGraphViewNode to);
        public void Select(TreeGraphViewNode node);
        public void Select(Edge edge);
        public void OnMove(TreeGraphViewNode node, Vector2 position);
        public void Delete(TreeGraphViewNode node);
        public void Delete(Edge edge);
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
                    (TreeGraphViewNode)edge.input.node,
                    (TreeGraphViewNode)edge.output.node));
            graphViewChange.elementsToRemove?.ForEach(RemoveGraphElement);
            graphViewChange.movedElements?.ForEach(el => {
                if (el is TreeGraphViewNode n) api.OnMove(n, el.GetPosition().position);
            });

            AssetDatabase.SaveAssets();
            return graphViewChange;
        }

        private void Draw() {
            graphElements.ForEach(RemoveElement);
            foreach(TreeGraphViewNode node in api.nodeViews) AddElement(node);
            foreach(Edge edge in api.edgeViews) AddElement(edge);
        }

        private void RemoveGraphElement(GraphElement element) {
            switch (element) {
                case TreeGraphViewNode n: api.Delete(n); break;
                case Edge e: api.Delete(e); break;
            }
            RemoveElement(element);
        }

        public new class UxmlFactory : UxmlFactory<TreeGraphView, UxmlTraits> { }
    }
}

using OSCore.Data.Animations;
using OSCore.ScriptableObjects;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace OSEditor {
    [CustomEditor(typeof(PlayerAnimationCfgSO))]
    public class PlayerAnimationCfgSOEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            if (GUILayout.Button("Edit State Graph")) {
                PlayerAnimationCfgSO cfg = (PlayerAnimationCfgSO)target;
                TreeGraphEditorWindow window =
                    EditorWindow.GetWindow<TreeGraphEditorWindow>();
                ITreeGraphAPI api =
                    new PlayerAnimationCfgTreeGraphAPI(window.rootVisualElement, cfg);

                window.Init(api);
                window.Show();
            }
        }
    }

    public class PlayerAnimationCfgTreeGraphAPI : ITreeGraphAPI {
        public IList<TreeGrapStateNode> nodeViews { get; }
        public IList<Edge> edgeViews { get; }

        private readonly VisualElement root;
        private readonly PlayerAnimationCfgSO cfg;

        private readonly IDictionary<TreeGrapStateNode, AnimSONode<PlayerAnim>> nodeSOLookup;
        private readonly IDictionary<string, TreeGrapStateNode> nodeLookup;
        private readonly IDictionary<Edge, AnimSOEdge> edgeSOLookup;
        private readonly IDictionary<(string, string), Edge> edgeLookup;

        public PlayerAnimationCfgTreeGraphAPI(VisualElement root, PlayerAnimationCfgSO cfg) {
            this.root = root;
            this.cfg = cfg;
            nodeLookup = new Dictionary<string, TreeGrapStateNode>();
            nodeSOLookup = new Dictionary<TreeGrapStateNode, AnimSONode<PlayerAnim>>();
            edgeSOLookup = new Dictionary<Edge, AnimSOEdge>();
            edgeLookup = new Dictionary<(string, string), Edge>();
            nodeViews = new List<TreeGrapStateNode>();
            edgeViews = new List<Edge>();

            foreach (AnimSONode<PlayerAnim> node in cfg.nodes) {
                nodeViews.Add(CreateNode(node));
            }
            foreach (AnimSOEdge edge in cfg.edges) {
                edgeViews.Add(CreateEdge(edge, nodeLookup[edge.from], nodeLookup[edge.to]));
            }
        }

        public Edge CreateEdge(TreeGrapStateNode from, TreeGrapStateNode to) {
            return CreateEdge(cfg.SetEdge<PlayerAnimSOEdge>(nodeSOLookup[from].id, nodeSOLookup[to].id), from, to);
        }

        public TreeGrapStateNode CreateNode(Vector2 position) {
            return CreateNode(cfg.CreateNode<PlayerAnimSONode>(position));
        }

        public void Delete(TreeGrapStateNode node) {
            AnimSONode<PlayerAnim> so = nodeSOLookup[node];
            cfg.DeleteNode(so.id);
            nodeLookup.Remove(so.id);
            nodeSOLookup.Remove(node);
        }

        public void Delete(Edge edge) {
            AnimSOEdge so = edgeSOLookup[edge];
            cfg.DeleteEdge(so.from, so.to);
            edgeLookup.Remove((so.from, so.to));
            edgeSOLookup.Remove(edge);
        }

        public void OnMove(TreeGrapStateNode node, Vector2 position) {
            node.SetPosition(new Rect(position, Vector2.zero));
            nodeSOLookup[node].position = position;
        }

        public void Select(TreeGrapStateNode node) {
            root.Q<TreeGraphInspector>().UpdateSelection(node);
        }

        public void Select(Edge edge) {
            root.Q<TreeGraphInspector>().UpdateSelection(edge);
        }

        public ScriptableObject Script(TreeGrapStateNode node) =>
            nodeSOLookup[node];

        public ScriptableObject Script(Edge edge) =>
            edgeSOLookup[edge];

        public void UnSelect(TreeGrapStateNode node) {
            node.title = nodeSOLookup[node].title;
        }

        public void UnSelect(Edge edge) { }

        private TreeGrapStateNode CreateNode(AnimSONode<PlayerAnim> so) {
            TreeGrapStateNode node = new(Switch(Select, Select), Switch(Delete, Delete));
            node.title = so.title;
            node.SetPosition(new Rect(so.position, Vector2.zero));
            nodeSOLookup[node] = so;
            nodeLookup[so.id] = node;
            return node;
        }

        private Edge CreateEdge(AnimSOEdge so, TreeGrapStateNode from, TreeGrapStateNode to) {
            Edge edge = from.output.ConnectTo(to.input);
            edge.RegisterCallback<MouseDownEvent>(evt => Select((Edge)evt.target));
            edgeSOLookup[edge] = so;
            edgeLookup[(so.from, so.to)] = edge;
            return edge;
        }

        private Action<GraphElement> Switch(Action<TreeGrapStateNode> nodeFn, Action<Edge> edgeFn) => el => {
            switch (el) {
                case TreeGrapStateNode n: nodeFn(n); break;
                case Edge e: edgeFn(e); break;
            }
        };
    }
}

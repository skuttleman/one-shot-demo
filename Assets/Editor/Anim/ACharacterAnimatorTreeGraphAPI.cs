using OSCore.ScriptableObjects;
using OSCore.System;
using OSEditor.TreeGraph;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;

namespace OSEditor.Anim {
    public abstract class ACharacterAnimatorTreeGraphAPI<State, SDetails, NDetails, EDetails> : ITreeGraphAPI
        where SDetails : AnimStateDetails<State>
        where NDetails : AnimSONode<State>
        where EDetails : AnimSOEdge {
        public IList<TreeGrapStateNode> nodeViews { get; }
        public IList<Edge> edgeViews { get; }

        private readonly VisualElement root;
        private readonly ACharacterAnimatorCfgSO<State, SDetails> cfg;

        private readonly IDictionary<TreeGrapStateNode, NDetails> nodeSOLookup;
        private readonly IDictionary<string, TreeGrapStateNode> nodeLookup;
        private readonly IDictionary<Edge, EDetails> edgeSOLookup;
        private readonly IDictionary<(string, string), Edge> edgeLookup;

        public ACharacterAnimatorTreeGraphAPI(
            VisualElement root, ACharacterAnimatorCfgSO<State, SDetails> cfg
        ) {
            this.root = root;
            this.cfg = cfg;
            nodeLookup = new Dictionary<string, TreeGrapStateNode>();
            nodeSOLookup = new Dictionary<TreeGrapStateNode, NDetails>();
            edgeSOLookup = new Dictionary<Edge, EDetails>();
            edgeLookup = new Dictionary<(string, string), Edge>();
            nodeViews = new List<TreeGrapStateNode>();
            edgeViews = new List<Edge>();

            foreach (NDetails node in cfg.nodes) {
                nodeViews.Add(CreateNode(node));
            }
            foreach (EDetails edge in cfg.edges) {
                edgeViews.Add(CreateEdge(edge, nodeLookup[edge.from], nodeLookup[edge.to]));
            }
        }

        public Edge CreateEdge(TreeGrapStateNode from, TreeGrapStateNode to) {
            return CreateEdge(cfg.SetEdge<EDetails>(nodeSOLookup[from].id, nodeSOLookup[to].id), from, to);
        }

        public TreeGrapStateNode CreateNode(Vector2 position) {
            return CreateNode(cfg.CreateNode<NDetails>(position));
        }

        public void Delete(TreeGrapStateNode node) {
            NDetails so = nodeSOLookup[node];
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

        public void Save() {
            AssetDatabase.SaveAssets();
        }

        private TreeGrapStateNode CreateNode(NDetails so) {
            TreeGrapStateNode node = new(Switch(Select, Select), Switch(Delete, Delete));
            node.title = so.title;
            node.SetPosition(new Rect(so.position, Vector2.zero));
            nodeSOLookup[node] = so;
            nodeLookup[so.id] = node;
            return node;
        }

        private Edge CreateEdge(EDetails so, TreeGrapStateNode from, TreeGrapStateNode to) {
            Edge edge = from.output.ConnectTo(to.input);
            edge.RegisterCallback<MouseDownEvent>(evt => Select((Edge)evt.target));
            edgeSOLookup[edge] = so;
            edgeLookup[(so.from, so.to)] = edge;
            return edge;
        }

        private Action<GraphElement> Switch(Action<TreeGrapStateNode> nodeFn, Action<Edge> edgeFn) => el => {
            switch (el) {
                case Edge e: edgeFn(e); break;
                default: nodeFn((TreeGrapStateNode)el); break;
            }
        };
    }
}
using QuikGraph;
using UniRx;
using UnityEngine;
using Utilities;
using Zenject;

namespace Power.Steam.Network
{
    public class SteamGraphDebugger : GraphDrawer
    {
        private GridGraph<NodeHandle> _gridGraph;

        private AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> _graph =  new();
        [Inject]
        public void InjectMe(GridGraph<NodeHandle> gridGraph)
        {
            _gridGraph = gridGraph;
            Subject<Vector2Int> addVertex = new Subject<Vector2Int>();
            Subject<Vector2Int> addVertexAndEdge = new Subject<Vector2Int>();
            Subject<SEdge<Vector2Int>> addEdge = new Subject<SEdge<Vector2Int>>();
            addVertex.Where(_ => !_graph.ContainsVertex(_)).Do(_ => _graph.AddVertex(_)).Subscribe(_ => Draw());
            addEdge.Where(_ => !_graph.ContainsEdge(_)).Do(_ => _graph.AddEdge(_)).Subscribe(_ => Draw());
            addVertexAndEdge.Buffer(2).Subscribe(t =>
            {
                Debug.Log($"Adding edge {t[0]} {t[1]}");
                addVertex.OnNext(t[0]);
                addVertex.OnNext(t[1]);
                addEdge.OnNext(new SEdge<Vector2Int>(t[0], t[1]));
            });
     
            gridGraph.OnNodeAdded.Select(t => (Vector2Int)t.Position).Where(t => !_graph.ContainsVertex(t)).Subscribe(addVertex);
            gridGraph.OnEdgeAdded.Select(t => (Vector2Int)t.Source.Position).Where(t => !_graph.ContainsVertex(t)).Subscribe(addVertexAndEdge);
            gridGraph.OnEdgeAdded.Select(t => (Vector2Int)t.Target.Position).Where(t => !_graph.ContainsVertex(t)).Subscribe(addVertexAndEdge);
            gridGraph.OnEdgeAdded
                .Select(t => new SEdge<Vector2Int>((Vector2Int)t.Source.Position, (Vector2Int)t.Target.Position))
                .Subscribe(addEdge);
        }
        public override AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> GetGraph() => _graph;
    }
}
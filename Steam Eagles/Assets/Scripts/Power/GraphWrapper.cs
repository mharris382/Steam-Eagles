using System;
using QuikGraph;
using UniRx;

public class GraphWrapper : IDisposable
{
    private Subject<GridNode> _onVertexAdded = new();
    private Subject<GridNode> _onVertexRemoved = new();
    private Subject<SEdge<GridNode>> _onEdgeAdded = new();
    private Subject<SEdge<GridNode>> _onEdgeRemoved = new();
    
    private AdjacencyGraph<GridNode, SEdge<GridNode>> _graph;
    public AdjacencyGraph<GridNode, SEdge<GridNode>> Graph => _graph;

    private IDisposable _disposable;

    public IObservable<GridNode> OnNodeAdded => _onVertexAdded;
    public IObservable<GridNode> OnNodeRemoved => _onVertexRemoved;
    public IObservable<SEdge<GridNode>> OnEdgeAdded => _onEdgeAdded;
    public IObservable<SEdge<GridNode>> OnEdgeRemoved => _onEdgeRemoved;
    
    public GraphWrapper()
    {
        _graph = new();
        CompositeDisposable cd = new();
        
        VertexAction<GridNode> addAction = vertex => { _onVertexAdded.OnNext(vertex); };
        VertexAction<GridNode> removeAction = v => { _onVertexRemoved.OnNext(v);};
        EdgeAction<GridNode, SEdge<GridNode>> addEdge = e => { _onEdgeAdded.OnNext(e); };
        EdgeAction<GridNode, SEdge<GridNode>> removeEdge = e => { _onEdgeRemoved.OnNext(e); };
        _graph.VertexAdded += addAction;
        _graph.VertexRemoved += removeAction;
        _graph.EdgeAdded += addEdge;
        _graph.EdgeRemoved += removeEdge;
        Disposable.Create(() => _graph.VertexAdded -= addAction).AddTo(cd);
        Disposable.Create(() => _graph.VertexRemoved -= removeAction).AddTo(cd);
        Disposable.Create(() => _graph.EdgeAdded -= addEdge).AddTo(cd);
        Disposable.Create(() => _graph.EdgeRemoved -= removeEdge).AddTo(cd);
        _disposable = cd;
    }

    public bool AddVertex(GridNode node)
    {
        return Graph.AddVertex(node);
    }
    public bool AddDirectionalEdge(GridNode src, GridNode dst)
    {
        return Graph.AddVerticesAndEdge(new SEdge<GridNode>(src, dst));
    }
    public bool AddBiDirectionalEdge(GridNode src, GridNode dst)
    {
        return Graph.AddVerticesAndEdge(new SEdge<GridNode>(src, dst)) &&
               Graph.AddVerticesAndEdge(new SEdge<GridNode>(dst, src));
    }

    public void Dispose()
    {
        _onVertexAdded?.Dispose();
        _onVertexRemoved?.Dispose();
        _onEdgeAdded?.Dispose();
        _onEdgeRemoved?.Dispose();
        _disposable?.Dispose();
    }
}
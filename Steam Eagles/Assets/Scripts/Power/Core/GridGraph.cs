using System;
using System.Collections.Generic;
using QuikGraph;
using UniRx;
using UnityEngine;

public class GridGraph<T>
{
    private static Vector3Int[] _directions = new Vector3Int[] {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };
    public delegate T UndirectedEdgeTagResolver(GridNode source, GridNode target);
    
    Dictionary<Vector3Int, GridNode> _usedNodes = new();
    AdjacencyGraph<GridNode, TaggedUndirectedEdge<GridNode, T>> graph= new();
    Queue<TaggedUndirectedEdge<GridNode,T>> _edgeCache = new();
    
    Subject<GridNode> _onNodeAdded = new();
    Subject<GridNode> _onNodeRemoved = new();
    Subject<TaggedUndirectedEdge<GridNode, T>> _onEdgeAdded = new();
    Subject<TaggedUndirectedEdge<GridNode, T>> _onEdgeRemoved = new();

    private Predicate<TaggedUndirectedEdge<GridNode, T>> _canAddEdge;
    
    public IObservable<GridNode> OnNodeRemoved => _onNodeRemoved;
    public IObservable<GridNode> OnNodeAdded => _onNodeAdded;
    public IObservable<TaggedUndirectedEdge<GridNode, T>> OnEdgeAdded => _onEdgeAdded;
    public IObservable<TaggedUndirectedEdge<GridNode, T>> OnEdgeRemoved => _onEdgeRemoved;

    public AdjacencyGraph<GridNode, TaggedUndirectedEdge<GridNode, T>> Graph => graph;
    public Predicate<TaggedUndirectedEdge<GridNode, T>> CanAddEdge
    {
        get => _canAddEdge ?? (_ => true); 
        set => _canAddEdge = value;
    }
    public bool HasNode(Vector3Int node) => _usedNodes.ContainsKey(node);
    public bool HasNode(Vector2Int node) => HasNode((Vector3Int)node);
    
    public bool AddNode(GridNode node)
    {
        if (HasNode(node.Position)) return false;
        graph.AddVertex(node);
        _usedNodes.Add(node.Position, node);
        _edgeCache.Clear();
        if (!CheckForEdge(Vector3Int.up) ||
            !CheckForEdge(Vector3Int.down) || 
            !CheckForEdge(Vector3Int.left) ||
            !CheckForEdge(Vector3Int.right))
        {
            graph.RemoveVertex(node);
            _edgeCache.Clear();
            _usedNodes.Remove(node.Position);
            return false;
        }

        //at this point we know we can add the node
        
        _onNodeAdded.OnNext(node);
        while (_edgeCache.Count > 0)
        {
            var e = _edgeCache.Dequeue();
            graph.AddEdge(e);
            _onEdgeAdded.OnNext(e);
        }
        bool CheckForEdge(Vector3Int i)
        {
            var neighbor = node.Position + i;
            if (HasNode(neighbor))
            {
                var neighborNode = _usedNodes[neighbor];
                if (neighborNode == null)
                {
                    _usedNodes.Remove(neighbor);
                    return true;
                }
                var newEdge = CreateEdge(node, neighborNode);
                if(!CanAddEdge(newEdge))
                    return false;
                _edgeCache.Enqueue(newEdge);
                return true;
            }
            return true;
        }
        return true;
    }
    TaggedUndirectedEdge<GridNode, T> CreateEdge(GridNode source, GridNode target)
    {
        try
        {
            return new TaggedUndirectedEdge<GridNode, T>(source, target, default);
        }
        catch (Exception e)
        {
            return new TaggedUndirectedEdge<GridNode, T>(target, source, default);
        }
    }

    public GridNode GetNode(Vector3Int position)
    {
        if (!HasNode(position)) return null;
        return _usedNodes[position];
    }

    public GridNode GetNode(Vector2Int position) => GetNode((Vector3Int)position);
    public  int GetNeighborCount(Vector3Int position)
    {
        int cnt = 0;
        foreach (var direction in _directions)
        {
            var offset = position + direction;
            if (HasNode(offset))
            {
                cnt++;
            }
        }
        return cnt;
    }
    public  int GetNeighborCount(Vector3Int position, out GridNode[] neighbors)
    {
        int cnt = 0;
        neighbors = new GridNode[4];
        foreach (var direction in _directions)
        {
            var offset = position + direction;
            if (HasNode(offset))
            {
                neighbors[cnt++] = _usedNodes[offset];
            }
        }
        return cnt;
    }
    public bool RemoveNode(Vector3Int position)
    {
        if (!HasNode(position)) return false;
        return RemoveNode(_usedNodes[position]);
    }
    public bool RemoveNode(Vector2Int position) => RemoveNode((Vector3Int)position);
    public bool RemoveNode(GridNode node)
    {
        if(!HasNode(node.Position))return false;
        _edgeCache.Clear();
        if (graph.TryGetOutEdges(node, out var edges))
        {
            foreach (var edge in edges) 
                _edgeCache.Enqueue(edge);
        }
        graph.RemoveVertex(node);
        while (_edgeCache.Count > 0)
        {
            var edge =_edgeCache.Dequeue();
            graph.RemoveEdge(edge);
            _onEdgeRemoved.OnNext(edge);
        }
        _onNodeRemoved.OnNext(node);
        return true;
    }
}

// public interface IGridGraphLogic<TVertex, TEdge> where TVertex : GridNode where TEdge : IEdge<TVertex>
// {
//     /// <summary>
//     /// determine if the given position is valid for a new vertex
//     /// </summary>
//     /// <param name="position"></param>
//     /// <returns></returns>
//     bool IsPositionValid(Vector3Int position);
//     int GetMaxAllowedConnections(TVertex vertex);
//     bool CanConnect(TVertex from, TVertex to, ref string msg);
// }
//
// public abstract class GridGraphBase<TVertex, TEdge>
//     where TVertex : GridNode 
//     where TEdge : IEdge<TVertex>,
//     IGraphSubjects<TVertex, TEdge>
// {
//     protected static Vector3Int[] cardinalDirections = new Vector3Int[] { Vector3Int.up,Vector3Int.down, Vector3Int.left, Vector3Int.right };
//     private readonly IFactory<Vector3Int, TVertex> _defaultVertexFactory;
//     private readonly IFactory<TVertex, TVertex, TEdge> _edgeFactory;
//     private readonly IGridGraphLogic<TVertex, TEdge> _graphLogic;
//     private readonly Subjects _graphSubjects = new();
//     private  Queue<TEdge> _edgeCache = new();
//     private Dictionary<Vector3Int, TVertex> _usedPositions = new();
//     
//     
//     public IObservable<TVertex> OnNodeAdded => _graphSubjects.OnNodeAdded;
//     public IObservable<TVertex> OnNodeRemoved => _graphSubjects.OnNodeRemoved;
//     public IObservable<TEdge> OnEdgeAdded => _graphSubjects.OnEdgeAdded;
//     public IObservable<TEdge> OnEdgeRemoved => _graphSubjects.OnEdgeRemoved;
//     
//     
//     class Subjects : IGraphSubjects<TVertex, TEdge>
//     {
//         Subject<TVertex> _onNodeAdded;// = new();
//         Subject<TVertex> _onNodeRemoved;// = new();
//         Subject<TEdge> _onEdgeAdded ;//= new();
//         Subject<TEdge> _onEdgeRemoved;// = new();
//
//         public void EdgeAdded(TEdge edge) => _onEdgeAdded?.OnNext(edge);
//
//         public void EdgeRemoved(TEdge edge) => _onEdgeRemoved?.OnNext(edge);
//
//         public void NodeAdded(TVertex node) => _onNodeAdded?.OnNext(node);
//
//         public void NodeRemoved(TVertex node) => _onNodeRemoved?.OnNext(node);
//
//         public IObservable<TVertex> OnNodeAdded => _onNodeAdded??=new();
//         public IObservable<TVertex> OnNodeRemoved => _onNodeRemoved??=new();
//         public IObservable<TEdge> OnEdgeAdded => _onEdgeAdded??=new();
//         public IObservable<TEdge> OnEdgeRemoved => _onEdgeRemoved??=new();
//     }
//     public GridGraphBase(
//         IFactory<Vector3Int, TVertex> defaultVertexFactory, 
//         IFactory<TVertex, TVertex, TEdge> edgeFactory, 
//         IGridGraphLogic<TVertex, TEdge> graphLogic)
//     {
//         _defaultVertexFactory = defaultVertexFactory;
//         _edgeFactory = edgeFactory;
//         _graphLogic = graphLogic;
//     }
//     
//     public bool HasVertex(Vector3Int position) => _usedPositions.ContainsKey(position);
//     public TVertex GetVertex(Vector3Int position)
//     {
//         if (!HasVertex(position))
//         {
//             return null;
//         }
//         return _usedPositions[position];
//     }
//     
//     public IEnumerable<TVertex> GetNeighbors(Vector3Int position)
//     {
//         foreach (var vector3Int in cardinalDirections)
//         {
//             var neighborPosition = position + vector3Int;
//             if (HasVertex(neighborPosition))
//             {
//                 yield return GetVertex(neighborPosition);
//             }
//         }
//     }
//     public int GetNeighborsFor(Vector3Int position, out TVertex[] vertices)
//     {
//         int cnt = 0;
//         vertices = new TVertex[4];
//         foreach (var direction in cardinalDirections)
//         {
//             var neighborPosition = position + direction;
//             if (HasVertex(neighborPosition))
//             {
//                 vertices[cnt++] = GetVertex(neighborPosition);
//             }
//         }
//         return cnt;
//     }
//     public bool RemoveVertex(Vector3Int position)
//     {
//         if (!_usedPositions.ContainsKey(position))
//             return false;
//         return RemoveVertex(_usedPositions[position]);
//     }
//     public bool RemoveVertex(TVertex vertex)
//     {
//         if (!HasVertex(vertex))
//         {
//             return false;
//         }
//         INTERNAL_RemoveVertex(vertex);
//         return true;
//     }
//
//     public bool CanAddVertexAtPosition(Vector3Int position)
//     {
//         if (HasVertex(position))
//         {
//             return false;
//         }
//         if (!_graphLogic.IsPositionValid(position))
//         {
//             return false;
//         }
//         string msg = "";
//         var cnt = GetNeighborsFor(position, out var neighbors);
//         for (int i = 0; i < cnt; i++)
//         {
//             var existingVertex = neighbors[i];
//             if (!CanConnect(position, existingVertex, ref msg))
//             {
//                 Debug.Log(msg);
//                 return false;
//             }
//         }
//         return true;
//     }
//
//     public bool CanAddNode(TVertex vertex)
//     {
//         string msg = "";
//         var cnt = GetNeighborsFor(vertex.Position, out var neighbors);
//         for (int i = 0; i < cnt; i++)
//         {
//             var existingVertex = neighbors[i];
//             if (!CanConnect(vertex, existingVertex, ref msg))
//             {
//                 Debug.Log(msg);
//                 return false;
//             }
//         }
//         return true;
//     }
//
//     public bool TryAddVertex(TVertex newVertex)
//     {
//         var position = newVertex.Position;
//         if (CanAddVertexAtPosition(position))
//         {
//             
//         }
//     }
//     public bool TryAddVertex(Vector3Int position, out TVertex newVertex)
//     {
//         newVertex = default;
//         if (!CanAddVertexAtPosition(position))
//         {
//             return false;
//         }
//         newVertex = _defaultVertexFactory.Create(position);
//         INTERNAL_AddVertex(newVertex);
//         return true;
//     }
//     private void INTERNAL_RemoveVertex(TVertex vertex)
//     {
//         AddVertexToGraph(vertex);
//         _graphSubjects.NodeAdded(vertex);
//         RemoveEdgesFromQueue();
//     }
//     private void INTERNAL_AddVertex(TVertex vertex)
//     {
//         AddVertexToGraph(vertex);
//         _graphSubjects.NodeAdded(vertex);
//         AddEdgesFromQueue();
//     }
//     private void RemoveEdgesFromQueue()
//     {
//         while (_edgeCache.Count > 0)
//         {
//             var edgeToRemove = _edgeCache.Dequeue();
//             RemoveEdgeFromGraph(edgeToRemove);
//             _graphSubjects.EdgeRemoved(edgeToRemove);
//         }
//     }
//     private void AddEdgesFromQueue()
//     {
//         while (_edgeCache.Count > 0)
//         {
//             var newEdge = _edgeCache.Dequeue();
//             AddEdgeToGraph(newEdge);
//             _graphSubjects.EdgeAdded(newEdge);
//         }
//     }
//     protected abstract void RemoveEdgeFromGraph(TEdge edge);
//     protected abstract void RemoveVertexFromGraph(TVertex vertex);
//     protected abstract void AddEdgeToGraph(TEdge edge);
//     protected abstract void AddVertexToGraph(TVertex vertex);
//     protected abstract bool CanConnect(TVertex from, TVertex to, ref string msg);
// }
//
// public interface IGraphSubjects<TVertex, TEdge> where TVertex : GridNode where TEdge : IEdge<TVertex>
// {
//     IObservable<TVertex> OnNodeAdded { get; } 
//     IObservable<TVertex> OnNodeRemoved { get; }
//     IObservable<TEdge> OnEdgeAdded { get; }
//     IObservable<TEdge> OnEdgeRemoved { get; }
// }
//
// public abstract class GridGraph<TVertex, TEdge> where TVertex : GridNode where TEdge : IEdge<TVertex>
// {
//     static Vector3Int[] directions = new Vector3Int[] { Vector3Int.up,Vector3Int.down, Vector3Int.left, Vector3Int.right };
//
//     private IFactory<Vector3Int, TVertex> _defaultVertexFactory;
//     private IFactory<TVertex, TVertex, TEdge> _edgeFactory;
//     
//     Dictionary<Vector3Int, TVertex> _usedPositions = new();
//     AdjacencyGraph<TVertex, TEdge> graph = new();
//     Queue<TEdge> _edgeCache = new();
//     
//     Predicate<TEdge> _canAddEdge;
//     Predicate<TVertex> _canAddVertex;
//     Predicate<Vector3Int> _isPositionValid;
//
//     Subject<TVertex> _onNodeAdded = new();
//     Subject<TVertex> _onNodeRemoved = new();
//     Subject<TEdge> _onEdgeAdded = new();
//     Subject<TEdge> _onEdgeRemoved = new();
//     
//     public IObservable<TVertex> OnNodeRemoved => _onNodeRemoved;
//     public IObservable<TVertex> OnNodeAdded => _onNodeAdded;
//     public IObservable<TEdge> OnEdgeAdded => _onEdgeAdded;
//     public IObservable<TEdge> OnEdgeRemoved => _onEdgeRemoved;
//
//     public Predicate<TEdge> CanAddEdge
//     {
//         get => _canAddEdge ?? (_ => true);
//         set => _canAddEdge = value;
//     }
//     public Predicate<TVertex> CanAddVertex
//     {
//         get => _canAddVertex ?? (_ => true);
//         set => _canAddVertex = value;
//     }
//     public Predicate<Vector3Int> IsPositionValid
//     {
//         get => _isPositionValid ?? (_ => true);
//         set => _isPositionValid = value;
//     }
//
//
//
//     public bool HasVertex(Vector3Int position) => _usedPositions.ContainsKey(position);
//
//     public TVertex GetVertex(Vector3Int position)
//     {
//         if (!HasVertex(position))
//         {
//             return null;
//         }
//         return _usedPositions[position];
//     }
//
//     /// <summary>
//     /// gets all neighbor vertices of the given position, does not
//     /// matter if the given position has a vertex or not
//     /// </summary>
//     /// <param name="position"></param>
//     /// <returns></returns>
//     public IEnumerable<TVertex> GetNeighbors(Vector3Int position)
//     {
//         foreach (var vector3Int in directions)
//         {
//             var neighborPosition = position + vector3Int;
//             if (HasVertex(neighborPosition))
//             {
//                 yield return GetVertex(neighborPosition);
//             }
//         }    
//     }
//
//     /// <summary>
//     /// tries to add a vertex at the given position using the given vertex factory to create the vertex object
//     /// </summary>
//     /// <param name="position">the position to create the new vertex at</param>
//     /// <param name="vertexFactory">needed so that rather than passing in the already created vertex object, we can delay the creation
//     /// of the vertex object until after we are certain that the given position is valid</param>
//     /// <returns>true if the position was valid and a vertex was created</returns>
//     /// <exception cref="NotImplementedException"></exception>
//     public bool AddVertex(Vector3Int position)
//     {
//         if (!CanAddVertexAtPosition(position))
//         {
//             return false;
//         }
//         var newVertex = _defaultVertexFactory.Create(position);
//         throw new NotImplementedException();
//     }
//
//     public bool CanAddVertexAtPosition(Vector3Int position)
//     {
//         if (HasVertex(position))
//         {
//             return false;
//         }
//
//         if (!CanAddVertexAtPosition(position))
//         {
//             
//             return false;
//         }
//
//
//         return true;
//     }
//
//     /// <summary>
//     /// checks to see if it would be possible to add an edge between the two given positions
//     /// assuming that the first position does have a vertex by the time the edge is added,
//     /// NOTE: the first position does not need to have a vertex at this time
//     /// </summary>
//     /// <param name="hypotheticalNode">does not need vertex in order to be valid</param>
//     /// <param name="existingNode">needs to already have vertex in order to be valid</param>
//     /// <returns></returns>
//     public bool IsHypotheticalEdgeValid(Vector3Int hypotheticalNode, Vector3Int existingNode)
//     {
//         
//         return true;
//     }
//
//
//     public bool IsConcreteEdgeValid(TVertex srcNode, TVertex dstNode)
//     {
//         if (!HasVertex(srcNode) || !HasVertex(dstNode))
//         {
//             return false;
//         }
//         
//         return true;
//     }
//     
//     public bool RemoveVertex(Vector3Int position)
//     {
//         if(!_usedPositions.TryGetValue(position, out var vertex))
//             return false;
//         if (!graph.ContainsVertex(vertex))
//         {
//             _usedPositions.Remove(position);
//             return false;
//         }
//         _edgeCache.Clear();
//         _usedPositions.Remove(position);
//         graph.RemoveVertex(vertex);
//         _onNodeRemoved.OnNext(vertex);
//         while (_edgeCache.Count > 0)
//         {
//             var edgeToRemove = _edgeCache.Dequeue();
//             graph.RemoveEdge(edgeToRemove);
//             _onEdgeRemoved.OnNext(edgeToRemove);
//         }
//         return true;
//     }
//     
//     
// }
using System;
using CoreLib.Extensions;
using QuikGraph;
using UniRx;
using UnityEngine;

public class GridGraph2D
{
    AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> _graph = new();
    Subject<Vector2Int> _onNodeAdded = new();
    Subject<Vector2Int> _onNodeRemoved = new();
    public IObservable<Vector2Int> OnNodeAdded => _onNodeAdded;
    public IObservable<Vector2Int> OnNodeRemoved => _onNodeRemoved;
    public AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> Graph => _graph;
    public void AddNode(Vector2Int position)
    {
        if (_graph.ContainsVertex(position))
            return;
        
        _graph.AddVertex(position);
        foreach (var neighbor in position.Neighbors())
        {
            if(_graph.ContainsVertex(neighbor))
            {
                _graph.AddEdge(new SEdge<Vector2Int>(position, neighbor));
                _graph.AddEdge(new SEdge<Vector2Int>(neighbor, position));
            }
        }
        _onNodeAdded.OnNext(position);
    }

    public void RemoveNode(Vector2Int position)
    {
        _graph.RemoveVertex(position);
    }
}
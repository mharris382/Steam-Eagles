﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using CoreLib.Extensions;
using QuikGraph;
using QuikGraph.Algorithms;
using UnityEngine;
using Zenject;
using UniRx;

namespace Power.Steam.Network
{
    public class PipeGridGraph : IInitializable
    {
        private readonly NodeRegistry _nodes;
        private readonly NodeHandle.Factory _nodeHandleFactory;
        private GridGraph2D _graph;
        private Dictionary<Vector2Int, int> _components = new();

        public  PipeGridGraph(
            SteamConsumers consumers, 
            SteamProducers producers,
            NodeRegistry nodes,
            NodeHandle.Factory nodeHandleFactory)
        {
            _nodes = nodes;
            _nodeHandleFactory = nodeHandleFactory;
            Consumers = consumers;
            Producers = producers;
            _graph = new();
        }

        public SteamConsumers Consumers { get; }
        public SteamProducers Producers { get; }

        public AdjacencyGraph<Vector2Int, SEdge<Vector2Int>> PipeGraph => _graph.Graph;

        public void Initialize()
        {
            _nodes.OnValueAdded.Where(t => t != null).Select(t => t.Position2D).Subscribe(Add);
            Consumers.OnSystemAdded.Select(t => t.Item1).Subscribe(Add);
            Producers.OnSystemAdded.Select(t => t.Item1).Subscribe(Add);
            
            _nodes.OnValueRemoved.Where(t => t != null).Select(t => t.Position2D).Subscribe(Remove);
            Consumers.OnSystemRemoved.Select(t => t.Item1).Subscribe(Remove);
            Producers.OnSystemRemoved.Select(t => t.Item1).Subscribe(Remove);
        }
        private void Remove(Vector2Int position)
        {
            if (!_graph.Graph.ContainsVertex(position))
                return;
            _graph.Graph.RemoveVertex(position);
            DirtyGraph(position);
        }

        private void Add(Vector2Int pos)
        {
            if (!_graph.Graph.ContainsVertex(pos))
            {
                _graph.AddNode(pos);
                Debug.Assert(_components.ContainsKey(pos) == false, $"Components already contains {pos}");
                _components.Add(pos, -1);
                DirtyGraph(pos);
            }
        }

        private void DirtyGraph(Vector2Int position)
        {
            foreach (var neighbor in position.Neighbors())
            {
                if(_components.ContainsKey(neighbor))
                    _components[neighbor] = -1;
            }
        }

        public int GetComponent(Vector2Int position)
        {
            if (_graph.Graph.ContainsVertex(position) == false)
            {
                Debug.LogError($"Graph does not contain vertex {position}");
                return -1;
            }
            if (_components.ContainsKey(position) && _components[position] != -1)
            {
                _components.Add(position, -1);
            }
            var count = _graph.Graph.WeaklyConnectedComponents(_components);
            return _components[position];
        }
    }

public class NodeRegistry : Registry<NodeHandle>
    {
        private static Vector2Int[] directions = new[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        [System.Obsolete("", false)]
        private readonly GridGraph<NodeHandle> _graph;
        private Dictionary<Vector2Int, NodeHandle> _handles = new();
        public int nextGUID = 0;
        private Dictionary<GridNode, int> _nodeComponents = new();
        private Dictionary<int, bool> _dirtyComponents = new();
        private bool _dirty;
        private int _componentCount;
        public int GetNextGUID() => nextGUID++;

        public NodeRegistry(GridGraph<NodeHandle> graph) : base()
        {
            _graph = graph;
        }

        protected override void AddValue(NodeHandle value)
        {
            if (!_graph.AddNode(value.Position))
            {
                Debug.LogError($"Failed to add node at {value.Position}");
                return;
            }

            if (_handles.ContainsKey(value.Position2D))
            {
                _handles[value.Position2D] = value;
            }
            else
            {
                _handles.Add(value.Position2D, value);
            }
            _dirty = true;
            base.AddValue(value);
        }
        protected override void RemoveValue(NodeHandle value)
        {
            if (_nodeComponents.ContainsKey(value.Position))
            {
                if (_dirtyComponents.ContainsKey(_nodeComponents[value.Position]))
                    _dirtyComponents[_nodeComponents[value.Position]] = true;
                else _dirtyComponents.Add(_nodeComponents[value.Position], true);
            }
            _dirty = true;
            _graph.RemoveNode(value.Position);
            _handles.Remove(value.Position2D);
            base.RemoveValue(value);
        }

        public int Count => _graph.Graph.VertexCount;
        
        
        
        public int CountConnectedComponents()
        {
            if (_dirty)
            {
                RecalculateConnectedComponents();
                _dirty = false;
            }
            return _componentCount;
        }

        private void RecalculateConnectedComponents()
        {
           _componentCount = _graph.Graph.WeaklyConnectedComponents(_nodeComponents);
        }

        public int GetComponentID(Vector3Int position) => GetComponentID((Vector2Int)position);
        public int GetComponentID(Vector2Int position)
        {
            if (_dirty)
            {
                RecalculateConnectedComponents();
                _dirty = false;
            }
            var pos =(Vector3Int)position;
            if (!_graph.HasNode(pos))
                return -1;
            var gridNode = _graph.GetNode(pos);
            if (_nodeComponents.TryGetValue(gridNode, out var componentID))
                return componentID;
            return -1;
        }
        public GridNode GetValue(Vector3Int position) => GetValue((Vector2Int)position);
        public GridNode GetValue(Vector2Int position)
        {
            return _graph.GetNode(position);
        }
        public NodeHandle GetHandle(Vector2Int position)
        {
            if (!_handles.ContainsKey(position))
            {
                return null;
            }
            return _handles[position];
        }
        public bool HasValue(Vector2Int position)
        {
            return _graph.HasNode(position);
        }
        public bool HasValue(Vector3Int position)
        {
            return HasValue((Vector2Int)position);
        }
        public IEnumerable<(int component, GridNode node)> GetAdjacentComponents(Vector2Int position, bool includeSelf = false)
        {
            foreach (var vector2Int in directions)
            {
                var pos = position + vector2Int;
                int component = GetComponentID(pos);
                if (component != -1)
                    yield return (component, _graph.GetNode(pos));
            }
            if(includeSelf)
            {
                var id = GetComponentID(position);
                if (id != -1)
                    yield return (id, _graph.GetNode(position));
            } 
        }


        public bool IsComponentDirty(int component)
        {
            if (_dirtyComponents.ContainsKey(component) == false)
            {
                _dirtyComponents.Add(component, true);
            }
            return _dirtyComponents[component];
        }
        
        public void ClearDirtyComponents()
        {
            foreach (var component in _dirtyComponents.Keys.ToArray())
            {
                if(_dirtyComponents[component])
                    _dirtyComponents[component] = false;
            }
        }

        public List<Vector2Int> GetPath(Vector2Int src, Vector2Int dst)
        {
            if (!_graph.Graph.ContainsVertex((Vector3Int)src))
            {
                foreach (var neighbor in src.Neighbors())
                {
                    if(_graph.Graph.ContainsVertex((Vector3Int)neighbor))
                    {
                        src = neighbor;
                        break;
                    }
                }
            }
            if (!_graph.Graph.ContainsVertex((Vector3Int)dst))
            {
                foreach (var neighbor in dst.Neighbors())
                {
                    if (_graph.Graph.ContainsVertex((Vector3Int)neighbor))
                    {
                        dst = neighbor;
                        break;
                    }
                }
            }

            var startNode = GetValue(src);
            var endNode = GetValue(dst);
            
            Debug.Assert(_graph.Graph.ContainsVertex(startNode));
            Debug.Assert(_graph.Graph.ContainsVertex(endNode));
            Debug.Assert(GetComponentID(startNode.Position) == GetComponentID(endNode.Position));
            
            var tryFunc = _graph.Graph.ShortestPathsDijkstra(e => 1, startNode);
            var tryFunc2 = _graph.Graph.ShortestPathsDijkstra(e => 1, endNode);
            var path = new List<Vector2Int>();
            if (tryFunc(endNode, out var pathList))
            {
                foreach (var node in pathList)
                {
                    path.Add((Vector2Int)node.Source.Position);
                    path.Add((Vector2Int)node.Target.Position);
                }
            }
            else if (tryFunc2(startNode, out var pathList2))
            {
                var list = pathList2.ToList();
                list.Reverse();
                pathList2 = list;
                foreach (var node in pathList2)
                {
                    path.Add((Vector2Int)node.Source.Position);
                    path.Add((Vector2Int)node.Target.Position);
                }
            }
            else
            {
                throw new Exception("Path not found");
            }
            return path;
        }
    }
}
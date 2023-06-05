using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using QuikGraph.Algorithms;
using UnityEngine;

namespace Power.Steam.Network
{
    public class NodeRegistry : Registry<NodeHandle>
    {
        private static Vector2Int[] directions = new[] {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };
        
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
                Unregister(value);
                Debug.LogError($"Failed to add node at {value.Position}");
                return;
            }
            _handles.Add(value.Position2D, value);
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
            var startNode = GetValue(src);
            var endNode = GetValue(dst);
            var tryFunc = _graph.Graph.ShortestPathsDijkstra(e => 1, startNode);
            var path = new List<Vector2Int>();
            if (tryFunc(endNode, out var pathList))
            {
                foreach (var node in pathList)
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
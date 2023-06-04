using System;
using System.Collections.Generic;
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
        public int nextGUID = 0;
        private Dictionary<GridNode, int> _nodeComponents = new();
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
            _dirty = true;
            base.AddValue(value);
        }
        protected override void RemoveValue(NodeHandle value)
        {
            _dirty = true;
            _graph.RemoveNode(value.Position);
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

        public GridNode GetValue(Vector2Int position)
        {
            return _graph.GetNode(position);
        }
        public bool HasValue(Vector2Int position)
        {
            return _graph.HasNode(position);
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
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Power.Steam.Network
{
    public class SteamNetworkTopology : INetworkTopology
    {
        private readonly NodeHandle.Factory _nodeHandleFactory;
        private readonly NodeRegistry _nodeRegistry;
        private readonly GridGraph<NodeHandle> _gridGraph;
        private readonly Dictionary<Vector2Int, NodeHandle> _usedPositions = new();

        public SteamNetworkTopology(
            NodeHandle.Factory nodeHandleFactory,
            NodeRegistry nodeRegistry,
            GridGraph<NodeHandle> gridGraph)
        {
            _nodeHandleFactory = nodeHandleFactory;
            _nodeRegistry = nodeRegistry;
            _gridGraph = gridGraph;
        }
        public NodeHandle AddNode(Vector2Int position, NodeType nodeType)
        {
            if (_usedPositions.ContainsKey(position))
            {
                if (_usedPositions[position] != null)
                {
                    if (_usedPositions[position].NodeType == nodeType)
                    {
                        return _usedPositions[position];
                    }

                    if (nodeType != NodeType.PIPE)
                    {
                        _usedPositions.Remove(position);
                    }
                    else
                    {
                        return _usedPositions[position];
                    }
                }
                else
                {
                    _usedPositions.Remove(position);
                }
                
            }
            var handle = _nodeHandleFactory.Create((Vector3Int)position, nodeType);
            _usedPositions.Add(position, handle);
            return handle;
        }

        public void RemoveNode(Vector2Int position)
        {
            if (_usedPositions.ContainsKey(position))
            {
                _nodeRegistry.Unregister(_usedPositions[position]);
                _usedPositions.Remove(position);
            }
        }

        public void ConnectNodes(Vector2Int positionA, Vector2Int positionB)
        {
            
        }

        public void DisconnectNodes(Vector2Int positionA, Vector2Int positionB)
        {
            
        }

        public IEnumerable<Vector2Int> GetUsedPositions() => _gridGraph.Graph.Vertices.Select(t => (Vector2Int)t.Position);
    }
}
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
            if (_nodeRegistry.HasValue(position))
            {
                var node = _nodeRegistry.GetHandle(position);
                if (nodeType != NodeType.PIPE && node.NodeType == NodeType.PIPE)
                {
                    _nodeRegistry.Unregister(node);
                    
                }
                else
                {
                    return node;
                }
            }
            var handle = _nodeHandleFactory.Create((Vector3Int)position, nodeType);
            return handle;
        }

        public void RemoveNode(Vector2Int position)
        {
            if (_nodeRegistry.HasValue(position))
            {
                _nodeRegistry.Unregister(_nodeRegistry.GetHandle(position));
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
using System;
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
            var handle = _nodeHandleFactory.Create((Vector3Int)position, nodeType);
            throw new NotImplementedException();
            return handle;
        }

        public void RemoveNode(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public void ConnectNodes(Vector2Int positionA, Vector2Int positionB)
        {
            throw new NotImplementedException();
        }

        public void DisconnectNodes(Vector2Int positionA, Vector2Int positionB)
        {
            throw new NotImplementedException();
        }
    }
}
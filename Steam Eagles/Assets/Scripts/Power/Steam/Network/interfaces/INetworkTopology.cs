using System.Collections.Generic;
using UnityEngine;

namespace Power.Steam.Network
{
    public interface INetworkTopology
    {
        NodeHandle AddNode(Vector2Int position, NodeType nodeType);
        void RemoveNode(Vector2Int position);
        void ConnectNodes(Vector2Int positionA, Vector2Int positionB);
        void DisconnectNodes(Vector2Int positionA, Vector2Int positionB);
        
        IEnumerable<Vector2Int> GetUsedPositions();
    }
}
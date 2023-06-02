using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    public class NodeHandle
    {
        public class Factory : PlaceholderFactory<Vector3Int, NodeType, NodeHandle> { }

        public int ID { get; }
        public NodeType NodeType { get; }
        public Vector3Int Position { get; }

        public NodeHandle(Vector3Int position, NodeType type, NodeRegistry nodeRegistry)
        {
            this.Position = position;
            this.NodeType = type;
            this.ID = nodeRegistry.GetNextGUID();
            nodeRegistry.Register(this);
        }
    }
}
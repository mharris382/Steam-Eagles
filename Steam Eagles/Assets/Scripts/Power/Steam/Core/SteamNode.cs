using UnityEngine;

namespace Power.Steam.Core
{
    public struct SteamNode : IPowerNetworkNode
    {
        public SteamNode(Vector3Int position, PowerNodeType nodeType, Steam steam)
        {
            Position = position;
            NodeType = nodeType;
            Steam = steam;
        }

        public float Power => Steam.pressure;
        public Vector3Int Position { get; }
        public PowerNodeType NodeType { get; }
        public Steam Steam { get; }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
    
    
}
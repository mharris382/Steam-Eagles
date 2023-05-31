using UnityEngine;

namespace Power.Steam.Core
{
    public struct SteamNode : IPowerNetworkNode
    {
        public SteamNode(Vector3Int position)
        {
            Position = position;
        }

        public Vector3Int Position { get; }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }
    
    public struct Pipe : IPowerNetworkEdge
    {
        public Pipe(SteamNode source, SteamNode target)
        {
            Source = source;
            Target = target;
        }

        public SteamNode Source { get; }
        public SteamNode Target { get; }

        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ Target.GetHashCode();
        }
    }
}
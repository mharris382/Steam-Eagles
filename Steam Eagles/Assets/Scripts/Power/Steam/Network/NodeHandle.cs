using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    public class NodeHandle
    {
        private readonly ISteamProcessing _steamProcessing;

        public class Factory : PlaceholderFactory<Vector3Int, NodeType, NodeHandle> { }

        public int ID { get; }
        public NodeType NodeType { get; }
        public Vector3Int Position { get; }
        public Vector2Int Position2D => new(Position.x, Position.y);

        public float Temperature
        {
            get =>  _steamProcessing.GetTemperature(Position2D);
            set => _steamProcessing.SetTemperature(Position2D, value);
        }

        public float Pressure
        {
          get =>_steamProcessing.GetPressureLevel(Position2D);
          set => _steamProcessing.SetPressureLevel(Position2D, value);
        } 
        
        public NodeHandle(Vector3Int position, NodeType type, NodeRegistry nodeRegistry, ISteamProcessing steamProcessing)
        {
            _steamProcessing = steamProcessing;
            this.Position = position;
            this.NodeType = type;
            this.ID = nodeRegistry.GetNextGUID();
            if (!nodeRegistry.HasValue(position))
            {
                nodeRegistry.Register(this);
            }
        }

        public override string ToString()
        {
            return $"{Position2D} {NodeType} {ID}\nPressure: {Pressure}Pa\nTemperature: {Temperature}K";
        }
    }
}
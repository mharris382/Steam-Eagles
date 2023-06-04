using System;
using UnityEngine;

namespace Power.Steam.Network
{
    public class NetworkSteamProcessing  : ISteamProcessing
    {
        public SteamConsumers SteamConsumers { get; }
        private readonly INetworkTopology _networkTopology;
        private readonly SteamProducers _steamProducers;
        private readonly SteamConsumers _steamConsumers;
        public NetworkSteamProcessing(INetworkTopology networkTopology, SteamProducers steamProducers, SteamConsumers steamConsumers)
        {
            SteamConsumers = steamConsumers;
            _networkTopology = networkTopology;
            _steamProducers = steamProducers;
        }
        public void UpdateSteamState(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public bool HasPosition(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public float GetSteamFlowRate(Vector2Int p0, Vector2Int p1)
        {
            throw new NotImplementedException();
        }

        public float GetPressureLevel(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public float GetTemperature(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Vector2Int position)
        {
            throw new NotImplementedException();
        }
    }
}
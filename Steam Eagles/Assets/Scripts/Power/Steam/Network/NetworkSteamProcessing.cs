using System;
using UnityEngine;

namespace Power.Steam.Network
{
    public class NetworkSteamProcessing : ISteamProcessing
    {
        private readonly INetworkTopology _networkTopology;

        public NetworkSteamProcessing(INetworkTopology networkTopology)
        {
            _networkTopology = networkTopology;
        }
        public void UpdateSteamState(float deltaTime)
        {
            throw new NotImplementedException();
        }

        public float GetSteamFlowRate(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public float GetPressureLevel(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Vector2Int position)
        {
            throw new NotImplementedException();
        }
    }
}
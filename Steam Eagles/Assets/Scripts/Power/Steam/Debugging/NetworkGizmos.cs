using System;
using Buildings;
using Power.Steam.Network;
using UnityEngine;
using Zenject;

namespace Power.Steam.Debugging
{
    public class NetworkGizmos : MonoBehaviour
    {
        
        private Building _building;
        private SteamConsumers _consumers;
        private SteamProducers _producers;
        private NodeRegistry _steamNodes;

        [Inject]
        public void Inject(Building building, SteamConsumers consumers, SteamProducers producers, NodeRegistry steamNodes)
        {
            _consumers = consumers;
            _producers = producers;
            _building = building;
            _steamNodes = steamNodes;
        }

        bool HasResources()
        {
            return _consumers != null && _producers != null && _building != null && _steamNodes != null;
        }


        public void OnDrawGizmos()
        {
            if (!HasResources()) 
                return;
            var gridSize = _building.Map.GetCellSize(BuildingLayers.PIPE);
            
        }
    }
}
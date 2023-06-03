using System;
using System.Collections.Generic;
using UnityEngine;

namespace Power.Steam.Network
{
    public class SteamNetwork 
    {
        private readonly NodeHandle.Factory _nodeHandleFactory;
        private readonly NodeRegistry _nodeRegistry;
        private readonly GridGraph<NodeHandle> _gridGraph;
        private readonly INetworkTopology _networkTopology;
        private readonly ISteamProcessing _steamProcessing;
        private readonly ISteamEventHandling _steamEventHandling;

        public SteamNetwork(
            NodeHandle.Factory nodeHandleFactory, 
            NodeRegistry nodeRegistry,
            GridGraph<NodeHandle> gridGraph,
            INetworkTopology networkTopology,
            ISteamProcessing steamProcessing, 
            ISteamEventHandling steamEventHandling)
        {
            this._nodeHandleFactory = nodeHandleFactory;
            this._nodeRegistry = nodeRegistry;
            _gridGraph = gridGraph;
            _networkTopology = networkTopology;
            _steamProcessing = steamProcessing;
            _steamEventHandling = steamEventHandling;
        }

        public NodeHandle AddNode(Vector2Int position, NodeType nodeType) => _networkTopology.AddNode(position, nodeType);

        public void RemoveNode(Vector2Int position) => _networkTopology.RemoveNode(position);

        public void ConnectNodes(Vector2Int positionA, Vector2Int positionB) => _networkTopology.ConnectNodes(positionA, positionB);

        public void DisconnectNodes(Vector2Int positionA, Vector2Int positionB) => _networkTopology.DisconnectNodes(positionA, positionB);
        public IEnumerable<Vector2Int> GetUsedPositions()
        {
            return _networkTopology.GetUsedPositions();
        }

        public void UpdateSteamState(float deltaTime) => _steamProcessing.UpdateSteamState(deltaTime);



        public bool HasPosition(Vector2Int position)
        {
            return _steamProcessing.HasPosition(position);
        }

        public float GetSteamFlowRate(Vector2Int p1, Vector2Int p2)
        {
            return _steamProcessing.GetSteamFlowRate(p1, p2);
        }

        public float GetPressureLevel(Vector2Int position) => _steamProcessing.GetPressureLevel(position);
        public float GetTemperature(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsBlocked(Vector2Int position) => _steamProcessing.IsBlocked(position);

        public IObservable<GasConsumedEventData> GasConsumedObservable => _steamEventHandling.GasConsumedObservable;

        public IObservable<GasProducedEventData> GasProducedObservable => _steamEventHandling.GasProducedObservable;
    }


    public interface INetwork : ISteamProcessing, INetworkTopology, ISteamEventHandling
    {
        
    }


}
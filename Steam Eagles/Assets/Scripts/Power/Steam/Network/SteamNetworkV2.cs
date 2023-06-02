using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    public class SteamNetwork : INetwork
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

        public void UpdateSteamState(float deltaTime) => _steamProcessing.UpdateSteamState(deltaTime);

        public float GetSteamFlowRate(Vector2Int position) => _steamProcessing.GetSteamFlowRate(position);

        public float GetPressureLevel(Vector2Int position) => _steamProcessing.GetPressureLevel(position);

        public bool IsBlocked(Vector2Int position) => _steamProcessing.IsBlocked(position);

        public IObservable<GasConsumedEventData> GasConsumedObservable => _steamEventHandling.GasConsumedObservable;

        public IObservable<GasProducedEventData> GasProducedObservable => _steamEventHandling.GasProducedObservable;
    }

    public class SteamNetworkInstaller : Installer<SteamNetworkInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindFactory<Vector3Int, NodeType, NodeHandle, NodeHandle.Factory>().AsSingle().NonLazy();
            Container.Bind<GridGraph<NodeHandle>>().To<NodeGraph>().AsSingle().NonLazy();
            Container.Bind<INetworkTopology>().To<SteamNetworkTopology>().AsSingle().NonLazy();
            Container.Bind<ISteamEventHandling>().To<SteamNetworkEventHandler>().AsSingle().NonLazy();
            Container.Bind<ISteamProcessing>().To<NetworkSteamProcessing>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SteamNetwork>().AsSingle().NonLazy();
        }
        public class NodeGraph : GridGraph<NodeHandle>{ }
    }


    public interface INetwork : ISteamProcessing, INetworkTopology, ISteamEventHandling
    {
        
    }


}
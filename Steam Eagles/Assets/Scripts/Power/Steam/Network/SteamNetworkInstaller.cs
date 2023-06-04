using UnityEngine;
using Zenject;

namespace Power.Steam.Network
{
    public class SteamNetworkInstaller : Installer<SteamNetworkInstaller>
    {
        public override void InstallBindings()
        {
         Container.BindFactory<Vector3Int, NodeType, NodeHandle, NodeHandle.Factory>().AsSingle().NonLazy();
         Container.Bind<GridGraph<NodeHandle>>().To<NodeGraph>().AsSingle().NonLazy();
         Container.Bind<NodeRegistry>().AsSingle().NonLazy();
         Container.Bind<INetworkTopology>().To<SteamNetworkTopology>().AsSingle().NonLazy();
         Container.Bind<ISteamEventHandling>().To<SteamNetworkEventHandler>().AsSingle().NonLazy();
         Container.BindInterfacesTo<NetworkSteamProcessing>().AsSingle().NonLazy();
         Container.Bind<INetwork>().To<SteamNetwork>().AsSingle().NonLazy();
         
         // Container.BindFactory<Vector2Int, SteamState, SteamState.Factory>().AsSingle().NonLazy();
         // Container.BindFactory<Vector2Int, Vector2Int, SteamFlow, SteamFlow.Factory>().AsSingle().NonLazy();
        }
        public class NodeGraph : GridGraph<NodeHandle>{ }
    }
}
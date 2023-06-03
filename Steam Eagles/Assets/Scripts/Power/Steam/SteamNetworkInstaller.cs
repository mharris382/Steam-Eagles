using Buildings;
using Power.Steam.Network;
using UnityEngine;
using Zenject;

public class SteamNetworkInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        //Power.Steam.Network.SteamNetworkInstaller.Install(Container);
        Container.Bind<INetwork>().FromSubContainerResolve()
            .ByInstaller<Power.Steam.Network.SteamNetworkInstaller>().AsSingle().NonLazy();
        Container.BindInterfacesTo<Tester>().AsSingle().NonLazy();
    }
    
    
    class Tester : IInitializable
    {
        private readonly INetwork _steamNetwork;
        private readonly Building _building;

        public Tester(INetwork steamNetwork, Building building)
        {
            _steamNetwork = steamNetwork;
            _building = building;
        }

        public void Initialize()
        {
            Debug.Assert(_steamNetwork != null);
            Debug.Log($"Initialized steam network for {_building.name}", _building);
        }
    }
}
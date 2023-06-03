using UnityEngine;
using Zenject;

public class SteamNetworkInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Power.Steam.Network.SteamNetworkInstaller.Install(Container);
    }
}
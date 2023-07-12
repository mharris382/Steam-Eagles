using a;
using UnityEngine;
using Zenject;

namespace AI.Bots
{
    public class BotGlobalInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BotWhitelist>().AsSingle().NonLazy();
        }
    }

}
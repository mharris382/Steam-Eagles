using System.ComponentModel;
using UnityEngine;
using Zenject;

namespace Items.Installer
{
    public class ItemInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<ItemCache>().AsSingle().NonLazy();
        }
    }
}
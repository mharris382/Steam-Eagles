using CoreLib.Signals;
using UnityEngine;
using Zenject;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(PlayerCharacterGUIController))]
    public class PlayerCharacterGUIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GUIRunner>().AsSingle().NonLazy();
            Container.Bind<PlayerCharacterGUIController>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }


    //public class UIPrefabFactory : Placeholder
}
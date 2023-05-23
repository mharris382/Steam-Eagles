using Buildings.Rooms.Tracking;
using Players.PCController;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Items.UI.HUDScrollView
{
    [InfoBox("Should be bound at the SceneContext level")]
    public class PlayerHUDInstaller : MonoInstaller
    {
        [Required, AssetsOnly]
        public PlayerHUD playerHUDPrefab;
       
        public override void InstallBindings()
        { 
            Container.BindFactoryCustomInterface<PC, PlayerHUDSystem, PlayerHUDSystem.Factory, ISystemFactory<PlayerHUDSystem>>();
            Container.BindInterfacesAndSelfTo<PlayerHUDSystems>().AsSingle().NonLazy();
            Container.Bind<PlayerHUDPrefab>().FromInstance(new PlayerHUDPrefab(playerHUDPrefab)).AsSingle().NonLazy();
            Container.BindFactory<PlayerHUD, PlayerHUDInstance, PlayerHUDInstance.Factory>().FromFactory<PrefabFactory<PlayerHUDInstance>>();
        }
    }
}
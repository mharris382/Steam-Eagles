using Buildings.Rooms.Tracking;
using Players.PCController;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Items.UI.HUDScrollView
{
    public class PlayerHUDInstaller : MonoInstaller
    {
        [ValidateInput(nameof(ValidatePrefab))]
        public GameObject playerHUDPrefab;

        bool ValidatePrefab(GameObject prefab, ref string error)
        {
            if (prefab == null)
            {
                error = "Missing playerHUDPrefab reference";
                return false;
            }

            if (prefab.GetComponent<PlayerHUD>() == null)
            {
                error = "Missing PlayerHUD component on playerHUDPrefab";
                return false;
            }
            return true;
        }
        public override void InstallBindings()
        {
          // Container.BindFactoryCustomInterface<PC, PlayerHUDSystem, PlayerHUDSystem.Factory,
          //         ISystemFactory<PlayerHUDSystem>>();
          // Container.BindInterfacesAndSelfTo<PlayerHUDSystems>().AsSingle().NonLazy();
            
        }
    }
}
using System.Linq;
using Buildings.Rooms;
using SaveLoad;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Buildings.DI
{
    [System.Obsolete("use ZenjectBinding instead")]
    public class BuildingSceneInstaller : MonoInstaller
    {
        [Required, SceneObjectsOnly, ValidateInput(nameof(ValidateBuildingGoContext))]
        public GameObject buildingGameObjectContext;
        public override void InstallBindings()
        {
            ReflectedInstaller<ILayerSpecificRoomTexSaveLoader>.Install(Container, ContainerLevel.SCENE);
        }


        bool ValidateBuildingGoContext(GameObject context, ref string msg)
        {
            if (context == null) return true;
            var building = context.GetComponentInChildren<Building>();
            if (building == null)
            {
                msg = "must have a Building component in the hierarchy";
                return false;
            }

            
          
            var goContext = context.GetComponent<GameObjectContext>();
            if (goContext == null)
            {
                msg = "Building Parent is missing GameObjectContext";
                return false;
            }

            int installerCount = goContext.Installers.Count();
            var prefabInstallerCount = goContext.InstallerPrefabs.Count();
            var soInstallerCount = goContext.ScriptableObjectInstallers.Count();
            var total = installerCount + prefabInstallerCount + soInstallerCount;
            if (total == 0)
            {
                msg = "GameObjectContext has no installers";
                return false;
            }
            
            var rooms = building.GetComponentsInChildren<Room>();
            if (rooms.Length == 0)
            {
                msg = "Building should have 1 or more rooms";
                return false;
            }
           
            return true;
        }
    }
}
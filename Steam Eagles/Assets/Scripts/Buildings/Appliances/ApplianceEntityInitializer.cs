using CoreLib.MyEntities;
using UnityEngine;

namespace Buildings.Appliances
{
    [RequireComponent(typeof(BuildingAppliance))]
    public class ApplianceEntityInitializer : EntityInitializer
    {
        private BuildingAppliance _buildingAppliance;
        public BuildingAppliance BuildingAppliance => _buildingAppliance != null ? _buildingAppliance : _buildingAppliance = GetComponent<BuildingAppliance>();
        
        public override string GetEntityGUID()
        {
            return BuildingAppliance.GetEntityGUID();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.APPLIANCE;
        }

        public override void OnEntityInitialized(Entity entity)
        {
            BuildingAppliance.IsEntityInitialized = true;
        }

        public override bool IsReadyToLoad()
        {
            var building = GetComponentsInParent<EntityInitializer>();
            if(building.Length == 0)
                return true;
            foreach (var entityInitializer in building)
            {
                if(!entityInitializer.isDoneInitializing)
                    return false;
            }
            return true;
        }
    }
}
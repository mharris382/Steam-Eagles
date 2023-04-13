using CoreLib.Entities;
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
    }
}
using System;
using Buildings.Appliances;
using CoreLib.Entities.Factory;
using JetBrains.Annotations;

namespace CoreLib.Entities.Buildings
{
    [System.Obsolete("Use ApplianceSaveLoader instead")]
    [UsedImplicitly]
    public class ApplianceFactory : EntityFactoryBase<ApplianceSaveData>
    {
        public override EntityType GetEntityType() => EntityType.APPLIANCE;

        protected override void LoadFromSaveData(Entity entity, ApplianceSaveData data)
        {
            var appliance =entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            appliance.IsOn = data.isApplianceOn;
        }

        protected override ApplianceSaveData GetSaveDataFromEntity(Entity entity)
        {
            var saveData = new ApplianceSaveData();
            var appliance = entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            saveData.isApplianceOn = appliance.IsOn;
            return saveData;
        }
    }
}
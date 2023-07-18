using Buildings.Appliances;
using CoreLib.MyEntities.Factory;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CoreLib.MyEntities.Buildings
{
    [UsedImplicitly]
    public class ApplianceSaveLoader : EntityJsonSaveLoaderBase<ApplianceSaveData>
    {
        public override EntityType GetEntityType() => EntityType.APPLIANCE;
        public override UniTask<ApplianceSaveData> GetDataFromEntity(EntityHandle entity)
        {
            var saveData = new ApplianceSaveData();
            var appliance = entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            saveData.isApplianceOn = appliance.IsOn;
            return UniTask.FromResult(saveData);
        }

        public override UniTask<bool> LoadDataIntoEntity(EntityHandle entity, ApplianceSaveData data)
        {
            var appliance =entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            appliance.IsOn = data.isApplianceOn;
            return UniTask.FromResult(true);
        }

        protected void LoadFromSaveData(Entity entity, ApplianceSaveData data)
        {
            var appliance =entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            appliance.IsOn = data.isApplianceOn;
        }

        protected ApplianceSaveData GetSaveDataFromEntity(Entity entity)
        {
            var saveData = new ApplianceSaveData();
            var appliance = entity.LinkedGameObject.GetComponent<BuildingAppliance>();
            saveData.isApplianceOn = appliance.IsOn;
            return saveData;
        }
    }
}
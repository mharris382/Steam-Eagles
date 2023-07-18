using Buildings;
using Buildings.SaveLoad;
using CoreLib.MyEntities.Factory;
using UnityEngine;

namespace CoreLib.MyEntities.Buildings
{
    [System.Obsolete("Use BuildingSaveLoader instead")]
    public class BuildingFactory : EntityFactoryBase<BuildingSaveData>
    {
        public override EntityType GetEntityType() => EntityType.BUILDING;

        protected override void LoadFromSaveData(Entity entity, BuildingSaveData data)
        {
            Debug.Assert(entity.LinkedGameObject != null, "Entity has no linked game object", entity);
            
            var building = entity.LinkedGameObject.GetComponent<Building>();
            building.IsFullyLoaded = false;
            
            data.Load(building, () =>
            {
                Debug.Log($"Completed loading building {building.ID}", building);
                building.IsFullyLoaded = true;
            });
        }

        protected override BuildingSaveData GetSaveDataFromEntity(Entity entity)
        {
            Debug.Assert(entity.LinkedGameObject != null, "Entity has no linked game object", entity);
            var building = entity.LinkedGameObject.GetComponent<Building>();
            return new BuildingSaveData(building);
        }
    }
}
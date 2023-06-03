using Buildings;
using Buildings.SaveLoad;
using CoreLib.Entities.Factory;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CoreLib.Entities.Buildings
{
    [UsedImplicitly]
    public class BuildingSaveLoader : EntityJsonSaveLoaderBase<BuildingSaveData>
    {
        public override EntityType GetEntityType() => EntityType.BUILDING;

        public override async UniTask<BuildingSaveData> GetDataFromEntity(EntityHandle entityHandle)
        {
            var wrapper = entityHandle.LinkedGameObject.GetComponent<BuildingSaveDataWrapper>();
            if (wrapper == null)
            {
                Debug.LogError("Missing BuildingSaveDataWrapper on building", entityHandle.LinkedGameObject);
            }
            else
            {
                Debug.Log("Saving Building Data Now", wrapper);
                await wrapper.saveDataV3.SaveGame();
                Debug.Log("Finished Saving Building Data", wrapper);
            }
            var building = entityHandle.LinkedGameObject.GetComponent<Building>();
            Debug.Log($"Started Saving building {building.name}", entityHandle.LinkedGameObject);
            var saveData = new BuildingSaveData(building);
            return saveData;
        }

        public override async UniTask<bool> LoadDataIntoEntity(EntityHandle entityHandle, BuildingSaveData data)
        {
            var building = entityHandle.LinkedGameObject.GetComponent<Building>();
            Debug.Log($"Started Loading building {building.name}", entityHandle.LinkedGameObject);

            var wrapper = entityHandle.LinkedGameObject.GetComponent<BuildingSaveDataWrapper>();
            if (wrapper == null)
            {
                Debug.LogError("Missing BuildingSaveDataWrapper on building", entityHandle.LinkedGameObject);
                return false;
            }
            building.IsFullyLoaded = false;
            var result = await data.LoadAsync(building);
            var result2 =  await wrapper.saveDataV3.LoadGame();
            if(!result) Debug.LogError("Failed to Load Building Save Data", building);
            if (!result2) Debug.LogError("Tilemaps Save Data Failed to load in building", building);
            building.IsFullyLoaded = true;
            
            if (!result || !result2) Debug.LogError("Failed to load building", building);
            Debug.Log($"Finished Loading building {building.name}", entityHandle.LinkedGameObject);
            
            return result;
        }
        
        protected void LoadFromSaveData(Entity entity, BuildingSaveData data)
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

        protected BuildingSaveData GetSaveDataFromEntity(Entity entity)
        {
            Debug.Assert(entity.LinkedGameObject != null, "Entity has no linked game object", entity);
            var building = entity.LinkedGameObject.GetComponent<Building>();
            return new BuildingSaveData(building);
        }
    }
}
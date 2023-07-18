using JetBrains.Annotations;
using SaveLoad;
using UnityEngine;

namespace CoreLib.MyEntities
{
    [UsedImplicitly]
    [LoadOrder(-10)]
    public class DynamicEntityLoader : SaveFileLoader<DynamicEntitiesSaveData>
    {
        public override bool LoadData(DynamicEntitiesSaveData data)
        {
            if (data == null)
                return false;
            
            foreach (var entity in data)
            {
                if (EntityManager.Instance.debug)
                    Debug.Log($"Loading Entity: {entity.entityGUID} (Type:{entity.entityType}");
                EntityManager.Instance.LoadDynamicEntity(entity.entityGUID, entity.entityType);
            }
            return true;
        }
    }
    
    
    
    
}
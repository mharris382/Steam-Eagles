using System;
using UnityEngine;

namespace CoreLib.Entities.Factory
{
    public class InventoryFactory : EntityFactoryBase
    {
        public virtual Entity CreateEntity(GameObject entityTransform, string guid, string loadPath = null)
        {
            throw new System.NotImplementedException();
        }

        protected override void LoadFromJSON(Entity entity, string json)
        {
            throw new NotImplementedException();
        }

        protected override string SaveToJSON(Entity entity)
        {
            throw new NotImplementedException();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.INVENTORY;
        }
    }
}
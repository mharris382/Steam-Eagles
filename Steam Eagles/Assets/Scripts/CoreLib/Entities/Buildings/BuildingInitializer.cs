using Buildings;
using UnityEngine;

namespace CoreLib.Entities.Buildings
{
    [RequireComponent(typeof(Building))]
    public class BuildingInitializer : EntityInitializer
    {
        private Building _building;
        public Building Building => _building ? _building : _building = GetComponent<Building>();
        public override string GetEntityGUID() => Building.ID;

        public override EntityType GetEntityType() => EntityType.BUILDING;

        public override void OnEntityInitialized(Entity entity)
        {
            Building.IsFullyLoaded = true;
        }

        public override bool IsReadyToLoad()
        {
            
            return true;
        }
    }
}
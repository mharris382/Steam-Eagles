using Buildings;
using UnityEngine;
using Zenject;

namespace Weather.Storms
{
    public class BuildingStormSubject : EntityStormSubject
    {
        private readonly Building _building;

        public class Factory : PlaceholderFactory<Building, BuildingStormSubject> {}

        public BuildingStormSubject(Building building) : base(building.gameObject)
        {
            _building = building;
        }
        public override Bounds SubjectBounds => _building.WorldSpaceBounds;
        public override void OnStormAdded(Storm storm)
        {
            Debug.Log("Building Is now effected by storm", _building);
            //throw new NotImplementedException();
        }

        public override void OnStormRemoved(Storm storm)
        {
            Debug.Log("Building Is no longer effected by storm", _building);
        }
    }
}
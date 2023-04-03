using UnityEngine;

namespace Buildings.Appliances
{
    public abstract class BuildingAppliance : MonoBehaviour, IEntityID
    {
        private BuildingAppliances _building;
        public BuildingAppliances BuildingMechanisms => _building != null ? _building : _building = GetComponentInParent<BuildingAppliances>();
        public string GetEntityGUID()
        {
            return gameObject.name;
        }
    }
}
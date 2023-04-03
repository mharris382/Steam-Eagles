using UniRx;
using UnityEngine;

namespace Buildings.Appliances
{
    [RequireComponent(typeof(Building))]
    public class BuildingAppliances : BuildingSubsystem<BuildingAppliance>
    {
        



        public Transform applianceParent;


        public override void OnSubsystemEntityRegistered(BuildingAppliance entity)
        {
            
        }

        public override void OnSubsystemEntityUnregistered(BuildingAppliance entity)
        {
            
        }
    }
}
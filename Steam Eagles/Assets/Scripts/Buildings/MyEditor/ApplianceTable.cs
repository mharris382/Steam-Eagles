using System.Collections.Generic;
using Buildings.Appliances;

namespace Buildings.MyEditor
{
    public class ApplianceTable : SubSystemTable<BuildingAppliances, BuildingAppliance>
    {
        public ApplianceTable(Building building) : base(building)
        {
        }


        public override void InitializeWrappers(List<SubsystemEntityWrapper<BuildingAppliances, BuildingAppliance>> wrappers)
        {
            var buildingMechanisms = _building.GetComponentsInChildren<BuildingAppliance>();
            foreach (var mechanism in buildingMechanisms)
            {
                wrappers.Add(new SubsystemEntityWrapper<BuildingAppliances, BuildingAppliance>(this, mechanism));
            }
        }
    }
}
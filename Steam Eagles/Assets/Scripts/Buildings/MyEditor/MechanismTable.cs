using System.Collections.Generic;
using Buildings.Mechanisms;

namespace Buildings.MyEditor
{
    public class MechanismTable : SubSystemTable<BuildingMechanisms, BuildingMechanism>
    {
        public MechanismTable(Building building) : base(building)
        {
        }

        public override void InitializeWrappers(List<SubsystemEntityWrapper<BuildingMechanisms, BuildingMechanism>> wrappers)
        {
            var buildingMechanisms = _building.GetComponentsInChildren<BuildingMechanism>();
            foreach (var mechanism in buildingMechanisms)
            {
                wrappers.Add(new SubsystemEntityWrapper<BuildingMechanisms, BuildingMechanism>(this, mechanism));
            }
        }
    }
    
}
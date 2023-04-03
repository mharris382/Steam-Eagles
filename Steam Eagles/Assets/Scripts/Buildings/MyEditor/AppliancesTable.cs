using System.Collections.Generic;
using System.Linq;
using Buildings.Appliances;
using Sirenix.OdinInspector;

namespace Buildings.MyEditor
{
    public class AppliancesTable
    {
        private readonly Building _building;
        private readonly BuildingAppliances _mechanisms;


        [TableList(IsReadOnly = true, AlwaysExpanded = true), ValidateInput(nameof(ValidateAppliances))]
        private List<ApplianceWrapper> _appliances;

        public AppliancesTable(Building building)
        {
            _building = building;
            _mechanisms = building.GetComponent<BuildingAppliances>();
            if (_mechanisms == null)
            {
                _mechanisms = building.gameObject.AddComponent<BuildingAppliances>();
            }
            _appliances = _mechanisms.GetComponentsInChildren<BuildingAppliance>().Select(appliance => new ApplianceWrapper(this, appliance)).ToList();
        }
        
        bool ValidateAppliances(List<ApplianceWrapper> wrappers, ref string errMsg)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (var mechanism in wrappers)
            {
                if (names.Contains(mechanism._appliance.name))
                {
                    errMsg = $"Duplicate mechanism name: {mechanism._appliance.name}";
                    return false;
                }

                names.Add(mechanism._appliance.name);
            }

            return true;
        }
        
        public class ApplianceWrapper
        {
            internal readonly BuildingAppliance _appliance;
            private readonly AppliancesTable _table;

            public string name
            {
                get => _appliance.name;
                set
                {
                    _appliance.name = value;
                }
            }
            
            [ShowInInspector]
            public string ApplianceType
            {
                get => _appliance.GetType().Name;
            }

            public ApplianceWrapper(AppliancesTable table, BuildingAppliance appliance)
            {
                _table = table;
                _appliance = appliance;
            }
        }
    }
}
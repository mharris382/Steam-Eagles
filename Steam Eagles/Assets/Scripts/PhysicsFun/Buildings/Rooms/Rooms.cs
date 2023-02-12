using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    public class Rooms : MonoBehaviour
    {
        [OnValueChanged(nameof(OnBuildingAssigned))]
        [SerializeField, Required]
        private StructureState building;

       
        [SerializeField] private List<Room> rooms;


        public bool  HasBuilding => building != null;
        public StructureState Building => building;
        
        public IEnumerable<Room> AllRooms
        {
            get
            {
                return GetComponentsInChildren<Room>();
            }
        }
        
        
        void OnBuildingAssigned(StructureState structureState)
        {
            if (structureState == null) return;
            var building = structureState.GetComponent<Building>();
            if (building == null)
            {
                Debug.LogWarning("StructureState does not have a Building component", structureState);
                return;
            }
            name = building.buildingName + " Rooms";
        }
    }
}

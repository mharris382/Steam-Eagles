using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
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

        public void UpdateRoomsList()
        {
            rooms = new List<Room>(GetComponentsInChildren<Room>());
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

        public Room GetRoomAtWS(Vector3 position)
        {
            var localPosition = Building.transform.InverseTransformPoint(position);
            foreach (var room in rooms)
            {
                localPosition.z = room.roomBounds.center.z;
                if (room.Bounds.Contains(localPosition))
                {
                    return room;
                }
            }
            return null;
        }

        [Button, ShowIf(nameof(HasBuilding))]
        public void SnapAllBoundsToGrid()
        {
            if (!HasBuilding) return;
            var grid = Building.GetComponent<Grid>();
            foreach (var room in AllRooms)
            {
                var bounds = room.roomBounds;
                var min = bounds.min;
                var max = bounds.max;
                min =  grid.CellToLocal(grid.LocalToCell(min));
                max = grid.CellToLocal(grid.LocalToCell(max));
                bounds.SetMinMax(min, max);
                room.roomBounds = bounds;
            }
        }
    }


    
}

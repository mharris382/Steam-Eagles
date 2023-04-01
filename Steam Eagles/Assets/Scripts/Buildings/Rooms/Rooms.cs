using System;
using System.Collections.Generic;
using PhysicsFun.Buildings;
using PhysicsFun.Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Rooms
{
    public class Rooms : MonoBehaviour
    {
        [OnValueChanged(nameof(OnBuildingAssigned))]
        [SerializeField, Required]
        private StructureState building;

        [Range(0, 1)]
        public float faceOpacity = 0.2f;
        [Range(0, 1)]
        public float outlineOpacity = 1;
        
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
                localPosition.z = room.RoomBounds.center.z;
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
                var bounds = room.RoomBounds;
                var min = bounds.min;
                var max = bounds.max;
                min =  grid.CellToLocal(grid.LocalToCell(min));
                max = grid.CellToLocal(grid.LocalToCell(max));
                bounds.SetMinMax(min, max);
                room.RoomBounds = bounds;
            }
        }
        
        [TableList]
        [SerializeField] private List<RoomGroupings> roomGroupings;

        public IEnumerable<RoomGroupings> GetGroups(bool rebuildGroups = false)
        {
            if (rebuildGroups)
            {
                Dictionary<Color, List<Room>> groups = new Dictionary<Color, List<Room>>();
                foreach (var roomGrouping in roomGroupings)
                {
                    roomGrouping.roomsInGroup.RemoveAll(t => t == null);
                    foreach (var room in roomGrouping.roomsInGroup)
                    {
                        if (room.roomColor != roomGrouping.groupColor)
                        {
                            roomGrouping.roomsInGroup.Remove(room);
                        }
                    }
                    groups.Add(roomGrouping.groupColor, roomGrouping.roomsInGroup);
                }
                foreach (var room in AllRooms)
                {
                    if (groups.ContainsKey(room.roomColor))
                    {
                        if(groups[room.roomColor].Contains(room) == false)
                            groups[room.roomColor].Add(room);
                    }
                    else
                        groups.Add(room.roomColor, new List<Room> { room });
                }
                roomGroupings = new List<RoomGroupings>();
               
            }
            foreach (var roomGrouping in roomGroupings)
            {
                yield return roomGrouping;
            }
        }


        void RebuildGroups()
        {
            
        }

        [Serializable]
        public class RoomGroupings
        {
            [OnValueChanged(nameof(UpdateColors))]
            public Color groupColor;
            public List<Room> roomsInGroup = new List<Room>();

            void UpdateColors()
            {
                roomsInGroup.RemoveAll(t => t == null);
                foreach (var room in roomsInGroup)
                {
                    room.roomColor = groupColor;
                }
            }
        }
    }
}

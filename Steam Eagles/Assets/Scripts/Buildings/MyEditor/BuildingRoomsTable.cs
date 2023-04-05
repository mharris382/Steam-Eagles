using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class BuildingRoomsTable
    {
        [ShowInInspector]
        public string BuildingName
        {
            get => _building.buildingName;
            set => _building.buildingName = value;
        }
        
        private readonly Building _building;
        private readonly StructureState _structure;

        [HorizontalGroup("Rooms")]
        [ShowInInspector, ReadOnly, ValidateInput(nameof(ValidateRooms))]
        private Rooms.Rooms _rooms;

        [Button, HorizontalGroup("Rooms")]
        void SelectRooms() => Selection.activeGameObject = _rooms.gameObject;

        [ShowInInspector, TableList(IsReadOnly = true, AlwaysExpanded = true)]
        private List<RoomWrapper> _roomWrappers;
        
        [Button, HideIf(nameof(HasRooms))]
        void CreateRooms()
        {
            var roomsGO = new GameObject($"[{_building.buildingName} ROOMS]");
            roomsGO.transform.SetParent(_building.transform);
            var rooms = roomsGO.AddComponent<Rooms.Rooms>();
            rooms.Building = _building.GetComponent<StructureState>();
        }
        
        public BuildingRoomsTable(Building building)
        {
            this._building = building;
            if (!_building.gameObject.TryGetComponent(out _structure))
            {
                _structure = _building.gameObject.AddComponent<StructureState>();
            }
            _rooms = building.GetComponentInChildren<Rooms.Rooms>();
            _roomWrappers = new List<RoomWrapper>(_rooms.AllRooms.Select(r => new RoomWrapper(r, this)));
        }

        bool HasRooms() => _rooms != null;
        
        bool ValidateRooms(Rooms.Rooms rooms, ref string errMsg)
        {
            if (rooms == null)
            {
                errMsg = "There are no rooms in this building";
                return false;
            }

            if (!rooms.HasBuilding)
            {
                errMsg = "Rooms are not assigned to this building";
                return false;
            }
            
            if(rooms.HasBuilding && rooms.Building != _structure)
            {
                errMsg = "Rooms are assigned to a different building";
                return false;
            }

            return true;
        }
        
        
        public class RoomWrapper
        {
            [InlineButton(nameof(SelectRoom))]
            [TableColumnWidth(120)]
            [ShowInInspector]
            public string RoomName
            {
                get => _room.name;
                set => _room.name = value;
            }
            [HideLabel]
            
            
            [ShowInInspector, ReadOnly]
            private readonly Room _room;

            public Vector2 RoomSize => _room.Bounds.size;

            public void SelectRoom() => Selection.activeGameObject = _room.gameObject;

            public RoomWrapper(Room room, BuildingRoomsTable roomsTable)
            {
                this._room = room;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buildings.Rooms;
using CoreLib;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Buildings.MyEditor
{
    public class RoomSizerTool
    {
        public List<Room> Rooms = new List<Room>();
        private Color SelectionColor => Color.Lerp(Color.green, Color.white, 0.5f);

        [ShowInInspector,  BoxGroup("Output")]
        public OutputRoomSizes outputRoomSizes;
        
        [LabelText("Rooms")]
        [TableList(IsReadOnly = true, AlwaysExpanded = true),ShowInInspector]
        public List<RoomWrapper> roomSelected = new List<RoomWrapper>();
        


        [ReadOnly, BoxGroup("Bounds"), PropertyOrder(-10)]
        public Bounds totalBounds;
        
        [BoxGroup("Bounds"), PropertyOrder(-9),ShowInInspector]
        private bool showCalculatedBounds = true;
        
        [BoxGroup("Bounds"), PropertyOrder(-8),ShowInInspector, ShowIf(nameof(showCalculatedBounds))]
        private float multiplier = 1;

        [BoxGroup("Bounds"), PropertyOrder(-8),ShowInInspector, ShowIf(nameof(showCalculatedBounds))]
        private BoundsCalculation calculation;

        public RoomSizerTool(Building building, IEnumerable<Room> allRooms)
        {
            roomSelected= allRooms.Select(r => new RoomWrapper(r, this, false)).ToList();
            calculation = new BoundsCalculation(this);
            outputRoomSizes = new OutputRoomSizes(building,allRooms);
        }

        protected void SelectRoom(RoomWrapper room)
        {
            if (roomSelected.Contains(room))
            {
                room._isSelected = true;
            }
            UpdateBounds();
            roomSelected.Sort();
        }

        protected void DeselectRoom(RoomWrapper room)
        {
            if (roomSelected.Contains(room))
            {
                room._isSelected = false;
            }
            UpdateBounds();
            roomSelected.Sort();
        }

        public void UpdateBounds()
        {
            var bounds = new Bounds();
            foreach (var room in roomSelected.Where(t => t._isSelected).Select(t => t.Room))
            {
                var roomBounds = room.Bounds;
                if (bounds.size == Vector3.zero)
                {
                    bounds = roomBounds;
                }
                else
                {
                    bounds.Encapsulate(roomBounds);
                }
            }
            this.totalBounds = bounds;
        }

        public class RoomWrapper : IComparable<RoomWrapper>
        {
            [HideInInspector] public Room Room;
            [HideInInspector] RoomSizerTool _tool;
            
            [HideInInspector] public bool _isSelected;

            
            private Color guiColor => _isSelected ? Color.green : Room.roomColor;
            
            
            [GUIColor(nameof(guiColor))]
            [ReadOnly, TableColumnWidth(150)]
            [ShowInInspector]
            public string name => Room.name;
            
            
            public Bounds Bounds => Room.Bounds;

            [ReadOnly, TableColumnWidth(50)]
            public float squareFeet => (Bounds.size.x * Bounds.size.y);
            public RoomWrapper(Room room, RoomSizerTool tool, bool isSelected)
            {
                Room = room;
                _tool = tool;
                _isSelected = isSelected;
            }

            [TableColumnWidth(100, false)]
            [ButtonGroup("Buttons")]
            [Button("+"), HideIf(nameof(_isSelected))]
            public void SelectRoom()
            {
                _tool.SelectRoom(this);
                _tool.UpdateBounds();
            }
            
            [ButtonGroup("Buttons")]
            [Button("-"), ShowIf(nameof(_isSelected))]
            public void DeselectRoom()
            {
                _tool.DeselectRoom(this);
                _tool.UpdateBounds();
            }

            public int CompareTo(RoomWrapper other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                var res = other._isSelected.CompareTo(_isSelected);


                if (res != 0)
                {
                    return res;
                }
                return string.Compare(name, other.name, StringComparison.Ordinal);
            }
        }

        public class BoundsCalculation
        {
            private readonly RoomSizerTool _sizerTool;
            bool isCalculating => _sizerTool.showCalculatedBounds && _sizerTool.multiplier > 0 && _sizerTool.roomSelected.Count > 0;
            public BoundsCalculation(RoomSizerTool sizerTool)
            {
                _sizerTool = sizerTool;
            }

            [ShowInInspector, ShowIf(nameof(isCalculating))]
            public Bounds TotalCacluatedBounds
            {
                get
                {
                    _sizerTool.UpdateBounds();
                    var originalB = _sizerTool.totalBounds;
                    var originalSize = originalB.size;
                    var originalCenter = originalB.center;
                    var newSize = originalSize * _sizerTool.multiplier;
                    return new Bounds(originalCenter,newSize);
                }
            }
        }

        public class RoomSizeWrapper
        {
            public Room Room { get; }
            private readonly RoomSizerTool _sizerTool;

            
            
            public RoomSizeWrapper(Room room, RoomSizerTool sizerTool)
            {
                Room = room;
                _sizerTool = sizerTool;
            }
        }
        
        
        [InlineProperty]
        public class OutputRoomSizes
        {
            private readonly Building _b;
            public float multiplier = 64;


            [FolderPath(AbsolutePath = true)]
            public string outputPath = "";

            [HideInInspector]
            private Room[] rooms;
            public OutputRoomSizes(Building b, IEnumerable<Room> allRooms)
            {
                _b = b;
                rooms = allRooms.ToArray();
            }

            [Button]
            public void OutputSizes()
            {
                var buildingName = _b.name;
                var path = $"{outputPath}/{buildingName}.csv";
           
                if(Directory.Exists(outputPath) == false)
                    Directory.CreateDirectory(outputPath);
                
                if(File.Exists(path))
                    File.Delete(path);
                using (FileStream fs = File.OpenWrite(path))
                {
                    StreamWriter sw = new StreamWriter(fs);
                    sw.Write($"Building, Room, Width(u), Height(u), Width(px), Height(px), Width(px^2), Height(px^2)\n");
                    foreach (var room in rooms)
                    {
                        var bounds = room.Bounds.size;
                        var size = new Vector2(bounds.x * multiplier, bounds.y * multiplier);
                        sw.Write($"{buildingName}, {room.name}, {bounds.x}, {bounds.y}, {size.x}, {size.y}, {Mathf.NextPowerOfTwo(Mathf.CeilToInt(size.x))}, {Mathf.NextPowerOfTwo(Mathf.CeilToInt(size.y))}\n");
                    }
                }
                Debug.Log($"Output Path:\n{path.Bolded().InItalics()}");
            }
        }
    }
}
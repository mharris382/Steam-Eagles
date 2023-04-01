#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Buildings.Rooms.MyEditor
{
    public partial class RoomsEditor
    {
        public class RoomSizeHandle : BoxBoundsHandle
        {
            private readonly Transform _building;
            private readonly Room _room;
            public bool IsValid { get; }
            public RoomSizeHandle(global::Buildings.Rooms.Rooms buildingRooms, Room room)
            {
                IsValid = buildingRooms.HasBuilding;
                if(!IsValid) return;
                this._building = buildingRooms.Building.transform;
                this._room = room;
                axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
                center = _building.TransformPoint(room.RoomBounds.center);
                size = _building.TransformVector(room.RoomBounds.size);
                handleColor = room.roomColor;
                wireframeColor = room.roomColor;
                midpointHandleDrawFunction = Handles.DotHandleCap;
                midpointHandleSizeFunction = PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction;
            }
            
            public Vector3 GetLocalCenter()
            {
                return _building.InverseTransformPoint(center);
            }
            
            public Vector3 GetLocalSize()
            {
                return _building.InverseTransformVector(size);
            }
        }
    }
}
#endif
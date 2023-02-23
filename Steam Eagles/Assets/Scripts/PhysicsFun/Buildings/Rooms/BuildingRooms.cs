using System.Collections.Generic;
using Buildings;
using Sirenix.OdinInspector;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif


namespace PhysicsFun.Buildings.Rooms
{
    public class BuildingRooms : MonoBehaviour
    {
        [SerializeField, Required] private StructureState building;
        [SerializeField] private List<Room> rooms;
        
        

        public bool  HasBuilding => building != null;
        public StructureState Building => building;
        
        public IEnumerable<Room> Rooms
        {
            get
            {
                return rooms;
            }
        }
    }

#if UNITY_EDITOR
    namespace MyEditor
    {
        [CustomEditor(typeof(BuildingRooms))]
        public class BuildingRoomsEditor : OdinEditor
        {
            private void OnSceneGUI()
            {
                BuildingRooms buildingRooms = (BuildingRooms) target;
                if (!buildingRooms.HasBuilding) return;
                
                foreach (Room room in buildingRooms.Rooms)
                {
                   
                    
                }
            }


            private void DrawRoom(BuildingRooms building, Room room)
            {
                Handles.color = room.roomColor;
                var bounds = room.roomBounds;
                
                var transform = building.Building.transform;

                var handle = new RoomSizeHandle(building, room);
                
                EditorGUI.BeginChangeCheck();
                if(handle.IsValid)
                    handle.DrawHandle();
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(building, "Change Room Size");
                    Bounds newBounds = new Bounds();
                    newBounds.center = transform.InverseTransformPoint(handle.center);
                    newBounds.size = transform.InverseTransformVector(handle.size);
                    room.roomBounds = newBounds;
                }

            }
            
            public class RoomSizeHandle : BoxBoundsHandle
            {
                private readonly Transform _building;
                private readonly Room _room;
                public bool IsValid { get; }
                public RoomSizeHandle(BuildingRooms buildingRooms, Room room)
                {
                    IsValid = buildingRooms.HasBuilding;
                    if(!IsValid) return;
                    this._building = buildingRooms.Building.transform;
                    this._room = room;
                    axes = PrimitiveBoundsHandle.Axes.X | PrimitiveBoundsHandle.Axes.Y;
                    handleColor = room.roomColor;
                    wireframeColor = room.roomColor;
                    midpointHandleDrawFunction = Handles.DotHandleCap;
                    midpointHandleSizeFunction = PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction;
                }
                
            }
        }
        
        
    }
    #endif
}
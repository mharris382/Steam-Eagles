#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    public class RoomsEditorWindow : OdinEditorWindow
    {
        
        public static bool IsEditorOpen(Rooms rooms)
        {
            
            var windows = Resources.FindObjectsOfTypeAll<RoomsEditorWindow>();
            foreach (var window in windows)
            {
                if (window.targetRooms == rooms)
                {
                    return true;
                }
            }
            return false;
        }
        
        public static void OpenWindow(Rooms rooms)
        {
            var window = GetWindow<RoomsEditorWindow>();
            window.targetRooms = rooms;
            window.Show();
        }
        
        
        
        [SerializeField, Required] public Rooms targetRooms;

        protected override IEnumerable<object> GetTargets()
        {
            if (targetRooms == null)
            {
                yield break;
            }
            yield return new RoomsEditorWrapper(targetRooms.AllRooms);
        }

        
        public class RoomsEditorWrapper
        {
            private List<Room> rooms;
            [ShowInInspector, TableList]
            public List<RoomEditorWrapper> roomEditorWrappers;
            public int Count => rooms.Count;
            public RoomsEditorWrapper(IEnumerable<Room> rooms)
            {
                this.rooms = new List<Room>(rooms);
                roomEditorWrappers = new List<RoomEditorWrapper>();
                foreach(var room in rooms)
                {
                    roomEditorWrappers.Add(new RoomEditorWrapper(room));
                }
            }
        }
        
        public class RoomEditorWrapper
        {
            public RoomEditorWrapper(Room room)
            {
                this.room = room;
            }

            [TableColumnWidth(150)]
            [ShowInInspector, LabelWidth(50), LabelText("Name")]
            public string RoomName
            {
                get => room.name;
                set => room.name = value;
            }

            [ShowInInspector, HideLabel, TableColumnWidth(50)]
            public Color RoomGUIColor
            {
                get => room.roomColor;
                set => room.roomColor = value;
            }

            [ShowInInspector, LabelWidth(50), LabelText("Camera"), TableColumnWidth(100)]
            public GameObject RoomCamera
            {
                get => room.roomCamera;
                set => room.roomCamera = value;
            }
            
            [HideInInspector]
            public Room room;

            
        }
    }
}
#endif
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace PhysicsFun.Buildings.Rooms
{
    [CustomEditor(typeof(Room))]
    public class RoomEditor : OdinEditor
    {
        private BoxBoundsHandle m_BoundsHandle = new BoxBoundsHandle();
        private static Room m_selectedRoomCamera;
        private bool m_isCameraSelected;

        bool IsSelectedRoomCamera(Room room)
        {
            return m_selectedRoomCamera == target;
        }
        public override void OnInspectorGUI()
        {
            
            Room room = (Room) target;
            string errorMsg = "";
            if (!IsRoomValid(ref errorMsg))
            {
                if (m_selectedRoomCamera == room) ChangeSelectedRoomCamera(null);
                EditorGUILayout.HelpBox(errorMsg, MessageType.Error);
                base.OnInspectorGUI();
                return;
            }

            if (IsSelectedRoomCamera(room) && GUILayout.Button("Disable Camera Preview"))
            {
                m_isCameraSelected = false;
                ChangeSelectedRoomCamera(null);
            }
            else if(!IsSelectedRoomCamera(room) && GUILayout.Button("Enable Camera Preview"))
            {
                ChangeSelectedRoomCamera(room);
                m_isCameraSelected = true;
            }
            base.OnInspectorGUI();

        }

        protected override void OnEnable()
        {
            Room room = (Room) target;
            string errorMsg = "";
            m_isCameraSelected = false;
            if (IsRoomValid(ref errorMsg))
            {
                ChangeSelectedRoomCamera(room);
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            Room room = (Room) target;
            if(m_selectedRoomCamera == room && !m_isCameraSelected) 
                ChangeSelectedRoomCamera(null);
            base.OnDisable();
        }

        private void ChangeSelectedRoomCamera(Room o)
        {
            if (m_selectedRoomCamera != null && m_selectedRoomCamera.roomCamera != null)
            {
                m_selectedRoomCamera.roomCamera.gameObject.SetActive(false);
            }
            m_selectedRoomCamera = o;
            if (m_selectedRoomCamera != null  && m_selectedRoomCamera.roomCamera != null)
            {
                m_selectedRoomCamera.roomCamera.gameObject.SetActive(true);
            }
        }

        bool IsRoomValid(ref string msg)
        {
            Room room = (Room) target;
            
            if (room == null) return false;
            Rooms rooms = room.GetComponentInParent<Rooms>();
            if (rooms == null)
            {
                msg = "Room is not a child of a Rooms object";
                return false;
            }

            if (!rooms.HasBuilding)
            {
                msg = "Rooms object does not have a building";
                return false;
            }

            if (!room.isDynamic) return true;
            if (room.isDynamic && room.dynamicBody == null)
            {
                msg = "Room is dynamic but does not have a dynamic body";
                return false;
            }

            return true;
        }

        public void OnSceneGUI()
        {
            Room room = (Room) target;
            if (room == null) return;
            Rooms rooms = room.GetComponentInParent<Rooms>();
            if (rooms == null) return;
            if (!rooms.HasBuilding) return;
            var transform = rooms.Building.transform;
            if (room.isDynamic)
            {
                if (room.dynamicBody == null) return;
                transform = room.dynamicBody.transform;
            }
            var bounds = room.Bounds;
            var center = transform.TransformPoint(room.roomBounds.center);
            
            DrawRoomArea(center, bounds, room);
            DrawRoomBounds(center, room, transform);
        }

        private void DrawRoomBounds(Vector3 center, Room room, Transform transform)
        {
            m_BoundsHandle.center = center;
            m_BoundsHandle.size = room.roomBounds.size;

            m_BoundsHandle.midpointHandleSizeFunction = MidpointHandleSizeFunction;
            m_BoundsHandle.handleColor = room.roomColor;
            m_BoundsHandle.wireframeColor = room.roomColor;
            Bounds newBounds = new Bounds();
            EditorGUI.BeginChangeCheck();
            m_BoundsHandle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(room, "Changed Room Size");
                newBounds.center = transform.InverseTransformPoint(m_BoundsHandle.center);
                newBounds.size = (m_BoundsHandle.size);
                room.roomBounds = newBounds;
            }
        }

        public static void DrawRoomArea(Vector3 center, Bounds bounds, Room room)
        {
            var rect = Rect.MinMaxRect(center.x - bounds.extents.x, center.y - bounds.extents.y, center.x + bounds.extents.x,
                center.y + bounds.extents.y);
            var color = room.roomColor;
            color.a = 0.25f;
            Handles.DrawSolidRectangleWithOutline(rect, color, color);
        }
        public static void DrawRoomArea(Rooms rooms, Room room)
        {
            var transform = rooms.Building.transform;
            if (room.isDynamic)
            {
                if (room.dynamicBody == null) return;
                transform = room.dynamicBody.transform;
            }
            var center = transform.TransformPoint(room.roomBounds.center);
            var bounds = room.Bounds;
            var rect = Rect.MinMaxRect(center.x - bounds.extents.x, center.y - bounds.extents.y, center.x + bounds.extents.x,
                center.y + bounds.extents.y);
            var color = room.roomColor;
            color.a = 0.25f;
            Handles.DrawSolidRectangleWithOutline(rect, color, color);
        }
        static float MidpointHandleSizeFunction(Vector3 position)
        {
            return PrimitiveBoundsHandle.DefaultMidpointHandleSizeFunction(position) * 2f;
        }
    }
}
#endif
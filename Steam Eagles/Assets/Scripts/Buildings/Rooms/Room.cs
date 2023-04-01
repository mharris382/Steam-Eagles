using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Buildings.Rooms
{
    [Serializable]
    public class Room : MonoBehaviour
    {
        public GameObject roomCamera;
        public Color roomColor = Color.red;
        public Bounds roomBounds = new Bounds(Vector3.zero, Vector3.one);

        [ToggleGroup(nameof(isDynamic))] public bool isDynamic;
        [ToggleGroup(nameof(isDynamic))] public Rigidbody2D dynamicBody;


        public List<Room> connectedRooms = new List<Room>();

        public Bounds RoomBounds
        {
            get => roomBounds;
            set
            {
                if (value.size.z == 0)
                {
                    var newMin = value.min;
                    var newMax = value.max;
                    newMin.z = -0.5f;
                    newMax.z = 0.5f;
                    value.SetMinMax(newMin, newMax);
                }
                roomBounds = value;
            }
        }
        
        public Bounds Bounds
        {
            get
            {
                if (RoomBounds.size.z == 0)
                {
                    var newMin = RoomBounds.min;
                    var newMax = RoomBounds.max;
                    newMin.z = -0.5f;
                    newMax.z = 0.5f;
                    RoomBounds.SetMinMax(newMin, newMax);
                }
                return RoomBounds;
            }
            set => RoomBounds = value;
        }
        
        private Rooms _rooms;

        public Rooms Rooms
        {
            get => _rooms != null ? _rooms : _rooms = GetComponentInParent<Rooms>();
        }

        public Transform BuildingTransform
        {
            get
            {
                if (Rooms == null) return null;
                return Rooms.Building.transform;
            }
        }

        public Vector3 WorldCenter => BuildingTransform.TransformPoint(RoomBounds.center);
        
        public void AddConnectedRoom(Room room)
        {
            if (connectedRooms.Contains(room)) return;
            connectedRooms.Add(room);
        }

        public void RemoveConnectedRoom(Room room)
        {
            if (!connectedRooms.Contains(room)) return;
            connectedRooms.Remove(room);
        }

        public bool ContainsWorldPosition(Vector2 wsPosition)
        {
            if (BuildingTransform == null)
            {
                Debug.LogError(Rooms != null ? $"Building transform is for {Rooms.name} null" : $"{name} Rooms is null", Rooms != null ? Rooms : this);
                return false;
            }
            var lsPosition = BuildingTransform.InverseTransformPoint(wsPosition);
            return ContainsLocalPosition(lsPosition);
            lsPosition.z = RoomBounds.center.z;
            return RoomBounds.Contains(lsPosition);
        }
        
        public bool ContainsLocalPosition(Vector3 lsPosition)
        {
            lsPosition.z = RoomBounds.center.z;
            return RoomBounds.Contains(lsPosition);
        }

        public bool ContainsLocalPosition(Vector2 lsPosition) => ContainsLocalPosition(new Vector3(lsPosition.x, lsPosition.y, RoomBounds.center.z));

        public void SetCameraActive(bool active)
        {
            if(roomCamera != null)
                roomCamera.SetActive(active);
        }
    }
}
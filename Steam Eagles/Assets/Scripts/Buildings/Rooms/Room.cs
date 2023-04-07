 using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Sirenix.OdinInspector;
 using Sirenix.OdinInspector.Editor;
 using UnityEditor;
 using UnityEngine;
 using UnityEngine.Tilemaps;

 namespace Buildings.Rooms
{
    [Serializable]
    public class Room : MonoBehaviour
    {
        public GameObject roomCamera;

        
        [EnumToggleButtons]
        public AccessLevel accessLevel = AccessLevel.EVERYONE;
        
        
        public Color roomColor = Color.red;
        public Bounds roomBounds = new Bounds(Vector3.zero, Vector3.one);

        [ToggleGroup(nameof(isDynamic))] public bool isDynamic;
        [ToggleGroup(nameof(isDynamic))] public Rigidbody2D dynamicBody;

        public bool IsDamageable => ((int)accessLevel & (int)AccessLevel.ENGINEERS) != 0;


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

        public Bounds WorldSpaceBounds
        {
            get
            {
                var lsBounds = Bounds;
                var wsMin = BuildingTransform.TransformPoint(lsBounds.min);
                var wsMax = BuildingTransform.TransformPoint(lsBounds.max);
                wsMin.z = -0.5f;
                wsMax.z = 0.5f;
                var wsBounds = new Bounds(wsMin, Vector3.zero);
                Bounds.SetMinMax(wsMin, wsMax);
                return wsBounds;
            }
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


        [Button]
        void OpenTester()
        {
            OdinEditorWindow.InspectObject(new RoomTester(this));
        }
        
        public void Fill(TileBase tileBase, Tilemap target)
        {
            
            
            var center = Bounds.center;
            var centerWS = target.transform.TransformPoint(center);
            var extent = target.transform.TransformVector(Bounds.extents);
            var newRect = new Rect(centerWS, extent*2);
            var min =newRect.min;
            var max =newRect.max;
            for (int x = Mathf.RoundToInt( min.x); x < max.x; x++)
            {
                for (int y = Mathf.RoundToInt(min.y); y < max.y; y++)
                {
                    var pos = new Vector3(x, y)-extent;
                    target.SetTile(target.WorldToCell(pos), tileBase);
                }
            }
        }
    }

    public class RoomTester
    {
        private readonly Room _room;
        private readonly Building _b;

        [ShowInInspector]
        public Bounds RoomBounds
        {
            get => _room.Bounds;
        }

        public Building Building
        {
            get => _b;
        }

        
        public TileBase tile;

        private bool hasTile => tile != null;

        [Button, ShowIf(nameof(hasTile))]
        void Fill()
        {
            var tm = Building.GetTilemap(BuildingLayers.WALL).Tilemap;
            Undo.RecordObject(tm, "Fill room");
            _room.Fill(tile, tm);
        }

        public RoomTester(Room room)
        {
            _room = room;
            _b =  _room.BuildingTransform.GetComponent<Building>();
        }
    }
}
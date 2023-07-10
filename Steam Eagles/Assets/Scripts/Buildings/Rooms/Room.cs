﻿ using System;
using System.Collections.Generic;
using System.Linq;
 using Buildings.Damage;
 using CoreLib;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
 using Sirenix.OdinInspector.Editor;
 using UnityEditor;
 #endif
 using UnityEngine;
 using UnityEngine.Events;
 using UnityEngine.Tilemaps;
 using Zenject;

 namespace Buildings.Rooms
{
    public enum BuildLevel
    {
        FULL,
        NONE
    }

    [Serializable]
    public class Room : MonoBehaviour
    {
        public interface IDamageProviderFactory : IFactory<Room,IDamageOptionProvider>{ }
        
        [Serializable] public class Events
        {
            public CharacterEvents characterEvents = new CharacterEvents();
            public PlayerEvents playerEvents = new PlayerEvents();
            [Serializable] public class CharacterEvents
            {
                public UnityEvent<GameObject> onCharacterEntered = new UnityEvent<GameObject>();
                public UnityEvent<GameObject> onCharacterExited = new UnityEvent<GameObject>();
            }
            [Serializable] public class PlayerEvents
            {
                public UnityEvent<int> onPlayerEntered = new UnityEvent<int>();
                public UnityEvent<int> onPlayerExited = new UnityEvent<int>();
            }
        }
        
        [SerializeField] private Events events = new Events();
        public GameObject roomCamera;
        public  RoomCameraConfig roomCameraConfig;
        [EnumToggleButtons]
        public AccessLevel accessLevel = AccessLevel.EVERYONE;
        [EnumPaging]
        public BuildLevel buildLevel = BuildLevel.FULL;
        
        public Color roomColor = Color.red;
        public Bounds roomBounds = new Bounds(Vector3.zero, Vector3.one);

        [ToggleGroup(nameof(isDynamic))] public bool isDynamic;
        [ToggleGroup(nameof(isDynamic))] public Rigidbody2D dynamicBody;

        public bool IsDamageable => (((int)accessLevel & (int)AccessLevel.ENGINEERS) != 0) && 
                                    ((int)accessLevel & (int)AccessLevel.OFFICERS) == 0 && 
                                    ((int)accessLevel & (int)AccessLevel.PILOTS) == 0 && 
                                    ((int)accessLevel & (int)AccessLevel.PASSENGERS) == 0;

        public bool debugGetCells;
        public GridLayout targetGrid;
        private IDamageOptionProvider _damageProvider;
        public IDamageOptionProvider DamageOption => _damageProvider;

        public List<Room> connectedRooms = new List<Room>();

        private Building _building;
        public Building Building => _building ? _building : _building = GetComponentInParent<Building>();


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
        
        public RectInt RoomRect
        {
            get
            {
                var min = RoomBounds.min;
                var max = RoomBounds.max;
                var minInt = new Vector2Int(Mathf.FloorToInt(min.x), Mathf.FloorToInt(min.y));
                var maxInt = new Vector2Int(Mathf.CeilToInt(max.x), Mathf.CeilToInt(max.y));
                return new RectInt(minInt, maxInt - minInt);
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
                var solidBounds = Building.Map.GetCellsForRoom(this, BuildingLayers.SOLID);
                var solidMin = solidBounds.min;
                var solidMax = solidBounds.max;
                var wsMin = Building.Map.CellToWorld(solidMin, BuildingLayers.SOLID);
                var wsMax = Building.Map.CellToWorld(solidMax, BuildingLayers.SOLID);
                var extent = wsMax - wsMin;
                var center = wsMax - (extent / 2f);
                wsMin.z = -0.5f;
                wsMax.z = 0.5f;
                var wsBounds = new Bounds(center, extent);
                return wsBounds;
            }
        }
        public Bounds LocalSpaceBounds
        {
            get
            {
                var solidBounds = Building.Map.GetCellsForRoom(this, BuildingLayers.SOLID);
                var solidMin = solidBounds.min;
                var solidMax = solidBounds.max;
                var wsMin = Building.Map.CellToLocal(solidMin, BuildingLayers.SOLID);
                var wsMax = Building.Map.CellToLocal(solidMax, BuildingLayers.SOLID);
                var extent = wsMax - wsMin;
                var center = wsMax - (extent / 2f);
                wsMin.z = -0.5f;
                wsMax.z = 0.5f;
                var wsBounds = new Bounds(center, extent);
                return wsBounds;
            }
        }
        public Vector2 Size => WorldSpaceBounds.size;
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


        #region [Injection Methods]
        //
        // [Inject]
        // public void InjectDamageProvider(IDamageProviderFactory factory)
        // {
        //     _damageProvider = factory.Create(this);
        // }

        #endregion


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
#if UNITY_EDITOR
            OdinEditorWindow.InspectObject(new RoomTester(this));
#endif
        }

        public void Fill(TileBase tileBase, Tilemap target)
        {
            foreach (var cell in GetCells(target))
            {
                target.SetTile(cell, tileBase);
            }
        }

        public IEnumerable<Vector3Int> GetCells( Tilemap target)
        {
            var center = Bounds.center;
            var centerWS = target.transform.TransformPoint(center);
            var extent = target.transform.TransformVector(Bounds.extents);
            var newRect = new Rect(centerWS, extent*2);
            var min =newRect.min;
            var max =newRect.max;
            for (int x = Mathf.RoundToInt( min.x)+1; x < max.x-1; x++)
            {
                for (int y = Mathf.RoundToInt(min.y)+1; y < max.y-1; y++)
                {
                    var pos = new Vector3(x, y)-extent;
                    yield return target.WorldToCell(pos);
                }
            }
        }

        public BoundsInt GetCells(GridLayout gridLayout)
        {
            var cellMin = gridLayout.LocalToCell(Bounds.min);
            var cellMax = gridLayout.LocalToCell(Bounds.max);
            cellMax.z = cellMin.z = 0;
            var size = cellMax - cellMin;
            return new BoundsInt(cellMin, size);
        }

        public BoundsInt GetBounds(BuildingLayers layers)
        {
            return Building.Map.GetCellsForRoom(this, layers);
        }
        public RectInt GetCellRect(Tilemap target)
        {
            var center = Bounds.center;
            var centerWS = target.transform.TransformPoint(center);
            var extent = target.transform.TransformVector(Bounds.extents);
            var newRect = new Rect(centerWS, extent*2);
            var min =newRect.min;
            var max =newRect.max;
            var rectInt = new RectInt();
            var minCell =  Vector3Int.one;
            var maxCell = Vector3Int.zero;
            minCell.z = 0;
            for (int x = Mathf.RoundToInt( min.x)+1; x < max.x-1; x++)
            {
                for (int y = Mathf.RoundToInt(min.y)+1; y < max.y-1; y++)
                {
                    var pos = new Vector3(x, y)-extent;
                    var cell = target.WorldToCell(pos);
                    if(pos.x < minCell.x)
                        minCell.x = cell.x;
                    if(pos.y < minCell.y)
                        minCell.y = cell.y;
                    if(pos.x > maxCell.x)
                        maxCell.x = cell.x;
                    if(pos.y > maxCell.y)
                        maxCell.y = cell.y;
                }
            }
            return new RectInt(minCell.x, minCell.y, maxCell.x, maxCell.y);
        }

        private void OnDrawGizmosSelected()
        {
            if(!debugGetCells || targetGrid == null) return;
            var cells = GetCells(targetGrid);
            Gizmos.color = this.roomColor;
            for (int x = cells.xMin; x < cells.xMax; x++)
            {
                for (int y = cells.yMin; y < cells.yMax; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    var worldCenter = targetGrid.CellToWorld(cell);
                    var cellSize = targetGrid.cellSize;
                    worldCenter += cellSize/2f;
                    Gizmos.DrawCube(worldCenter, cellSize * 0.95f);
                }
            }
           
        }



        public void NotifyPlayerCharacterEnteredRoom(PCInstance pcInstance)
        {
            
        }
        
        public void NotifyPlayerCharacterExitedRoom(PCInstance pcInstance)
        {
            
        }

        /// <summary>
        /// convience method to get all cells in a room for a specific layer
        /// <seealso cref="BuildingMap"/>
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public IEnumerable<BuildingCell> GetBuildingCells(BuildingLayers layer)
        {
            var area = _building.Map.GetCellsForRoom(this, layer);
            for (int x = area.xMin; x < area.xMax; x++)
            {
                for (int y = area.yMin; x < area.yMax; y++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    yield return new BuildingCell(cell, layer);
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
#if UNITY_EDITOR
            Undo.RecordObject(tm, "Fill room");
#endif
            _room.Fill(tile, tm);
        }

        public RoomTester(Room room)
        {
            _room = room;
            _b =  _room.BuildingTransform.GetComponent<Building>();
        }
    }
}
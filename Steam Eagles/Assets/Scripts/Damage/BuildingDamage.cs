using System;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Buildings.Tiles;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Damage
{
    [RequireComponent(typeof(Building))]
    public class BuildingDamage : MonoBehaviour
    {
        public DamageTilemapConfig[] damageableTilemaps;
        private Building _building;
        private Room[] _damageableRooms;
        [ShowInInspector, HideInEditorMode]
        private RectInt[] _damageableRoomCells;
        
        [ShowInInspector, HideInEditorMode,TableList]
        TilemapDamageHandler[] _tilemapDamageHandlers;
        
        Dictionary<BuildingLayers, TilemapDamageHandler> _tilemapDamageHandlersDict = new Dictionary<BuildingLayers, TilemapDamageHandler>();
        private void Awake()
        {
            _building = GetComponent<Building>();
            var grid = _building.Grid;
            
            var rooms = GetComponentsInChildren<Room>().Where(t => t.IsDamageable).ToArray();
            
            
            _damageableRooms = rooms;
            
            var tilemaps = new [] {_building.WallTilemap.Tilemap, _building.PipeTilemap.Tilemap, _building.WireTilemap.Tilemap};
            _tilemapDamageHandlers = new TilemapDamageHandler[damageableTilemaps.Length];
            for (int i = 0; i < damageableTilemaps.Length; i++)
            {
                var tm = damageableTilemaps[i];
                int cnt = 0;
                var tilemap = _building.GetTilemap(tm.layer).Tilemap;
                _tilemapDamageHandlers[i] = new TilemapDamageHandler(tm.layer, tilemap, grid, tm.damageableTile, tm.repairableTile, rooms);
                _tilemapDamageHandlersDict.Add(tm.layer, _tilemapDamageHandlers[i]);
            }
        }

        private void Start()
        {
            RecreateHandlers();
        }

        private void RecreateHandlers()
        {
            for (int i = 0; i < damageableTilemaps.Length; i++)
            {
                var tm = damageableTilemaps[i];
                if (tm.autofill)
                {
                    _tilemapDamageHandlers[i].AutoFill();
                }
            }
        }

        [Serializable]
        public class DamageTilemapConfig
        {
            public bool autofill = false;
            public BuildingLayers layer;
            [Required] public DamageableTile damageableTile;
            [Required] public RepairableTile repairableTile;
        }


        private void OnDrawGizmos()
        {
            if (_tilemapDamageHandlers == null) return;
            bool reset = false;
            foreach (var tmDamageHandler in _tilemapDamageHandlers)
            {
                if(tmDamageHandler != null)
                    tmDamageHandler.OnDrawGizmos(this);
                else
                {
                    reset = true;
                }
            }

            if (reset)
            {
                RecreateHandlers();
            }
        }

        public IDamageableBuildingLayer GetDamageableLayer(BuildingLayers layer)
        {
            return _tilemapDamageHandlersDict[layer];
        }
    }

    
    
    public class TilemapDamageHandler : IDamageableBuildingLayer
    {
     
        private RectInt[] _roomCells;
        private List<Vector2Int>[] _damageableTilesInRooms;
        private Dictionary<Vector2Int, (int roomIndex, int listIndex, int tileHandleIndex)> _cellRoomLookup = new Dictionary<Vector2Int, (int roomIndex, int listIndex, int tileHandleIndex)>();
        private readonly List<TileHandle> _damageableTiles;

        public TileHandle GetHandle(Vector2Int cell) => _damageableTiles[_cellRoomLookup[cell].tileHandleIndex];
        public int GetCellRoom(Vector2Int cell) => _cellRoomLookup[cell].roomIndex;
        
        

        public TilemapDamageHandler(BuildingLayers layers, Tilemap tilemap, Grid grid,
            DamageableTile damageableTile, RepairableTile repairableTile, 
            Room[] rooms)
        {
            this.Layers = layers;
            this.Tilemap = tilemap;
            this.DamageableTile = damageableTile;
            this.RepairableTile = repairableTile;
            this.Rooms = rooms;
            _damageableTiles = new List<TileHandle>();
            
            _damageableTilesInRooms = new List<Vector2Int>[Rooms.Length];
            _roomCells = new RectInt[Rooms.Length];
            _cellRoomLookup = new Dictionary<Vector2Int, (int roomIndex, int listIndex, int tileHandleIndex)>();
            var tilesList = new List<Vector2Int>();
            for (int i = 0; i < rooms.Length; i++)
            {
                _damageableTilesInRooms[i] = new List<Vector2Int>();
                
                var found = rooms[i].GetCells(tilemap).Select(t => (Vector2Int)t).ToList();
                for (int j = 0; j < found.Count; j++)
                {
                    var cell = found[i];
                    if (_cellRoomLookup.ContainsKey(cell)) continue;
                    _damageableTilesInRooms[i].Add(cell);
                    tilesList.Add(cell);
                    _cellRoomLookup.Add(cell, (i, _damageableTilesInRooms[i].Count-1, tilesList.Count - 1));
                }
                _roomCells[i] = rooms[i].GetCellRect(tilemap);
            }
            _damageableTiles = tilesList
                .Select(t =>
                    new TileHandle(t,
                        onDamage:() => {
                            Debug.Log("$Damaged tile {t.ToString()}");
                            tilemap.SetTile((Vector3Int)t, RepairableTile);
                        }, 
                        onRepair: () =>
                        {
                            Debug.Log($"Repaired tile {t.ToString()}");
                            tilemap.SetTile((Vector3Int)t, DamageableTile);
                        })).ToList();
            
        }

        public Room[] Rooms { get; set; }

        public void AutoFill()
        {
            foreach (var rectInt in Cells)
            {
                AutoFillRect(rectInt);
            }
        }

       public  void AutoFillRect(RectInt rect)
        {
            
        }

        public void OnDrawGizmos(MonoBehaviour mb)
        {
            
        }
        [ShowInInspector]
        public BuildingLayers Layers { get; }

        public RepairableTile RepairableTile { get; }
        
        public RectInt[] Cells { get; }

        public DamageableTile DamageableTile { get; }
        [ShowInInspector]

        public Tilemap Tilemap { get; }
        

        public int RoomCount { get; }

        public List<Vector2Int> GetDamageableTilesInRoom(int roomIndex)
        {
            return _damageableTilesInRooms[roomIndex];
        }

        public List<TileHandle> GetDamageableTiles()
        {
            return _damageableTiles;
        }

        public void DamageTile(Vector2Int tile, int damage)
        {
            GetHandle(tile).DamageCell();
        }
    }
  
}
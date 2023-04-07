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
            }
        }

        private void Start()
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
            foreach (var tmDamageHandler in _tilemapDamageHandlers)
            {
                tmDamageHandler.OnDrawGizmos(this);
            }
        }
        
        public IDamageableBuildingLayer GetDamageableLayer(BuildingLayers layer) => _tilemapDamageHandlers.FirstOrDefault(t => t.Layers == layer);
    }

    
    
    public class TilemapDamageHandler : IDamageableBuildingLayer
    {
     
        
        public TilemapDamageHandler(BuildingLayers layers, Tilemap tilemap, Grid grid,
            DamageableTile damageableTile, RepairableTile repairableTile, 
            Room[] rooms)
        {
            this.Layers = layers;
            this.Tilemap = tilemap;
            this.DamageableTile = damageableTile;
            this.RepairableTile = repairableTile;
            this.Rooms = rooms;
            _damageableTiles = new List<Vector2Int>();
            _damageableTiles = rooms.SelectMany(room => room.GetCells(tilemap).Select(t => (Vector2Int)t)).ToList();
            foreach (var room in rooms)
            {
                
            }
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
        
        List<Vector2Int> _damageableTiles;

        public List<Vector2Int> GetDamageableTiles()
        {
            throw new NotImplementedException();
        }

        public void DamageTile(Vector2Int tile, int damage)
        {
            throw new NotImplementedException();
        }
    }
  
}
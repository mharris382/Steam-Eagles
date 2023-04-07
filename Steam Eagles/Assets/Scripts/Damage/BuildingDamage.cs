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
               var _damageableRoomCells = new RectInt[rooms.Length];
                foreach (var room in rooms)
                {
                    Debug.Log($"Found damageable room: {room.name}");
                    var bounds = room.WorldSpaceBounds;
                    var min = bounds.min;
                    min.z = 0;
                    var max = bounds.max;
                    max.z = 0;
                    var cellMin = tilemap.layoutGrid.WorldToCell(min);
                    var cellMax = tilemap.layoutGrid.WorldToCell(max);
                    _damageableRoomCells[cnt] = new RectInt(cellMin.x, cellMin.y, cellMax.x, cellMax.y);
                    cnt++;
                }
                
                _tilemapDamageHandlers[i] = new TilemapDamageHandler(tm.layer,
                   tilemap, grid, tm.damageableTile, tm.repairableTile, _damageableRoomCells);
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
    }

    public class Tester
    {
        public TilemapDamageHandler damageHandler;

        [ShowInInspector, TableList]
        public RectWrapper[] wrappers;
        [ShowInInspector]
        public class RectWrapper
        {
            [HideInInspector]
            public TilemapDamageHandler damageHandler;
            [ShowInInspector]
            public RectInt rect;

            public RectWrapper(TilemapDamageHandler damageHandler, RectInt rect)
            {
                this.damageHandler = damageHandler;
                this.rect = rect;
            }

            [Button]
            public void Fill()
            {
                damageHandler.AutoFillRect(rect);
            }
        }

        public Tester(TilemapDamageHandler tilemapDamageHandler)
        {
            damageHandler = tilemapDamageHandler;
            wrappers = new RectWrapper[tilemapDamageHandler.Cells.Length];
            for (int i = 0; i < wrappers.Length; i++)
            {
                wrappers[i] = new RectWrapper(tilemapDamageHandler,tilemapDamageHandler.Cells[i] );
            }   
        }
    }

    
    public class TilemapDamageHandler
    {
        [Button]
        void OpenTester()
        {
            OdinEditorWindow.InspectObject(new Tester(this));
        }
        public TilemapDamageHandler(BuildingLayers layers, Tilemap tilemap, Grid grid,
            DamageableTile damageableTile, RepairableTile repairableTile, 
            RectInt[] cells)
        {
            this.Layers = layers;
            this.Tilemap = tilemap;
            this.DamageableTile = damageableTile;
            this.RepairableTile = repairableTile;
            this.Cells = cells;
            


        }

        public void AutoFill()
        {
            foreach (var rectInt in Cells)
            {
                AutoFillRect(rectInt);
            }
        }

       public  void AutoFillRect(RectInt rect)
        {
            var scale = Tilemap.transform.lossyScale;
            for (int x = rect.min.x; x < rect.max.x; x++)
            {
                for (int y = rect.min.y; y < rect.max.y; y++)
                {
                    var vec = new Vector3Int(x, y, 0);
                    var tile = Tilemap.GetTile(vec);
                    if (tile == null)
                        Tilemap.SetTile(vec, DamageableTile);
                }
            }
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
    }
  
}
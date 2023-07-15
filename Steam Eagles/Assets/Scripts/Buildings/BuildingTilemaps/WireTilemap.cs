using System.Collections.Generic;
using System.Linq;
using Buildings.Graph;
using PhysicsFun.Buildings;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;
using Zenject;

namespace Buildings.BuildingTilemaps
{
    public class WireTilemap : DestructableTilemap
    {
        [Tooltip("tilemap will derive it's state from this tilemap, will set tiles to activeWireTile if they are active in this tilemap")]
        [SerializeField,Required] private Tilemap wiresListenerTilemap;
        [SerializeField,Required] private TileBase activeWireTile;
        [SerializeField,Required] private TileBase inactiveWireTile;
        public int zOffset = 5;
        
        
        private WireTilemapGraph _graph;


        [Inject] void Install(WireTilemapGraph wireGrid)
       {
           _graph = wireGrid;
       }
        
        public override BuildingLayers Layer => BuildingLayers.WIRES;


        [Button]
        public void Init()
        {
            if (_graph == null) return;
            var sourceCells = _graph.GetSourceCells().ToList();
            Queue<BuildingCell> cells = new(sourceCells);
            HashSet<BuildingCell> visited = new();
            while (cells.Count > 0)
            {
                var current = cells.Dequeue();
                SetCell(current, true);
                visited.Add(current);
                
                foreach (var buildingCell in current.GetNeighbors())
                {
                    if (visited.Contains(buildingCell)) continue;
                    cells.Enqueue(buildingCell);
                }
            }

            foreach (var cell in _graph.UndirectedGraph.Vertices)
            {
                if (visited.Contains(cell)) continue;
                SetCell(cell, false);
            }
        }
        
        
        void SetCell(BuildingCell cell, bool active)
        {
            var tilemap = wiresListenerTilemap;
            var tile = active ? activeWireTile : inactiveWireTile;
            var pos = cell.cell;
            pos.z -= zOffset;
            tilemap.SetTile(pos, tile);
        }
        
        public override string GetSaveID()
        {
            return "Wires";
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - 10;
        }


        public override IEnumerable<string> GetTileAddresses()
        {
            yield return "WireTile";
        }
    }
}
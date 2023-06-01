using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Buildings.Tiles;
using QuikGraph.Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

namespace Buildings
{
    public interface IBGrid
    {
        Vector2 GetCellSize(BuildingLayers layer);
        Vector3Int WorldToCell(Vector3 wp, BuildingLayers buildingLayers);
        Vector3 CellToWorld(Vector3Int cell, BuildingLayers buildingLayers);
        Vector3 CellToWorld(BuildingCell cell);
        Vector3 CellToLocal(BuildingCell cell);
        Vector3 CellToLocal(Vector3Int cell, BuildingLayers buildingLayers);
    }

    public abstract class BGridRuleInstaller : Installer<EditableTile, BGridRuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<IBGridRule>().To<CompositeBGridRule>().FromNew().AsSingle();
            InstallBGridRules();
        }

        public abstract void InstallBGridRules();

        class CompositeBGridRule : IBGridRule
        {
            [Inject]
            public List<IBGridRule> rules;
            public bool CanPlaceTile(IBGridReader gridReader, BuildingCell buildingCell, EditableTile tile, ref string failureMessage)
            {

                foreach (var bGridRule in rules)
                {
                    if (!bGridRule.CanPlaceTile(gridReader, buildingCell, tile, ref failureMessage))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

    }

    
    /// <summary>
    /// can be injected globally to contain all the rules for placing tiles on the building grid. will need to be able
    /// to resolve a set of IBGridRules for a given tile type. Since we don't know which set we need until runtime, we
    /// will use a factory to create the set of rules for a given tile type.
    /// </summary>
    public interface IBGridRule
    {
        bool CanPlaceTile(IBGridReader gridReader, BuildingCell buildingCell, EditableTile tile, ref string failureMessage);
    }
    
    public interface IBGridReader
    {
        bool HasTile(BuildingCell buildingCell);
        TileBase GetTile(BuildingCell buildingCell);
    }
    public interface IBGridWriter   
    {
        bool SetTile(BuildingCell buildingCell, TileBase tile);
    }
    

    public class BuildingMap : IBuildingRoomLookup, IBGrid
    {
        public class Factory : PlaceholderFactory<Building, BuildingMap> { }
        
        private readonly Building _building;
        private class CellToRoomLookup
        {
            public readonly Vector2 cellSize;
            private Dictionary<Vector3Int, Room> _cellToRoomLookup = new Dictionary<Vector3Int, Room>();
            private Dictionary<Room, BoundsInt> _roomToCellBounds = new Dictionary<Room, BoundsInt>();
            public CellToRoomLookup(GridLayout grid, Room[] rooms)
            {
                this.cellSize = grid.cellSize;
                foreach (var room in rooms)
                {
                    var roomBounds = room.GetCells(grid);
                    _roomToCellBounds.Add(room, roomBounds);
                    for (int x = roomBounds.xMin; x < roomBounds.xMax; x++)
                    {
                        for (int y = roomBounds.yMin; y < roomBounds.yMax; y++)
                        {
                            var cell = new Vector3Int(x, y, 0);
                            if (!_cellToRoomLookup.ContainsKey(cell))
                            {
                                _cellToRoomLookup.Add(cell, room);
                            }
                            else
                            {
                                Debug.LogError($"Cell {cell} is already registered to {_cellToRoomLookup[cell].name} but also found in {room.name}");
                            }
                        }
                    }
                }
            }
            public Room GetRoom(Vector3Int cell) => _cellToRoomLookup.ContainsKey(cell) ? _cellToRoomLookup[cell] : null;

            public BoundsInt GetCells(Room room) => _roomToCellBounds[room];
            public IEnumerable<(Room room, Vector3Int cell)> GetAllCells() => _cellToRoomLookup.Select(kvp => (kvp.Value, kvp.Key));
            public IEnumerable<(Room room, BoundsInt bounds)> GetAllBounds() => _roomToCellBounds.Select(kvp => (kvp.Key, kvp.Value));
            
            public bool HasCell(Vector3Int cell) => _cellToRoomLookup.ContainsKey(cell);
        }

        private readonly Dictionary<Vector2, CellToRoomLookup> _cellToRoomMaps;
        private readonly RoomGraph _roomGraph;

        private readonly Dictionary<BuildingLayers, Vector2> _layerToCellSize;
        private readonly Dictionary<BuildingLayers, Tilemap> _layerToTilemap;

        public BuildingGraphs BuildingGraphs { get; }

        public BuildingMap(Building building)
        {
            Room[] rooms = building.GetComponentsInChildren<Room>();
            _building = building;
            _cellToRoomMaps = new Dictionary<Vector2, CellToRoomLookup>();
            _layerToCellSize = new Dictionary<BuildingLayers, Vector2>();
            _layerToTilemap = new Dictionary<BuildingLayers, Tilemap>();
            _roomGraph = new RoomGraph(building.GetComponentInChildren<Rooms.Rooms>());
            
            BoundsInt buildingBounds = new BoundsInt();
            foreach (var buildingTilemap in building.GetAllBuildingLayers())
            {
                var cellSize = buildingTilemap.CellSize;
                var tmLayer = buildingTilemap.Layer;
                
                if(!_layerToCellSize.ContainsKey(tmLayer))
                    _layerToCellSize.Add(tmLayer, cellSize);
                
                buildingBounds = buildingBounds.Encapsulate(buildingTilemap.Tilemap.cellBounds);
                
                if (!_layerToTilemap.ContainsKey(tmLayer)) 
                    _layerToTilemap.Add(tmLayer, buildingTilemap.Tilemap);
                
                if (!_cellToRoomMaps.ContainsKey(cellSize))
                    _cellToRoomMaps.Add(cellSize, new CellToRoomLookup(buildingTilemap.Tilemap.layoutGrid, rooms));
            }
            //NOTE: must be called at the end of the constructor so the building map is fully initialized
            BuildingGraphs = new BuildingGraphs(this);
        }

        public Tilemap GetTilemap(BuildingLayers layers)
        {
            return _layerToTilemap[layers];
        }

        public IEnumerable<(BoundsInt, Room)> GetAllBoundsForLayer(BuildingLayers layer) => GetMapForLayer(layer).GetAllBounds().Select(kvp => (kvp.bounds, kvp.room));
        public IEnumerable<(BoundsInt, Room)> GetAllBoundsForLayer(BuildingLayers layer, Predicate<Room> roomFilter) => GetMapForLayer(layer).GetAllBounds().Where(t => roomFilter(t.room)).Select(kvp => (kvp.bounds, kvp.room));

        public IEnumerable<(BuildingLayers layer, Vector2 cellSize)> GetUniqueLayers() => _layerToCellSize.Select(kvp => (kvp.Key, kvp.Value));

        public RoomGraph RoomGraph => _roomGraph;

        
        CellToRoomLookup GetMapForLayer(BuildingLayers layer)
        {
            if (!_layerToCellSize.ContainsKey(layer))
                return _cellToRoomMaps[Vector2.one];
            
            return _cellToRoomMaps[_layerToCellSize[layer]];
        }

        public Room GetRoom(Vector3Int cell, BuildingLayers layers) => GetMapForLayer(layers).GetRoom(cell);
        public BoundsInt GetCellsForRoom(Room room, BuildingLayers layers) => GetMapForLayer(layers).GetCells(room);
        public bool CellIsInARoom(Vector3Int cell, BuildingLayers layer) => GetMapForLayer(layer).HasCell(cell);

        public Vector2 GetCellSize(BuildingLayers layer) => _layerToCellSize[layer];

        public Vector3Int  WorldToCell(Vector3 wp, BuildingLayers buildingLayers) => _layerToTilemap[buildingLayers].WorldToCell(wp);
        public Vector3 CellToWorld(Vector3Int cell, BuildingLayers buildingLayers) => _layerToTilemap[buildingLayers].CellToWorld(cell);
        public Vector3 CellToWorld(BuildingCell cell) => CellToWorld(cell.cell, cell.layers);
        public Vector3 CellToLocal(BuildingCell cell) => CellToLocal(cell.cell, cell.layers);
        
        public Vector3 CellToLocal(Vector3Int cell, BuildingLayers buildingLayers) => _layerToTilemap[buildingLayers].CellToLocal(cell);
        public IEnumerable<BuildingCell> ConvertBetweenLayers(BuildingCell fromCell, BuildingLayers toLayer) => ConvertBetweenLayers(fromCell.layers, toLayer, fromCell.cell).Select(c => new BuildingCell(c, toLayer));
        public IEnumerable<Vector3Int> ConvertBetweenLayers(BuildingLayers fromLayer, BuildingLayers toLayer,
            Vector3Int fromCell)
        {
            var fromSize = _layerToCellSize[fromLayer];
            var toSize = _layerToCellSize[toLayer];
            if (Math.Abs(fromSize.x - toSize.x) < Mathf.Epsilon)
            {
                yield return fromCell;
            }
            // from is bigger than to so there are more than one cell in to
            else if (fromSize.x > toSize.x)
            {
                int numCells = Mathf.RoundToInt(fromSize.x / toSize.x);
                var bottomLeft = _layerToTilemap[toLayer].layoutGrid
                    .LocalToCell(_layerToTilemap[fromLayer].layoutGrid.CellToLocal(fromCell));
                for (int i = 0; i < numCells; i++)
                {
                    for (int j = 0; j < numCells; j++)
                    {
                        yield return bottomLeft + new Vector3Int(i, j, 0);
                    }
                }
            }
            //to is bigger than from so there is only one cell in to
            else if (fromSize.x < toSize.x)
            {
                yield return _layerToTilemap[toLayer].layoutGrid
                    .LocalToCell(_layerToTilemap[fromLayer].layoutGrid.CellToLocal(fromCell));
            }


        }

        public TileBase GetTile(Vector3Int cell, BuildingLayers layers)
        {
            if (!_layerToTilemap.ContainsKey(layers))
                return null;
            return _layerToTilemap[layers].GetTile(cell);
        }

        public TileBase GetTile(BuildingCell cell) => GetTile(cell.cell, cell.layers);

        public T GetTile<T>(Vector3Int cell, BuildingLayers layers) where T : TileBase
        {
            if (!_layerToTilemap.ContainsKey(layers))
            {
                Debug.LogError($"No tilemap found for layer {layers}");
                return null;
            }
            return _layerToTilemap[layers].GetTile<T>(cell);
        }

        public IEnumerable<Vector3Int> GetPath(Vector2Int pathStart, Vector2Int pathEnd, BuildingLayers getLayer)
        {
            if (pathStart == pathEnd)
            {
                return new List<Vector3Int> {(Vector3Int)pathStart};
            }

            var graph = BuildingGraphs.GetGraphForLayer(getLayer);
            if (!graph.AdjacencyGraph.ContainsVertex(pathStart) ||
                !graph.AdjacencyGraph.ContainsVertex(pathEnd))
            {
                return new List<Vector3Int> { (Vector3Int)pathStart,(Vector3Int) pathEnd };
            }

            // var result = graph.AdjacencyGraph.ShortestPathsDijkstra(e => 1, pathStart);
            var result = graph.AdjacencyGraph.ShortestPathsAStar(e => 1, v => Vector2Int.Distance(v, pathEnd), pathStart);
            if (result(pathEnd, out var path))
            {
                return path.Select(t => (Vector3Int)t.Target);
            }
            return new List<Vector3Int> { (Vector3Int)pathStart, (Vector3Int)pathEnd };
        }

        public void SetTile(Vector3Int cell, BuildingLayers layer, EditableTile tile)
        {
            var tm = GetTilemap(layer);
            tm.SetTile(cell, tile);
            _building.tilemapChangedSubject.OnNext(new BuildingTilemapChangedInfo(_building, tm, cell, layer));
        }

        public void SetTile(Vector3Int cell, EditableTile tile)
        {
            if (tile == null)
            {
                throw new NullReferenceException("Cannot clear cell by setting tile to null. If this was the intent use overload \'SetTile(cell, layer, null)\' instead");
            }
            var layer = tile.GetLayer();
            SetTile(cell, layer, tile);
            
        }

        public IEnumerable<Vector3Int> GetAllNonEmptyCells(BuildingLayers layer) => GetAllCells(layer).Where(t => GetTile(t, layer) != null);

        public IEnumerable<Vector3Int> GetAllCells(BuildingLayers layers) => _cellToRoomMaps[_layerToCellSize[layers]].GetAllCells().Select(t => t.cell);

    }


    
}
using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Buildings.Tiles;
using QuikGraph.Algorithms;
using UniRx;
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

        private readonly Dictionary<BuildingLayers, HashSet<Vector2Int>> _blockedCells = new();
        private readonly Dictionary<BuildingLayers, Vector2> _layerToCellSize;
        private readonly Dictionary<BuildingLayers, Tilemap> _layerToTilemap;
        private readonly Dictionary<BuildingLayers, BuildingMapEvents> _layerToEvents = new ();
        class BuildingMapEvents
        {
            public Subject<(Vector3Int cell, TileBase tile)> onTileChanged = new Subject<(Vector3Int cell, TileBase)>();
            public IObservable<(Vector3Int cell, TileBase tile)> OnTileSet => onTileChanged.Where(t => t.tile != null);
            public IObservable<Vector3Int> OnTileCleared => onTileChanged.Where(t => t.tile == null).Select(t => t.cell);
        }

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

        public Vector3 CellToWorldCentered(Vector3Int cell, BuildingLayers buildingLayers) => CellToWorld(cell, buildingLayers) + (Vector3)GetCellSize(buildingLayers)/2f;
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

        public bool HasCell(BuildingCell cell) => GetTile(cell) != null;
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

        private void SetTile(BuildingCell cell, TileBase tile)
        {
            var tm = GetTilemap(cell.layers);
            tm.SetTile(cell.cell, tile);
            _building.tilemapChangedSubject.OnNext(new BuildingTilemapChangedInfo(_building, tm, cell.cell, cell.layers));
            var room = GetRoom(cell.cell, cell.layers);
            
            var buildingEvents = GetBuildingMapEvents(cell.layers);
            buildingEvents.onTileChanged.OnNext((cell.cell, tile));
            
            var roomEvents = room.GetComponent<RoomEvents>();
            Debug.Assert(roomEvents != null, room);
            roomEvents.OnTileSet(cell.cell, cell.layers, tile);
        }
        
        public void SetTile(Vector3Int cell, BuildingLayers layer, TileBase tile) => SetTile(new BuildingCell(cell, layer), tile);

        public void SetTile(Vector3Int cell, BuildingLayers layer, EditableTile tile) => SetTile(new BuildingCell(cell, layer), tile);

        public void SetTile(Vector3Int cell, EditableTile tile)
        {
            var layer = tile.GetLayer();
            SetTile(cell, layer, tile);
            
        }


        public bool IsCellBlocked(Vector3Int cell, BuildingLayers layer) => IsCellBlocked((Vector2Int)cell, layer);
        public bool IsCellBlocked(BuildingCell cell) => IsCellBlocked(cell.cell2D, cell.layers);
        public bool IsCellBlocked(Vector2Int cell, BuildingLayers layer)
        {
            if (_blockedCells.TryGetValue(layer, out var blockedCells))
            {
                return blockedCells.Contains(cell);
            }
            return false;
        }
        public void UnblockCell(Vector2Int cell, BuildingLayers layer)
        {
            if (_blockedCells.TryGetValue(layer, out var blockedCells))
            {
                blockedCells.Remove(cell);
            }
        }
        public void UnblockCells(IEnumerable<Vector2Int> cells, BuildingLayers layer)
        {
            if (_blockedCells.TryGetValue(layer, out var blockedCells))
            {
                foreach (var cell in cells)
                {
                    blockedCells.Remove(cell);
                }
            }
        }

        public void BlockCell(Vector2Int cell, BuildingLayers layer)
        {
            if (!_blockedCells.TryGetValue(layer, out var blockedCells))
            {
                _blockedCells.Add(layer, blockedCells = new ());
            }
            blockedCells.Add(cell);
        }
        
        public void BlockCells(IEnumerable<Vector2Int> cells, BuildingLayers layer)
        {
            if (!_blockedCells.TryGetValue(layer, out var blockedCells))
            {
                _blockedCells.Add(layer, blockedCells = new ());
            }
            foreach (var cell in cells) _blockedCells[layer].Add(cell);
        }

        public IEnumerable<Vector3Int> GetAllNonEmptyCells(BuildingLayers layer) => GetAllCells(layer).Where(t => GetTile(t, layer) != null);

        public IEnumerable<Vector3Int> GetAllCells(BuildingLayers layers) => _cellToRoomMaps[_layerToCellSize[layers]].GetAllCells().Select(t => t.cell);

        private BuildingMapEvents GetBuildingMapEvents(BuildingLayers layers)
        {
            if (!_layerToEvents.TryGetValue(layers, out var bme))
            {
                _layerToEvents.Add(layers, bme = new BuildingMapEvents());
            }
            return bme;
        }


        public IObservable<Vector2Int> OnTileCleared2D(BuildingLayers layers) => OnTileCleared(layers).Select(t => (Vector2Int)t);
        public IObservable<Vector3Int> OnTileCleared(BuildingLayers layers) => GetBuildingMapEvents(layers).OnTileCleared;
        public IObservable<(Vector2Int cell, TileBase tile)> OnTileSet2D(BuildingLayers layers) => OnTileSet(layers).Select(t => ((Vector2Int)t.cell, t.tile));
        public IObservable<(Vector3Int cell, TileBase tile)> OnTileSet(BuildingLayers layers) => GetBuildingMapEvents(layers).OnTileSet;
    }


    
}
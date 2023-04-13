using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using QuikGraph.Algorithms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    public class BuildingMap : IBuildingRoomLookup
    {
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

        public BuildingMap(Building building, Room[] rooms)
        {
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


        public IEnumerable<(BuildingLayers layer, Vector2 cellSize)> GetUniqueLayers() => _layerToCellSize.Select(kvp => (kvp.Key, kvp.Value));

        public RoomGraph RoomGraph => _roomGraph;

        private CellToRoomLookup GetMapForLayer(BuildingLayers layer)
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

        public T GetTile<T>(Vector3Int cell, BuildingLayers layers) where T : TileBase
        {
            if (!_layerToTilemap.ContainsKey(layers))
            {
                Debug.LogError($"No tilemap found for layer {layers}");
                return null;
            }
            return _layerToTilemap[layers].GetTile<T>(cell);
        }

        public List<Vector3Int> GetPath(Vector2Int pathStart, Vector2Int pathEnd, BuildingLayers getLayer)
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

            var result = graph.AdjacencyGraph.ShortestPathsDijkstra(e => 1, pathStart);
            if (result(pathEnd, out var path))
            {
                return path.Select(t =>(Vector3Int)t.Source).ToList();
            }
            return new List<Vector3Int> { (Vector3Int)pathStart, (Vector3Int)pathEnd };
        }
    }


    
}
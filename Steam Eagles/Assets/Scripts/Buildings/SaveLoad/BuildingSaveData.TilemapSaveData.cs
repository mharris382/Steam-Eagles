using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.SaveLoad
{
    [Serializable]
        public class TilemapSaveData
        {
          [SerializeField] private TileSaveData[] _saveData;
          [SerializeField] private TileSaveData[] _emptyTileData;
            public TilemapSaveData(Building building)
            {
                Dictionary<TileBase, TileSaveData> saveDataFound = new Dictionary<TileBase, TileSaveData>();
                var emptySaveData = new Dictionary<BuildingLayers, TileSaveData>();

                SaveTilesOnLayer(BuildingLayers.SOLID);
                SaveTilesOnLayer(BuildingLayers.WALL);
                SaveTilesOnLayer(BuildingLayers.WIRES);
                SaveTilesOnLayer(BuildingLayers.PIPE);

                _saveData = saveDataFound.Values.ToArray();
                _emptyTileData = emptySaveData.Values.ToArray();
                Debug.Log($"Found {_saveData.Length} tiles to save.");
                
                void SaveTilesOnLayer(BuildingLayers layer)
                {
                    var allSolidCells = building.Map.GetAllCells(layer);
                    foreach (var cell in allSolidCells)
                    {
                        var tile = building.Map.GetTile(cell, layer);
                        if (tile == null)
                        {
                            if (!emptySaveData.ContainsKey(layer))
                                emptySaveData.Add(layer, new TileSaveData(layer, null));
                            var data = emptySaveData[layer];
                            data.cells.Add((Vector2Int)cell);
                        }
                        else
                        {
                            if (!saveDataFound.TryGetValue(tile, out var saveData))
                            {
                                saveData = new TileSaveData(layer, tile);
                                saveDataFound.Add(tile, saveData);
                            }
                            saveData.cells.Add((Vector2Int)cell);
                        }
                    }
                }
            }
            
            [Serializable]
            public class TileSaveData
            {
                public BuildingLayers layer;
                public TileBase tile;
                public List<Vector2Int> cells;

                public TileSaveData(BuildingLayers layer, TileBase tile)
                {
                    this.layer = layer;
                    this.tile = tile;
                    cells = new List<Vector2Int>();
                }

                public void LoadData(Building building)
                {
                    Debug.Assert(tile != null, "saved tile is null");
                    LoadData(building.Map.GetTilemap(layer));
                }

                public void LoadData(Tilemap tilemap)
                {
                    foreach (var vector2Int in cells)
                    {
                        var cell = new Vector3Int(vector2Int.x, vector2Int.y, 0);
                        tilemap.SetTile(cell, tile);
                    }
                }
            }


            public void LoadTilemapData(Building building)
            {
                Debug.Log($"Loading tilemap data for building {building.ID}...",building);
                
                foreach (var emptyTiles in _emptyTileData)
                {
                    Debug.Log($"Loading {emptyTiles.cells.Count} empty tiles for {emptyTiles.layer} on building {building.ID}.", building);
                    foreach (var cell in emptyTiles.cells)
                    {
                        building.Map.SetTile((Vector3Int)cell, emptyTiles.layer, null);
                    }
                }
                
                foreach (var saveData in _saveData)
                {
                    Debug.Assert(saveData.tile != null, "saved tile is null");
                    Debug.Assert(saveData.tile is EditableTile);
                    Debug.Log($"Loading {saveData.cells.Count} {saveData.tile.name} tiles for {saveData.layer} on building {building.ID}.", building);
                    foreach (var saveDataCell in saveData.cells)
                    {
                        building.Map.SetTile((Vector3Int)saveDataCell, saveData.layer, saveData.tile as EditableTile);
                    }
                }
                
                Debug.Log($"Finished Loading tilemap data onto building {building.ID}.", building);
            }
        }
}
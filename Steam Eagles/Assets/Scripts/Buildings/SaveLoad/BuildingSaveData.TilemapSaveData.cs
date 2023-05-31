using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms;
using Buildings.Tiles;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.SaveLoad
{
    [Serializable]
        public class TilemapSaveData
        {
          //[SerializeField] private TileSaveData[] _saveData;
          //[SerializeField] private TileSaveData[] _emptyTileData;
         [SerializeField] TilemapSaveDataV2 tilemapSaveDataV2;
            public TilemapSaveData(Building building)
            {
                tilemapSaveDataV2 = new TilemapSaveDataV2(building);
                // Dictionary<TileBase, TileSaveData> saveDataFound = new Dictionary<TileBase, TileSaveData>();
                // var emptySaveData = new Dictionary<BuildingLayers, TileSaveData>();
                //
                // SaveTilesOnLayer(BuildingLayers.SOLID);
                // SaveTilesOnLayer(BuildingLayers.WALL);
                // SaveTilesOnLayer(BuildingLayers.WIRES);
                // SaveTilesOnLayer(BuildingLayers.PIPE);
                //
                // _saveData = saveDataFound.Values.ToArray();
                // _emptyTileData = emptySaveData.Values.ToArray();
                // Debug.Log($"Found {_saveData.Length} tiles to save.");
                //
                // void SaveTilesOnLayer(BuildingLayers layer)
                // {
                //     var allSolidCells = building.Map.GetAllCells(layer);
                //     foreach (var cell in allSolidCells)
                //     {
                //         var tile = building.Map.GetTile(cell, layer);
                //         if (tile == null)
                //         {
                //             if (!emptySaveData.ContainsKey(layer))
                //                 emptySaveData.Add(layer, new TileSaveData(layer, null));
                //             var data = emptySaveData[layer];
                //             data.cells.Add((Vector2Int)cell);
                //         }
                //         else
                //         {
                //             if (!saveDataFound.TryGetValue(tile, out var saveData))
                //             {
                //                 saveData = new TileSaveData(layer, tile);
                //                 saveDataFound.Add(tile, saveData);
                //             }
                //             saveData.cells.Add((Vector2Int)cell);
                //         }
                //     }
                // }
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
                tilemapSaveDataV2.Load(building);
                //Debug.Log($"Loading tilemap data for building {building.ID}...",building);
                //
                //foreach (var emptyTiles in _emptyTileData)
                //{
                //    Debug.Log($"Loading {emptyTiles.cells.Count} empty tiles for {emptyTiles.layer} on building {building.ID}.", building);
                //    foreach (var cell in emptyTiles.cells)
                //    {
                //        building.Map.SetTile((Vector3Int)cell, emptyTiles.layer, null);
                //    }
                //}
                //foreach (var saveData in _saveData)
                //{
                //    Debug.Assert(saveData.tile != null, "saved tile is null");
                //    Debug.Assert(saveData.tile is EditableTile);
                //    Debug.Log($"Loading {saveData.cells.Count} {saveData.tile.name} tiles for {saveData.layer} on building {building.ID}.", building);
                //    foreach (var saveDataCell in saveData.cells)
                //    {
                //        building.Map.SetTile((Vector3Int)saveDataCell, saveData.layer, saveData.tile as EditableTile);
                //    }
                //}
                //
                //Debug.Log($"Finished Loading tilemap data onto building {building.ID}.", building);
            }
        }


        [Serializable]
        public class TilemapSaveDataV2
        {
            [SerializeField]
            private List<TilemapLayerSaveData> layers;
            public TilemapSaveDataV2(Building building)
            {
                layers = new List<TilemapLayerSaveData>();
                layers.Add(new TilemapLayerSaveData(building, BuildingLayers.PIPE));
                layers.Add(new TilemapLayerSaveData(building, BuildingLayers.WALL));
                layers.Add(new TilemapLayerSaveData(building, BuildingLayers.WIRES));
                layers.Add(new TilemapLayerSaveData(building, BuildingLayers.SOLID));
            }
            
            [Serializable]
            public class TilemapLayerSaveData
            {
                [SerializeField] private List<Chunk> chunks;
                [SerializeField] private string layerName;
                public TilemapLayerSaveData(Building building, BuildingLayers layers)
                {
                    layerName = layers.ToString();
                   chunks = building.Map.GetAllBoundsForLayer(layers, room => room.buildLevel == BuildLevel.FULL)
                       .Select(t => new Chunk(building, layers, t.Item1, t.Item2.name)).ToList();
                }

                public void Load(Building building)
                {
                    foreach (var chunk in chunks)
                    {
                        chunk.LoadChunk(building);
                    }
                }

                public async UniTask LoadAsync(Building building)
                {
                    Debug.Log($"Loading chunks for {layerName}");
                    await UniTask.WaitUntil(() => building.Tiles.isReady);
                    await UniTask.WhenAll(chunks.Select(t => UniTask.RunOnThreadPool(() => t.LoadChunk(building))));
                }
                
                [Serializable] public class Chunk
                {
                    [SerializeField] private string roomName;
                    [SerializeField] private string layerName;
                    [SerializeField] private  BuildingLayers layer;
                    [SerializeField] private BoundsInt bounds;
                    [SerializeField] private List<TileBase> tiles;
                    [SerializeField] private int[] tileIds; 
                    public Chunk(Building building, BuildingLayers layer, BoundsInt bounds, string roomName)
                    {
                        this.layer = layer;
                        this.layerName = this.layer.ToString();
                        this.roomName = roomName;
                        this.bounds = bounds;
                        tiles = new List<TileBase>();
                        tileIds = new int[bounds.size.x * bounds.size.y];
                        //save tiles from building
                        for (int x = bounds.xMin; x < bounds.xMax; x++)
                        {
                            for (int y = bounds.yMin; y < bounds.yMax; y++)
                            {
                                var cell = new Vector3Int(x, y, 0);
                                var xIndex = x - bounds.xMin;
                                var yIndex = y - bounds.yMin;
                                var index = xIndex + yIndex * bounds.size.x;
                                var tileId = -1;
                                var tile = building.Map.GetTile(cell, layer);
                                if (tile != null)
                                {
                                    if (tiles.Contains(tile) == false) tiles.Add(tile);
                                    tileId = tiles.IndexOf(tile);
                                }
                                tileIds[index] = tileId;
                            }
                        }
                    }

                    public void LoadChunk(Building building)
                    {
                        for (int x = bounds.xMin; x < bounds.xMax; x++)
                        {
                            for (int y = bounds.yMin; y < bounds.yMax; y++)
                            {
                                var cell = new Vector3Int(x, y, 0);
                                var index = (x - bounds.xMin) + (y - bounds.yMin) * bounds.size.x;
                                var tileId = tileIds[index];
                                var tile = tileId == -1 ? null : tiles[tileId];
                                if (tileId == -1 && layer == BuildingLayers.WALL)
                                {
                                    if (tiles.Count == 0 )
                                    {
                                        if (building.Tiles.isReady)
                                        {
                                            tile = building.Tiles.WallTile;
                                            Debug.Assert(tile != null, "tile is null");
                                        }
                                        else
                                        {
                                            building.StartCoroutine(WaitForTilesAndLoadChunk(building));
                                            return;
                                        }
                                    }
                                }
                                building.Map.SetTile(cell, layer, tile as EditableTile);
                            }
                        }
                    }


                    public IEnumerator WaitForTilesAndLoadChunk(Building building)
                    {
                        while (building.Tiles.isReady == false)
                        {
                            yield return null;
                        }
                        LoadChunk(building);
                    }
                }
                
            }

            public void Load(Building building)
            {
                foreach (var tilemapLayerSaveData in layers)
                {
                    tilemapLayerSaveData.Load(building);
                }
            }
            public async UniTask LoadAsync(Building building)
            {
                await UniTask.WhenAll(layers.Select(t => t.LoadAsync(building)));
            }
        }
}
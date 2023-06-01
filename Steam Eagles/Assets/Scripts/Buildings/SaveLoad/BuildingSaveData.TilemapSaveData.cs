using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Buildings;
using Buildings.Rooms;
using Buildings.Tiles;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

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
        }


    
}

public class TilemapSaveDataV3Installer : Installer<TilemapSaveDataV3Installer>
{
    public override void InstallBindings()
    {
        Container.Bind<TilemapsSaveDataV3>().AsSingle().NonLazy();
        Container.BindFactory<Room, RoomTilemapTextures, RoomTilemapTextures.Factory>().AsSingle().NonLazy();
    }
}

[Serializable]
public class TilemapsSaveDataV3
{
    
    private readonly Building _building;
    private readonly RoomTilemapTextures.Factory _factory;

    public TilemapsSaveDataV3(Building building, RoomTilemapTextures.Factory factory)
    {
        _building = building;
        _factory = factory;
        var rooms = building.Rooms.AllRooms.Where(t => t.buildLevel != BuildLevel.NONE).ToList();
    }

    public async UniTask<bool> LoadGame()
    {
        var textures =  _building.Rooms.AllRooms.Where(t => t.buildLevel != BuildLevel.NONE).Select(t => _factory.Create(t))
            .ToList();
        var allSuccess = true;
        foreach (var texture in textures)
        {
            Debug.Assert(texture._room!=null);
            var result = await texture.LoadRoom(texture._room);
            if (!result)
            {
                allSuccess = false;
                Debug.Log($"Failed to load room {texture._room.name}");
            }
        }
        return allSuccess;
    }
            
    public async UniTask<bool> SaveGame()
    {
        var textures =  _building.Rooms.AllRooms.Where(t => t.buildLevel != BuildLevel.NONE).Select(t => _factory.Create(t))
            .ToList();
        var results = await UniTask.WhenAll(textures.Select(t => t.SaveRoom(t._room)));
        return results.All(t => t);
    }
}

public class RoomTilemapTextures
{
    public readonly Room _room;
    private readonly GlobalSavePath _savePath;

    private RoomTexture solidTexture;
    private RoomTexture pipeTexture;
    private RoomTexture wallTexture;
    private RoomTexture wireTexture;

    public class Factory : PlaceholderFactory<Room, RoomTilemapTextures>
    {
    }

    public RoomTilemapTextures(Room room, GlobalSavePath savePath)
    {
        _room = room;
        _savePath = savePath;
        var building = room.Building;
        var map = building.Map;
        solidTexture = new RoomTexture(room, BuildingLayers.SOLID);
        pipeTexture = new RoomTexture(room, BuildingLayers.PIPE);
        wallTexture = new RoomTexture(room, BuildingLayers.WALL);
        wireTexture = new RoomTexture(room, BuildingLayers.WIRES);
    }

    IEnumerable<RoomTexture> RoomTextures()
    {
        yield return solidTexture;
        yield return pipeTexture;
        yield return wallTexture;
        yield return wireTexture;
    }

    public async UniTask<bool> LoadRoom(Room room)
    {
        var success = true;
        foreach (var task in RoomTextures().Select(t => t.LoadRoom(room, _savePath.FullSaveDirectoryPath)))
        {
            var result = await task;
            if (!result)
            {
                success = false;
            }
        }
        Debug.Log($"Finished loading room {room.name}");
        return success;
    }

    public async UniTask<bool> SaveRoom(Room room)
    {
        var results = await UniTask.WhenAll(RoomTextures().Select(t => t.SaveRoom(room, _savePath.FullSaveDirectoryPath)));
        return results.All(t => t);
    }

    [Serializable]
    public class RoomTextureJsonData
    {
        [SerializeField] public List<TileBase> tiles;
    }
    [Serializable]
    public class RoomTexture
    {
       [SerializeField] private BuildingLayers _layer;
       [SerializeField] private BoundsInt _bounds;
       [SerializeField] private List<TileBase> _tiles;

       public RoomTexture(Room room, BuildingLayers layer)
        {
            _layer = layer;
            var building = room.Building;
            var map = building.Map;
            _bounds = map.GetCellsForRoom(room, layer);
            _tiles = new List<TileBase>();
        }

       public async UniTask<bool> SaveRoom(Room room, string saveDirectory)
       {
           var map = room.Building.Map;
           var texture = new Texture2D(_bounds.size.x, _bounds.size.y);
           Color[] pixels = new Color[_bounds.size.x * _bounds.size.y];
           for (int x = _bounds.xMin; x < _bounds.xMax; x++)
           {
               for (int y = _bounds.yMin; y < _bounds.yMax; y++)
               {
                   var pos = new Vector3Int(x, y, 0);
                   var tile = map.GetTile<EditableTile>(pos, _layer);
                   var index = (x - _bounds.xMin) + (y - _bounds.yMin) * _bounds.size.x;
                   pixels[index] = tile == null ? Color.black : Color.white;
                   if (tile != null) _tiles.Add(tile);
               }
           }
           texture.SetPixels(pixels);
           texture.Apply();
           byte[] pngData = texture.EncodeToPNG();
           string filePath = Path.Combine(saveDirectory, $"{room.name}_{_layer}.png");
           await File.WriteAllBytesAsync(filePath, pngData);
           var jsonData = new RoomTextureJsonData() {
               tiles = _tiles
           };
           string jsonFilePath = Path.Combine(saveDirectory, $"{room.name}_{_layer}.json");
           await File.WriteAllTextAsync(jsonFilePath, JsonUtility.ToJson(jsonData));
           Debug.Log($"Saved {filePath}");
           return true;
       }

       public async UniTask<bool> LoadRoom(Room room, string saveDirectory)
       {
            string filePath = Path.Combine(saveDirectory, $"{room.name}_{_layer}.png");
            if (File.Exists(filePath) == false) return false;
            Debug.Log($"Loading {filePath}"); 
            string jsonFilePath = Path.Combine(saveDirectory, $"{room.name}_{_layer}.json");
            string jsonString = await File.ReadAllTextAsync(jsonFilePath);
            var jsonData = JsonUtility.FromJson<RoomTextureJsonData>(jsonString);
            this._tiles = jsonData.tiles;
            byte[] imageData = await File.ReadAllBytesAsync(filePath);
            Debug.Log($"Finished reading from {filePath}"); 
            Texture2D texture = new Texture2D(_bounds.size.x, _bounds.size.y);
            if (!texture.LoadImage(imageData))
            {
                Debug.LogError($"Failed to load image data: {filePath}");
                return false;
            }
            texture.Apply();
            Color[] pixels = texture.GetPixels();
            int nextTile = 0;
            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                for (int y = _bounds.yMin; y < _bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var index = (x - _bounds.xMin) + (y - _bounds.yMin) * _bounds.size.x;
                    var pixel = pixels[index];
                    if (pixel != Color.black)
                    {
                        try
                        {
                            var tile = _tiles[nextTile++];
                            room.Building.Map.SetTile(pos, _layer, tile as EditableTile);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Failed to load tile at {pos} {e}");
                            return false;
                        }
                    }
                    else
                    {
                        room.Building.Map.SetTile(pos, _layer, null);
                    }
                }
            }
            Debug.Log($"Finished applying from {filePath}"); 
            return true;
       }
    }
}
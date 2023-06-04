using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buildings;
using Buildings.Rooms;
using Buildings.Tiles;
using CoreLib;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using Debug = UnityEngine.Debug;

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

    IEnumerable<Room> GetAllSavedRooms() => _building.Rooms.AllRooms.Where(t => t.buildLevel != BuildLevel.NONE);
    IEnumerable<RoomTilemapTextures> GetAllSavedRoomTextures() => GetAllSavedRooms().Select(t => _factory.Create(t));
    public async UniTask<bool> LoadGame()
    {
        var textures = GetAllSavedRoomTextures().ToList();
#if LOAD_PARALLEL
        return await LoadInParallel(textures);
#else
        return await LoadInSequence(textures);
#endif
    }

    #region [Load Functions]

    private async UniTask<bool> LoadInParallel(List<RoomTilemapTextures> textures)
    {
        var loadOps = from tex in textures select tex.LoadRoom(tex._room);
        var results = await UniTask.WhenAll(loadOps);
        var allSuccess = true;
        for (int i = 0; i < results.Length; i++)
        {
            if (!results[i])
            {
                allSuccess = false;
                Debug.Log($"Failed to load room {textures[i]._room.name}".ColoredRed(),textures[i]._room);
            }
        }
        return allSuccess;
    }
    private async UniTask<bool> LoadInSequence(List<RoomTilemapTextures> textures)
    {
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

    #endregion

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
    private readonly RoomTexture.Factory _roomTextureFactory;
    private readonly TexSaveLoadFactory _texSaveLoadFactory;

    private RoomTexture solidTexture;
    private RoomTexture pipeTexture;
    private RoomTexture wallTexture;
    private RoomTexture wireTexture;

    public RoomTilemapTextures(Room room, GlobalSavePath savePath, RoomTexture.Factory roomTextureFactory)
    {
        _room = room;
        _savePath = savePath;
        _roomTextureFactory = roomTextureFactory;
        var building = room.Building;
        var map = building.Map;
        solidTexture =_roomTextureFactory.Create(room, BuildingLayers.SOLID);
        pipeTexture = _roomTextureFactory.Create(room, BuildingLayers.PIPE);
        wallTexture = _roomTextureFactory.Create(room, BuildingLayers.WALL);
        wireTexture = _roomTextureFactory.Create(room, BuildingLayers.WIRES);
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
        var loadTasks = from roomTex in RoomTextures() 
                                            select roomTex.LoadRoom(room, _savePath.FullSaveDirectoryPath);
        var results  = await UniTask.WhenAll(loadTasks);
        success = results.All(t => t);
        return success;
    }

    public async UniTask<bool> SaveRoom(Room room)
    {
        var saveTasks = from roomTex in RoomTextures() 
                                           select roomTex.SaveRoom(room, _savePath.FullSaveDirectoryPath);
        var results = await UniTask.WhenAll(saveTasks);
        return results.All(t => t);
    }


    [Serializable]
    public class RoomTexture
    {
        private readonly TileAssets _tileAssets;
        [SerializeField] private BuildingLayers _layer;
        [SerializeField] private BoundsInt _bounds;
        [SerializeField] private List<TileBase> _tiles;
        private readonly IRoomTilemapTextureSaveLoader _saveLoader;

        //used for debugging load times
        private Stopwatch _operationTimer;

        public RoomTexture(Room room, TileAssets tileAssets, BuildingLayers layer,
            TexSaveLoadFactory texSaveLoadFactory)
        {
            _tileAssets = tileAssets;
            _layer = layer;
            _saveLoader = texSaveLoadFactory.Create(layer);
            Debug.Assert(_saveLoader != null);
            var building = room.Building;
            var map = building.Map;
            _bounds = map.GetCellsForRoom(room, layer);
            _tiles = new List<TileBase>();
        }

        public async UniTask<bool> SaveRoom(Room room, string saveDirectory)
        {
            Debug.Assert(_saveLoader != null);
            LogOperationStarted(room, false);
            var texture = GetTextureSaveData(room, out _tiles);
            var pngData = texture.EncodeToPNG();
            var jsonData = new JsonData() { tiles = _tiles };

            string filePath, jsonFilePath;
            GetFilePaths(saveDirectory, room.name, out filePath, out jsonFilePath, false);

            var task1 = File.WriteAllBytesAsync(filePath, pngData);
            var task2 = File.WriteAllTextAsync(jsonFilePath, JsonUtility.ToJson(jsonData));

            await task1;
            await task2;
            LogOperationFinished(room, false, true);
            return true;
        }

        public async UniTask<bool> LoadRoom(Room room, string saveDirectory)
        {
            Debug.Assert(_saveLoader != null);
            LogOperationStarted(room, true);
            string filePath, jsonFilePath;
            if (!GetFilePaths(saveDirectory, room.name, out filePath, out jsonFilePath, true))
            {
                LogFailureReason(room, true,
                    $"Failed to get file paths for {saveDirectory}\nimage path:{filePath}\njson path: {jsonFilePath}");
                return false;
            }

            await LoadTilesFromJson(jsonFilePath);
            var texture = await LoadTextureFromPath(filePath);
            if (texture == null)
            {
                LogFailureReason(room, true, $"Failed to load texture from path {filePath}");
                return false;
            }

            var roomTextures = GetRoomTextures(room);
            roomTextures.AssignTexture(_layer, texture);

            return LoadData(room, texture, filePath);
        }

        TileBase GetDefaultTile()
        {
            return _tileAssets.GetDefaultTile(_layer);
        }


    #region [SAVE/LOAD HELPERS]

        RoomTextures GetRoomTextures(Room room)
        {
            var roomTextures = room.GetComponent<RoomTextures>();
            if(roomTextures == null) roomTextures = room.gameObject.AddComponent<RoomTextures>();
            return roomTextures;
        }
        
        public bool GetFilePaths(string saveDirectory, string roomName, out string pngFilePath, out string jsonFilePath, bool fileMustExist)
        {
            pngFilePath = Path.Combine(saveDirectory, $"{roomName}_{_layer}.png");
            jsonFilePath = Path.Combine(saveDirectory, $"{roomName}_{_layer}.json");
            if(fileMustExist == false) return true;
            return File.Exists(pngFilePath) && File.Exists(jsonFilePath);
        }


        #region [DEBUG HELPERS]

        private void StartTimer()
        {
            _operationTimer = new Stopwatch();
            _operationTimer.Start();
        }

        private void EndTimer()
        {
            _operationTimer.Stop();
        }

        private void LogFailureReason(Room room, bool isLoad, string reason) => Debug.LogError($"Failed to {(isLoad ? "Load" : "Save")} room {room.name}\n {reason.Bolded()}");

        private void LogOperationFinished(Room room, bool isLoad, bool successful)
        {
            EndTimer();
            LogOperationMsg(room, isLoad, $"{(successful ? "Successfully" : "Unsuccessfully")} Completed in {_operationTimer.ElapsedMilliseconds} ms");
        }

        private void LogOperationStarted(Room room, bool isLoad)
        {
            StartTimer();
            LogOperationMsg(room, isLoad, "Starting");
        }

        private void LogOperationMsg(Room room, bool isLoad, string msg) => Debug.Log($"{msg} {(isLoad ? "Load" : "Save").Bolded() } of room {room.name}");

            #endregion

        #region [SAVE HELPERS]

        Texture2D GetTextureSaveData(Room room, out List<TileBase> tiles)
        {
            tiles = new List<TileBase>();
            var texture = new Texture2D(_bounds.size.x, _bounds.size.y);
            Color[] pixels = new Color[_bounds.size.x * _bounds.size.y];
            foreach (var tup in GetPositionsAndTilesFromRoom(room))
            {
                var color = GetColorForSaveData(tup.cell, tup.tile);
                pixels[tup.pixIndex] = color;
                if (tup.tile != null) tiles.Add(tup.tile);
            }
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        IEnumerable<(Vector3Int cell, int pixIndex, TileBase tile)> GetPositionsAndTilesFromRoom(Room room)
        {
            var map = room.Building.Map;
            var roomBounds = map.GetCellsForRoom(room, _layer);
            for (int x = roomBounds.xMin; x < roomBounds.xMax; x++)
            {
                for (int y = roomBounds.yMin; y < roomBounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var index = (x - roomBounds.xMin) + (y - roomBounds.yMin) * _bounds.size.x;
                    var tile = map.GetTile<TileBase>(pos, _layer);
                    yield return (pos, index, tile);
                }
            }
        }

        Color GetColorForSaveData(Vector3Int pos, TileBase tile)
        {
            var color = tile == null ? Color.clear : Color.white;
            var prevColor = color;
            _saveLoader.SavePositionDataToColor(pos, ref color);
            if (color != prevColor) Debug.Log($"Color changed to {color} by save loader for layer: {_layer}");
            return color;
        }

        #endregion
        
        #region [LOAD HELPERS]

        private bool HasTile(Color color) => !(color.a < 0.001f);

        private bool LoadData(Room room, Texture2D texture, string filePath)
        {
            Color[] pixels = texture.GetPixels();
            int nextTile = 0;
            bool failed = false;
            foreach (var tup in GetCellsAndPixels(pixels))
            {
                if(!LoadSaveDataAtCell(room, ref nextTile, tup.cell, tup.pixel)) failed = true;
            }
            if(!failed)
                Debug.Log($"Finished applying from {filePath}");
            else
                LogFailureReason(room, true,
                    $"Number of tiles in json ({_tiles.Count} does not match number of tiles found in texture ({nextTile})");
            LogOperationFinished(room, true, !failed);
            return !failed;
        }
       

        private bool LoadSaveDataAtCell(Room room, ref int nextTile, Vector3Int cell, Color pixel)
        {
            if (HasTile(pixel))
            {
                try
                {
                    var tile = _tiles[nextTile++];
                    Debug.Assert(_saveLoader != null);
                    Debug.Assert(room.Building != null);
                    Debug.Assert(room.Building.Map != null);
                    if (tile == null) tile = GetDefaultTile();
                    Debug.Assert(tile != null);
                    room.Building.Map.SetTile(cell, _layer, tile);
                    _saveLoader.SetTile(cell, pixel, tile);
                    Debug.Log($"Loaded tile {tile.name} in {room.name}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load tile at {cell} {e}");
                    return false;
                }
            }
            else
            {
                _saveLoader.SetEmpty(cell, pixel);
                room.Building.Map.SetTile(cell, _layer, null);
            }
            return true;
        }

        private IEnumerable<(Vector3Int cell, Color pixel)> GetCellsAndPixels(Color[] pixelData) => GetCellsAndPixels().Select(t => (t.cell, pixelData[t.pixelIndex]));

        private IEnumerable<(Vector3Int cell, int pixelIndex)> GetCellsAndPixels()
        {
            for (int x = _bounds.xMin; x < _bounds.xMax; x++)
            {
                for (int y = _bounds.yMin; y < _bounds.yMax; y++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var index = (x - _bounds.xMin) + (y - _bounds.yMin) * _bounds.size.x;
                    yield return (pos, index);
                }
            }
        }

        private async UniTask LoadTilesFromJson(string jsonFilePath)
        {
            string jsonString = await File.ReadAllTextAsync(jsonFilePath);
            var jsonData = JsonUtility.FromJson<JsonData>(jsonString);
            this._tiles = jsonData.tiles;
        }

        private async UniTask<Texture2D> LoadTextureFromPath(string imageFilePath)
        {
            byte[] imageData = await File.ReadAllBytesAsync(imageFilePath);
            Debug.Log($"Finished reading from {imageFilePath}"); 
            Texture2D texture = new Texture2D(_bounds.size.x, _bounds.size.y);
            if (!texture.LoadImage(imageData))
            {
                Debug.LogError($"Failed to load image data: {imageFilePath}");
                return  null;
            }
            texture.Apply();
            return texture;
        }

        #endregion
        
        #endregion



        public class Factory : PlaceholderFactory<Room, BuildingLayers, RoomTexture>
        {
            
        }
        
        [Serializable]
        public class JsonData
        {
            [SerializeField] public List<TileBase> tiles;
        }
    }

    public class Factory : PlaceholderFactory<Room, RoomTilemapTextures> { }
}
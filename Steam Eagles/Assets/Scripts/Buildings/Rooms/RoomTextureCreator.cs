using System;
using System.Collections.Generic;
using System.Linq;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using Codice.CM.Common;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Rooms
{
    public class RoomTextureCreator
    {
        /// <summary> structure used to pass data to compute shader </summary>
        public struct TilemapData
        {
            public Vector2Int position;
            public float value;

            public TilemapData(Vector2Int position, float value)
            {
                this.position = position;
                this.value = value;
            }
            
            public static int Stride() => sizeof(float) + (sizeof(int) * 2);
        }
        private readonly RoomState _roomState;
        private BoundsLookup _boundsLookup;
        private Dictionary<BuildingLayers, Texture> _textureCache = new();
        private Subject<BuildingTile> _onTileSet = new();
        private BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(_roomState.Room);
        
        public bool ProfileTime { get; set; }
        
        public RoomTextureCreator(RoomState roomState)
        {
            _roomState = roomState;
        }

        public Texture CreateTextureFor(BuildingLayers layer)
        {
            if (_textureCache.ContainsKey(layer) && _textureCache[layer] != null)
            {
                return _textureCache[layer];
            }
            _textureCache.Remove(layer);
            var size = BoundsLookup.GetBounds(layer).size;
            var texture = new RenderTexture(size.x, size.y, 0);
            texture.enableRandomWrite = true;
            texture.filterMode = FilterMode.Point;
            texture.Create();
            _textureCache.Add(layer, texture);
            return texture;
        }

        
        static IEnumerable<TilemapData> GetTilemapData(
            BoundsInt bounds, BuildingLayers layers, 
            Func<BuildingCell, BuildingTile> tileGetter, 
            Predicate<BuildingTile> useTile,
            Func<BuildingTile, float> valueGetter)
        {
            
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cellPos = new Vector3Int(x, y);
                    var texel = new Vector2Int(x - bounds.xMin, y - bounds.yMin);
                    var cell = new BuildingCell(cellPos, layers);
                    var tile = tileGetter(cell);
                    if (useTile(tile))
                    {
                        var value = valueGetter(tile);
                        yield return new TilemapData(texel, value);
                    }
                }
            }
        }
        public  IEnumerable<TilemapData> GetTilemapData_Arbitrary(
            IEnumerable<BuildingCell> buildingCells, BuildingLayers layer, 
            Func<BuildingCell, BuildingTile> tileGetter, 
            Predicate<BuildingTile> useTile,
            Func<BuildingTile, float> valueGetter)
        {

            foreach (var buildingCell in buildingCells)
            {
                var bounds = BoundsLookup.GetBounds(buildingCell.layers);
                var cellPos = buildingCell.cell2D;
                var texel = new Vector2Int(cellPos.x - bounds.xMin, cellPos.y - bounds.yMin);
         
                var cell = new BuildingCell(cellPos, layer);
                var tile = tileGetter(cell);
              
                var value = valueGetter(tile);
                yield return new TilemapData(texel, value);
                
            }
        }

        static IEnumerable<TilemapData> GetTilemapDataMultipleLayer(
            BoundsInt bounds, BuildingLayers[] layers, 
            Func<BuildingCell, BuildingTile> tileGetter, 
            Predicate<BuildingTile> useTile,
            Func<BuildingTile, float> valueGetter)
        {
            
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cellPos = new Vector3Int(x, y);
                    var texel = new Vector2Int(x - bounds.xMin, y - bounds.yMin);
                    foreach (var layer in layers)
                    {
                        var cell = new BuildingCell(cellPos, layer);
                        var tile = tileGetter(cell);
                        if (useTile(tile))
                        {
                            var value = valueGetter(tile);
                            yield return new TilemapData(texel, value);
                            break;
                        }
                    }
                }
            }
        }
         public  IEnumerable<TilemapData> GetTilemapDataMultipleLayer_Arbitrary(
             IEnumerable<BuildingCell> buildingCells, BuildingLayers[] layers, 
             Func<BuildingCell, BuildingTile> tileGetter, 
             Predicate<BuildingTile> useTile,
             Func<BuildingTile, float> valueGetter)
         {

             foreach (var buildingCell in buildingCells)
             {
                 var bounds = BoundsLookup.GetBounds(buildingCell.layers);
                 var cellPos = buildingCell.cell2D;
                 var texel = new Vector2Int(cellPos.x - bounds.xMin, cellPos.y - bounds.yMin);
                 foreach (var layer in layers)
                 {
                     var cell = new BuildingCell(cellPos, layer);
                     var tile = tileGetter(cell);
                     if (useTile(tile))
                     {
                         var value = valueGetter(tile);
                         yield return new TilemapData(texel, value);
                         break;
                     }
                 }
             }
         }

        private Func<BuildingCell, BuildingTile> TileGetter => _roomState.Room.Building.Map.GetBuildingTile;
        private Predicate<BuildingTile> UseTileDefault => tile => tile.tile != null;
        private Func<BuildingTile, float> ValueGetterDefault => tile => (tile.tile == null ? 0 : 1f);

        bool UseWallTile(BuildingTile tile)
        {
            if (tile.tile is DamagedWallTile)
                return true;
            return false;
        }
        float GetWallTileValue(BuildingTile tile) => 0.5f;

        float GetGasTileValue(BuildingTile tile) => 1;

        public Texture CopyFromMultipleTilemaps(bool recreate, params BuildingLayers[] layers)
        {
            BuildingLayers combinedLayers = 0;
            foreach (var layer in layers) combinedLayers |= layer;
            if (_textureCache.ContainsKey(combinedLayers))
            {
                
                _textureCache.Remove(combinedLayers);
            }
            var texture = (RenderTexture) CreateTextureFor(combinedLayers);
            var bounds = BoundsLookup.GetBounds(layers[0]);
            for (int i = 1; i < layers.Length; i++)
            {
                var lb = BoundsLookup.GetBounds(layers[i]);
                Debug.Assert(lb == bounds, "Bounds Mismatch");
            }
            var data = GetTilemapDataMultipleLayer(bounds, layers, TileGetter, UseTileDefault, ValueGetterDefault).ToList();
            if (data.Count == 0)
                return texture;
            data.Add(data[0]);
            WriteTilemapDataToTexture(texture, data.ToArray());
            return texture;
        }

        (Func<BuildingCell, BuildingTile>, Predicate<BuildingTile>, Func<BuildingTile, float>) getTileGetter(BuildingLayers layers)
        {
            switch (layers)
            {
                case BuildingLayers.WALL:
                   return (TileGetter,  UseWallTile, GetWallTileValue);
                    break;
                case BuildingLayers.GAS:
                    return (TileGetter, UseTileDefault, GetGasTileValue);
                    break;
                case BuildingLayers.FOUNDATION:
                case BuildingLayers.SOLID:
                case BuildingLayers.PIPE:
                case BuildingLayers.WIRES:
                case BuildingLayers.PLATFORM:
                case BuildingLayers.LADDERS:
                case BuildingLayers.COVER:
                case BuildingLayers.DECOR:
                    return (TileGetter, UseTileDefault, ValueGetterDefault);
                    break;
                case BuildingLayers.NONE:
                case BuildingLayers.REQUIRED:
                default:
                    throw new ArgumentOutOfRangeException(nameof(layers), layers, null);
            }
        }
        public Texture CopyFromTilemap(BuildingLayers layers, bool recreate= false)
        {
            if (recreate && _textureCache.ContainsKey(layers))
            {
                _textureCache.Remove(layers);
            }
            
            var bounds = BoundsLookup.GetBounds(layers); 
            var map = _roomState.Room.Building.Map;

            List<TilemapData> tilemapData = new List<TilemapData>();
            var (tileGetter, useTile, valueGetter) = getTileGetter(layers);
            tilemapData.AddRange(GetTilemapData(bounds, layers, tileGetter, useTile, valueGetter));

            var texture = (RenderTexture) CreateTextureFor(layers);
            if (tilemapData.Count == 0) return texture;
            var tile0 = tilemapData[0];
            tilemapData.Add(tile0);
            
            // for (int x = bounds.xMin; x < bounds.xMax; x++)
            // {
            //     for (int y = bounds.yMin; y < bounds.yMax; y++)
            //     {
            //         var cell = new Vector3Int(x, y);
            //         var texel = new Vector2Int(x - bounds.xMin, y - bounds.yMin);
            //         
            //         
            //         var tile = map.GetTile(cell, layers);
            //         OnSet(cell, tile);
            //         if (ShouldIncludeTile(tile))
            //         {
            //             var color = GetColorForTile(tile, layers);
            //             var value = GetValueForTile(tile, layers);
            //             tmp.SetPixel(texel.x, texel.y, color);
            //             tilemapData.Add(new TilemapData(texel, value));
            //             Debug.Log($"{layers} Cell: {cell} texel: {texel}");
            //         }
            //     }   
            // }
            //
            // void OnSet(Vector3Int cell, TileBase tile)
            // {
            //     var index = (cell.x - bounds.xMin) + (cell.y - bounds.yMin) * bounds.size.x;
            //     var texX = index % bounds.size.x;
            //     var texY = index / bounds.size.y;
            //     tmp.SetPixel(texX,texY, tile == null ? Color.black : Color.white);
            //     
            // }



          
           
            
            var data = tilemapData.ToArray();

            WriteTilemapDataToTexture(texture, data);
         
            return texture;
        }

        public void WriteArbitraryData(IEnumerable<BuildingCell> buildingCells, BuildingLayers layers)
        {
            BuildingLayers combinedLayers = layers;
            var texture = (RenderTexture) CreateTextureFor(combinedLayers);
            WriteArbitraryData(texture, buildingCells, layers);
        }
        
        public void WriteArbitraryData(RenderTexture texture, IEnumerable<BuildingCell> buildingCells, BuildingLayers layers)
        {
            BuildingLayers combinedLayers = layers;
            
            var (tileGetter, useTile, valueGetter) = getTileGetter(layers);
            
            List<TilemapData> tilemapData = new List<TilemapData>();
            tilemapData.AddRange(GetTilemapData_Arbitrary(buildingCells, layers, tileGetter, useTile, valueGetter));
            
            if (tilemapData.Count == 0) return;
            var tile0 = tilemapData[0];
            tilemapData.Add(tile0);
            WriteTilemapDataToTexture(texture, tilemapData.ToArray());
        }

        public RenderTexture WriteArbitraryData(IEnumerable<BuildingCell> buildingCells, BuildingLayers[] layers)
        {
            BuildingLayers combinedLayers = 0;
            foreach (var layer in layers) combinedLayers |= layer;
            var texture = (RenderTexture) CreateTextureFor(combinedLayers);
            var data = GetTilemapDataMultipleLayer_Arbitrary(buildingCells, layers, TileGetter, UseTileDefault, ValueGetterDefault).ToList();
            if (data.Count == 0)
                return texture;
            data.Add(data[0]);
            WriteTilemapDataToTexture(texture, data.ToArray());
            return texture;
        }

        public static void WriteTilemapDataToTexture(RenderTexture texture, TilemapData[] data)
        {
            var computeShader = BufferToTexCompute.BufferToTexComputeShader;
            var stride = TilemapData.Stride();
            var kernel = computeShader.FindKernel("CopyData");
            
            var computeBuffer = new ComputeBuffer(data.Length, stride);
            computeBuffer.SetData(data);
            
            computeShader.SetBuffer(kernel, "inputData", computeBuffer);
            computeShader.SetTexture(kernel, "Result", texture);
            
            int threadsX = data.Length % 8 == 0 ? data.Length / (int)8 : (data.Length / (int)8) + 1;
            if(threadsX == 0)threadsX = 1;
            int threadsY = 1;
            int threadsZ = 1;
            
            computeShader.Dispatch(kernel, threadsX, threadsY, threadsZ);   
            computeBuffer.Release();
        }

        bool ShouldIncludeTile(TileBase tileBase)
        {
            if (tileBase is DamagedWallTile)
                return false;
            return tileBase != null;
        }

        float GetValueForTile(TileBase tile, BuildingLayers layers)
        {
            if (layers == BuildingLayers.WALL)
            {
                if (tile is DamagedWallTile)
                {
                    return 0;
                }
                return 1;
            }
            else
            {
                if (tile == null)
                {
                    return 0;
                }

                return 1;
            }
        }
        Color GetColorForTile(TileBase tile, BuildingLayers layers)
        {
            if (layers == BuildingLayers.WALL)
            {
                if (tile is DamagedWallTile)
                {
                    return Color.black;
                }
                return Color.white;
            }
            else
            {
                if (tile == null)
                {
                    return Color.black;
                }
                return Color.white;
            }
        }
    }
}
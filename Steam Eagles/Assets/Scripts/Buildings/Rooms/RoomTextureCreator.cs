using System.Collections.Generic;
using Buildings.Rooms.Tracking;
using Buildings.Tiles;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Rooms
{
    public class RoomTextureCreator
    {
        private readonly RoomState _roomState;
        private BoundsLookup _boundsLookup;
        private Dictionary<BuildingLayers, Texture> _textureCache = new();
        private BoundsLookup BoundsLookup => _boundsLookup ??= new BoundsLookup(_roomState.Room);
        
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

        struct TilemapData
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

      
        public Texture CopyFromTilemap(BuildingLayers layers, bool recreate= false)
        {
            if (recreate && _textureCache.ContainsKey(layers))
            {
                _textureCache.Remove(layers);
            }
            var texture = (RenderTexture) CreateTextureFor(layers);
            var bounds = BoundsLookup.GetBounds(layers); 
            var map = _roomState.Room.Building.Map;
            
            
            
            List<TilemapData> tilemapData = new List<TilemapData>();
            Texture2D tmp = new Texture2D(bounds.size.x, bounds.size.y);
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var cell = new Vector3Int(x, y);
                    var texel = new Vector2Int(x - bounds.xMin, y - bounds.yMin);
                    
                    
                    var tile = map.GetTile(cell, layers);
                    OnSet(cell, tile);
                    if (ShouldIncludeTile(tile))
                    {
                        var color = GetColorForTile(tile, layers);
                        var value = GetValueForTile(tile, layers);
                        tmp.SetPixel(texel.x, texel.y, color);
                        tilemapData.Add(new TilemapData(texel, value));
                        Debug.Log($"{layers} Cell: {cell} texel: {texel}");
                    }
                }   
            }

            void OnSet(Vector3Int cell, TileBase tile)
            {
                var index = (cell.x - bounds.xMin) + (cell.y - bounds.yMin) * bounds.size.x;
                var texX = index % bounds.size.x;
                var texY = index / bounds.size.y;
                tmp.SetPixel(texX,texY, tile == null ? Color.black : Color.white);
                
            }



            if (tilemapData.Count == 0)
            {
                return texture;
            }
            var tile0 = tilemapData[0];
            tilemapData.Add(tile0);
            var computeShader = BufferToTexCompute.BufferToTexComputeShader;
            var data = tilemapData.ToArray();
            var stride = TilemapData.Stride();
            var kernel = computeShader.FindKernel("CopyData");
            var computeBuffer = new ComputeBuffer(data.Length, stride);
            computeBuffer.SetData(data);
            computeShader.SetBuffer(kernel, "inputData", computeBuffer);
            computeShader.SetTexture(kernel, "Result", texture);
            
            int threadsX = data.Length % 8 == 0 ? data.Length / (int)8 : (data.Length / (int)8) + 1;
            if(threadsX == 0)threadsX = 1;
            int threadsY = 1;// Mathf.CeilToInt(size.y / 8f);
            int threadsZ = 1;
            computeShader.Dispatch(kernel, threadsX, threadsY, threadsZ);   
            computeBuffer.Release();
            return texture;
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
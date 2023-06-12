using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Buildings;
using CoreLib;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;

public interface IRoomTilemapTextureSaveLoader
{
    void SetEmpty(Vector3Int pos, Color pixel);
    void SetTile(Vector3Int pos, Color pixel, TileBase tile);
    void SavePositionDataToColor(Vector3Int pos, ref Color color);
}

public class TexSaveLoadFactory : PlaceholderFactory<BuildingLayers, IRoomTilemapTextureSaveLoader>
{
    
}
public class TexSaveLoadFactoryImpl : IFactory<BuildingLayers, IRoomTilemapTextureSaveLoader>
{
    private readonly IRoomTilemapTextureSaveLoader _nullTextureSaveLoader;
    private readonly Dictionary<BuildingLayers, CompositeRoomTilemapTextureSaveLoader> _layerSpecificRoomTexSaveLoaders;

    public TexSaveLoadFactoryImpl(List<ILayerSpecificRoomTexSaveLoader> layerSpecificSaveLoaders)
    {
        _nullTextureSaveLoader = new NullTextureSaveLoader();
        _layerSpecificRoomTexSaveLoaders = new Dictionary<BuildingLayers, CompositeRoomTilemapTextureSaveLoader>();
        foreach (var layerSpecificRoomTex in layerSpecificSaveLoaders)
        {
            var layer = layerSpecificRoomTex.TargetLayer;
            if (!_layerSpecificRoomTexSaveLoaders.TryGetValue(layerSpecificRoomTex.TargetLayer,
                    out var compositeRoomTilemapTextureSaveLoader))
            {
                
                _layerSpecificRoomTexSaveLoaders.Add(layerSpecificRoomTex.TargetLayer, compositeRoomTilemapTextureSaveLoader=new CompositeRoomTilemapTextureSaveLoader(layer));
            }
            compositeRoomTilemapTextureSaveLoader.textureSaveLoaders.Add(layerSpecificRoomTex);
        }
        StringBuilder sb = new StringBuilder();
        foreach (var kvp in _layerSpecificRoomTexSaveLoaders)
        {
            sb.AppendLine(kvp.Value.ToString());
        }
        Debug.Log(sb.ToString());
    }

    public IRoomTilemapTextureSaveLoader Create(BuildingLayers param)
    {
        if (_layerSpecificRoomTexSaveLoaders.TryGetValue(param, out var loader))
        {
            return loader;
        }
        return _nullTextureSaveLoader;
    }

    class NullTextureSaveLoader : IRoomTilemapTextureSaveLoader
    {
        public void SetEmpty(Vector3Int pos, Color pixel)
        {
        }

        public void SetTile(Vector3Int pos, Color pixel, TileBase tile)
        {
            
        }

        public void SavePositionDataToColor(Vector3Int pos, ref Color color)
        {
            
        }
    }

    class CompositeRoomTilemapTextureSaveLoader : IRoomTilemapTextureSaveLoader
    {
        private readonly BuildingLayers _layer;
        public readonly List<IRoomTilemapTextureSaveLoader> textureSaveLoaders;

        public CompositeRoomTilemapTextureSaveLoader(BuildingLayers layer)
        {
            _layer = layer;
            this.textureSaveLoaders = new List<IRoomTilemapTextureSaveLoader>();
        }
        public CompositeRoomTilemapTextureSaveLoader(BuildingLayers layer, List<IRoomTilemapTextureSaveLoader> textureSaveLoaders)
        {
            _layer = layer;
            this.textureSaveLoaders = textureSaveLoaders;
        }

        public void SetEmpty(Vector3Int pos, Color pixel)
        {
            foreach (var roomTilemapTextureSaveLoader in textureSaveLoaders)
            {
                roomTilemapTextureSaveLoader.SetEmpty(pos, pixel);
            }
        }

        public void SetTile(Vector3Int pos, Color pixel, TileBase tile)
        {
            foreach (var roomTilemapTextureSaveLoader in textureSaveLoaders)
            {
                roomTilemapTextureSaveLoader.SetTile(pos, pixel, tile);
            }
        }

        public void SavePositionDataToColor(Vector3Int pos, ref Color color)
        {
            foreach (var roomTilemapTextureSaveLoader in textureSaveLoaders)
            {
                roomTilemapTextureSaveLoader.SavePositionDataToColor(pos, ref color);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Layer: ");
            sb.AppendLine(_layer.ToString().ToLowerInvariant().Bolded());
            sb.Append("SaveLoaders (Count= ");
            sb.Append(textureSaveLoaders.Count.ToString().Bolded());
            sb.Append("):\t");
            foreach (var specificRoomTexSaveLoader in textureSaveLoaders)
            {
                sb.Append(specificRoomTexSaveLoader.GetType().Name);
                sb.Append(", ");
            }
            return sb.ToString();
        }
    }
}



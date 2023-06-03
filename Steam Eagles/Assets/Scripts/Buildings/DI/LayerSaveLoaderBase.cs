using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.DI
{
    public abstract class LayerSaveLoaderBase : ILayerSpecificRoomTexSaveLoader
    {
        public abstract void SetEmpty(Vector3Int pos, Color pixel);

        public abstract void SetTile(Vector3Int pos, Color pixel, TileBase tile);

        public abstract void SavePositionDataToColor(Vector3Int pos, ref Color color);
        public abstract BuildingLayers TargetLayer { get; }
    }
}
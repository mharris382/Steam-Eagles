using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.DI
{
    public abstract class NullLayerSaveLoader : LayerSaveLoaderBase
    {
        public override void SetTile(Vector3Int pos, Color pixel, TileBase tile) { }
        public override void SavePositionDataToColor(Vector3Int pos, ref Color color) { }
        public override void SetEmpty(Vector3Int pos, Color pixel){ }
    }
}
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings
{
    public interface IBuildingTilemaps
    {
        public bool HasTile(Vector2Int cellPosition, BuildingLayers layers);
        public bool TryGetTile(Vector2Int cellPosition, BuildingLayers layers, out TileBase tile);
    }
}
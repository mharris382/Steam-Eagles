using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Rooms
{
    public interface ITileColorPicker
    {
        Color GetColorForTile( Vector3Int position,BuildingLayers layer, TileBase tile, Color prevColor);
    }
}
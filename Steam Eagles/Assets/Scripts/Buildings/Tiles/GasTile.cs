using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    [CreateAssetMenu(fileName = "New Gas Tile", menuName = "Steam Eagles/Tiles/Gas Tile", order = 0)]
    public class GasTile : Tile
    {
        [ColorUsage(true, true)]
        public Color gasTileColor = Color.red;
    }
}
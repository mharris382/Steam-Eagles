using UnityEngine;
using UnityEngine.Tilemaps;

namespace World.CustomTiles
{
    [CreateAssetMenu(fileName = "new variable tile", menuName = "2D/Tiles/New Variable Tile", order = 0)]
    public class VariableTile : Tile
    {
        [Header("Counting tile")]
        public Sprite[] sprites;
        public TileBase[] tilesToCheck;

        // public Sprite GetSprite(Vector3Int pos)
        // {
        //     
        // }
        public override void RefreshTile(Vector3Int position, ITilemap tilemap)
        {
            base.RefreshTile(position, tilemap);
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
        }
    }
}
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Buildings.Tiles
{
    public class PipeTile : EditableTile
    {
        public Vector4 scale = new Vector4(1, 1, 1, 1);
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            tileData.transform.SetTRS(tileData.transform.GetPosition(), tileData.transform.rotation, scale);
        }
        
    }
}
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PhysicsFun.Airships
{
    public interface ITilemapAttachment
    {
        void AttachToTilemap(Tilemap tm, Vector3Int cell);
        void Disconnect();
    }
  
}
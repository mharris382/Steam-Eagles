using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Spaces
{
    public class DynamicTilemapController : MonoBehaviour
    {
        
    }


    public static class TilemapExtensions
    {
        internal static DynamicTilemapController dynamicTilemapController;
        public static bool CanPlacePipe(this Tilemap tilemap, Vector3Int vector3Int)
        {
            if (dynamicTilemapController == null)
                return true;
            throw new NotImplementedException();
        }
    }
}
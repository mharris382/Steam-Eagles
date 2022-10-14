using System.Collections.Generic;
using CoreLib;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Tilemap Array", fileName = " Shared Tilemap Array",
        order = 0)]
    public class SharedTilemaps : SharedArray<Tilemap>
    {
        
    }
}
using CoreLib;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Block Data Array", fileName = " Shared Tilemap Array",
        order = 0)]
    public class SharedBlocks : SharedArray<BlockData>
    {
        
    }
}
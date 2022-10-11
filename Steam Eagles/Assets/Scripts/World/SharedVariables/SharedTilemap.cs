using CoreLib;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    [CreateAssetMenu(menuName = "Shared Variables/Create Shared Tilemap", fileName = " Shared Tilemap", order = 0)]
    public class SharedTilemap : SharedVariable<Tilemap> { }
}
using UnityEngine;
using World;

namespace Buildings.BuildingTilemaps
{
    public abstract class EditableTilemap : RenderedTilemap
    {
        [SerializeField]
        private BuildingLayers blockingLayers = BuildingLayers.FOUNDATION;
        public virtual BuildingLayers GetBlockingLayers() => blockingLayers;
    }
}
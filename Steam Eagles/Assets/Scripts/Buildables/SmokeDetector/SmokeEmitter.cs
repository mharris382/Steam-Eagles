using Buildables.Utilities;
using Buildings;
using UnityEngine.Tilemaps;

namespace Buildables.SmokeDetector
{
    public class SmokeEmitter : TileSetter
    {
        public override BuildingLayers TargetLayer => BuildingLayers.SOLID;


        protected override bool Validate(BuildingLayers layer, TileBase tile)
        {
            if (layer != BuildingLayers.GAS)
                return false;
            return base.Validate(layer, tile);
        }
    }
}
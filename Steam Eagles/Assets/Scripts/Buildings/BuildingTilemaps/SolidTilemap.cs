using System.Collections.Generic;
using PhysicsFun.Buildings;
using UnityEngine;
using World;

namespace Buildings.BuildingTilemaps
{
    public class SolidTilemap : DestructableTilemap
    {
        public const int ORDER_IN_LAYER = -2;
        public override BuildingLayers Layer => BuildingLayers.SOLID;
        public override string GetSaveID()
        {
            return "Solid";
        }

        public override BuildingLayers GetBlockingLayers()
        {
            return base.GetBlockingLayers() | 
                   BuildingLayers.FOUNDATION;
        }

        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer + ORDER_IN_LAYER;
        }

        public override IEnumerable<string> GetTileAddresses()
        {
            yield return "SolidTile";
            yield return "SolidTile_Angled";
        }
    }
}
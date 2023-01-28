using System;
using Buildings.BuildingTilemaps;
using PhysicsFun.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings
{
    
    [RequireComponent(typeof(TilemapCollider2D), typeof(PlatformEffector2D))]
    public class PlatformTilemap : EditableTilemap
    {
        private void OnValidate()
        {
            var tmc = GetComponent<TilemapCollider2D>();
            tmc.usedByEffector = true;
            
        }

        public override BuildingLayers Layer => BuildingLayers.PLATFORM;
        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer - SolidTilemap.ORDER_IN_LAYER - 1;
        }
    }
}
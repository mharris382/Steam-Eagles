using CoreLib;
using PhysicsFun.Buildings;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;

namespace Buildings.BuildingTilemaps
{
    [RequireComponent(typeof(TilemapCollider2D))]
    public class LadderTilemap : EditableTilemap, ILadderTilemap
    {
        public override BuildingLayers Layer => BuildingLayers.LADDERS;
        
        public override string GetSaveID()
        {
            return "Ladders";
        }
        
        public override int GetSortingOrder(Building building)
        {
            return building.orderInLayer + 10;
        }
    }
}
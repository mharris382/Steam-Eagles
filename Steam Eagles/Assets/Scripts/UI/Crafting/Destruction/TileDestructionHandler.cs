using System;
using Buildables;
using Buildings;
using Items;
using UnityEngine;
using Zenject;

namespace UI.Crafting.Destruction
{
    public class TileDestructionHandler : DestructionHandler
    {
        public class Factory : PlaceholderFactory<Recipe, TileDestructionHandler> { }
        public TileDestructionHandler(Recipe recipe, DestructionPreview destructionPreview) : base(recipe, destructionPreview)
        {
        }

        public override bool HasDestructionTarget(Building building, BuildingCell cell)
        {
            
            var hasTile = building.Map.GetTile(cell) != null;
            if (!hasTile)
                return false;
            var bmachines = building.GetComponent<BMachines>();
            var bmachine = bmachines.Map.GetMachine(cell.cell2D + Vector2Int.up);
            if (bmachine != null && bmachine.snapsToGround)
            {
                return false;
            }

            return true;
        }

        public override Vector2 GetDestructionTargetSize(Building building, BuildingCell cell)
        {
            return building.Map.GetCellSize(cell.layers);
        }

        public override void Destruct(Building building, BuildingCell cell)
        {
            building.Map.SetTile(cell, null);
        }
    }
}
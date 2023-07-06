using System;
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
            return building.Map.GetTile(cell) != null;
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
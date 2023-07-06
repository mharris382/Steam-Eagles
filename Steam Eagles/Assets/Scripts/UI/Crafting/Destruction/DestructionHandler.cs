using Buildings;
using Items;
using UnityEngine;
using Zenject;

namespace UI.Crafting.Destruction
{
    public abstract class DestructionHandler
    {
        private readonly DestructionPreview _destructionPreview;
        public Recipe Recipe { get; }

        public class Factory : PlaceholderFactory<Recipe, DestructionHandler> { }

        public DestructionHandler(Recipe recipe, DestructionPreview destructionPreview)
        {
            _destructionPreview = destructionPreview;
            Recipe = recipe;
        }

        public abstract bool HasDestructionTarget(Building building, BuildingCell cell);
        
        public abstract Vector2 GetDestructionTargetSize(Building building, BuildingCell cell);

        public void UpdateDestructionPreview(Building building, BuildingCell cell)
        {
            var hasTarget = HasDestructionTarget(building, cell);
            if (!hasTarget)
            {
                _destructionPreview.color = Color.clear;
                return;
            }
            else
            {
                _destructionPreview.color = CraftingPreviewResources.Instance.destructColor;
            }
            var size = GetDestructionTargetSize(building, cell);
            _destructionPreview.UpdateTarget(building, cell, size);
        }
        public abstract void Destruct(Building building, BuildingCell cell);
    }
}
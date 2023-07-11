using System;
using Buildings;
using Items;
using UI.Crafting.Events;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class TilePreview : RecipePreview<TileBase>
    {
        private readonly CraftingEventPublisher _eventPublisher;

        public class Factory : PlaceholderFactory<Recipe, TileBase, TilePreview> {}
        public TilePreview(Recipe recipe, TileBase loadedObject, CraftingEventPublisher eventPublisher) : base(recipe, loadedObject)
        {
            _eventPublisher = eventPublisher;
        }


        private SpriteRenderer _spriteRenderer;

        public override GameObject CreatePreviewFrom(Recipe recipe, TileBase loadedObject, Building building, BuildingCell aimedPosition)
        {
            var go = new GameObject("TilePreview");
            var sr = _spriteRenderer = go.AddComponent<SpriteRenderer>();
            if (loadedObject is RuleTile rt)
            {
                sr.sprite = rt.m_DefaultSprite;
            }

            go.transform.parent = building.transform;
            go.transform.position = building.Map.CellToWorldCentered(aimedPosition.cell, aimedPosition.layers);
            return go;
        }

        protected override void UpdatePreviewInternal(Building building, BuildingCell aimedPosition, bool isValid,
            bool flipped)
        {
            var go = PreviewObject;
            _spriteRenderer.color = isValid ? Resources.validColor : Resources.invalidColor;
            go.transform.position = building.Map.CellToWorldCentered(aimedPosition.cell, aimedPosition.layers);
            
            //publish preview feedback event
            _eventPublisher.OnTilePreview(building, aimedPosition, building.Map.GetTile(aimedPosition), LoadedObject);
        }

        public override void BuildFromPreview(Building building, BuildingCell gridPosition, bool isFlipped)
        {
            var tile = LoadedObject;
            var oldTile = building.Map.GetTile(gridPosition.cell, gridPosition.layers);
            if (oldTile != null)
            {
                _eventPublisher.OnTileSwapped(gridPosition, oldTile, tile);
            }
            else
            {
                _eventPublisher.OnTileBuilt(gridPosition, tile);
            }
            _eventPublisher.OnTileChanged(building, gridPosition, oldTile, tile);
            building.Map.SetTile(gridPosition.cell, gridPosition.layers, tile);
        }
    }
}
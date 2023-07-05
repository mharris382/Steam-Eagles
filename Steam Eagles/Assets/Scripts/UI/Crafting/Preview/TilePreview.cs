using System;
using Buildings;
using Items;
using UnityEngine;
using UnityEngine.Tilemaps;
using Zenject;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class TilePreview : RecipePreview<TileBase>
    {
        public class Factory : PlaceholderFactory<Recipe, TileBase, TilePreview> {}
        public TilePreview(Recipe recipe, TileBase loadedObject) : base(recipe, loadedObject) { }


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
        }

        public override void BuildFromPreview(Building building, BuildingCell gridPosition, bool isFlipped)
        {
            var tile = LoadedObject;
            building.Map.SetTile(gridPosition.cell, gridPosition.layers, tile);
        }
    }
}
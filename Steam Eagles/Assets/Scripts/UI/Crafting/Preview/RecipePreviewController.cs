using System;
using Buildings;
using CoreLib;
using Items;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class RecipePreviewController : IDisposable
    {
        private RecipePreview _currentPreview;

        public RecipePreviewController() { }

        private RecipePreview CreatePreviewFor(Recipe objNext, Object loadedObject)
        {
            switch (objNext.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    return new TilePreview(objNext, loadedObject as TileBase);
                    break;
                case Recipe.RecipeType.MACHINE:
                    return new PrefabPreview(objNext, loadedObject as GameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void UpdatePreview(Recipe recipe, Object loadedObject, Building building, BuildingCell aimedPosition, bool isValid)
        {
            if (_currentPreview != null)
            {
                if (_currentPreview.Recipe != recipe || _currentPreview.LoadedObject != loadedObject)
                {
                    _currentPreview.Dispose();
                    _currentPreview = null;
                }
            }

            _currentPreview ??= CreatePreviewFor(recipe, loadedObject);
            _currentPreview.UpdatePreview(building, aimedPosition, isValid);
        }


        public void Dispose()
        {
            _currentPreview?.Dispose();
        }

        public void BuildFromPreview(Building building, BuildingCell gridPosition)
        {
            if (_currentPreview != null)
            {
                _currentPreview.BuildFromPreview(building, gridPosition);
                _currentPreview.Dispose();
                _currentPreview = null;
            }
        }
    }
}
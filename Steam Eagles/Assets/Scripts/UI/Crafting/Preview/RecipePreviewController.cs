using System;
using Buildings;
using CoreLib;
using Items;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class RecipePreviewController : IDisposable
    {
        private readonly PrefabPreview.Factory _prefabPreviewFactory;
        private readonly TilePreview.Factory _tilePreviewFactory;
        private RecipePreview _currentPreview;
        private bool _isFlipped = false;

        public RecipePreviewController(PrefabPreview.Factory prefabPreviewFactory, TilePreview.Factory tilePreviewFactory)
        {
            _prefabPreviewFactory = prefabPreviewFactory;
            _tilePreviewFactory = tilePreviewFactory;
        }

        private RecipePreview CreatePreviewFor(Recipe objNext, Object loadedObject)
        {
            switch (objNext.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    return _tilePreviewFactory.Create(objNext, loadedObject as TileBase);
                    break;
                case Recipe.RecipeType.MACHINE:
                    return _prefabPreviewFactory.Create(objNext, loadedObject as GameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public void UpdatePreview(Recipe recipe, Object loadedObject, PlayerInput playerInput, Building building,
            BuildingCell aimedPosition, bool isValid)
        {
            if (_currentPreview != null)
            {
                if (_currentPreview.Recipe != recipe || _currentPreview.LoadedObject != loadedObject)
                {
                    _currentPreview.Dispose();
                    _currentPreview = null;
                }
            }

            var moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
            if (moveInput.x != 0) _isFlipped = moveInput.x < 0;


            _currentPreview ??= CreatePreviewFor(recipe, loadedObject);
            _currentPreview.UpdatePreview(building, aimedPosition, isValid, _isFlipped);
        }


        public void Dispose()
        {
            _currentPreview?.Dispose();
        }

        public void BuildFromPreview(Building building, BuildingCell gridPosition)
        {
            if (_currentPreview != null)
            {
                _currentPreview.BuildFromPreview(building, gridPosition, _isFlipped);
                _currentPreview.Dispose();
                _currentPreview = null;
            }
        }
    }
}
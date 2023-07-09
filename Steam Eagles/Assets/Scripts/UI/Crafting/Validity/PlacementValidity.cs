using System;
using Buildings;
using Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class PlacementValidity
    {
        private readonly UICrafting _crafting;

        private readonly PlacementValidityChecks _tileValidityChecks;
        private readonly PlacementValidityChecks _prefabValidityChecks;
        private string _errorMsg;
        
        public PlacementValidity(UICrafting crafting , TilePlacementValidityChecks tileValidityChecks, PrefabPlacementValidityChecks prefabValidityChecks)
        {
            _crafting = crafting;
            _tileValidityChecks = tileValidityChecks;
            _prefabValidityChecks = prefabValidityChecks;
        }

        public void UpdateValidity(Recipe recipe, Object loadedObject, GameObject character, Building building, BuildingCell cell)
        {
            IsValid = GetValidityChecks(recipe).IsPlacementValid(recipe, loadedObject, character, building, cell, ref _errorMsg);
        }

        PlacementValidityChecks GetValidityChecks(Recipe recipe)
        {
            switch (recipe.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    return _tileValidityChecks;
                    break;
                case Recipe.RecipeType.MACHINE:
                    return _prefabValidityChecks;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        

        public bool IsValid
        {
            get;
            set;
        }
        
    }
}
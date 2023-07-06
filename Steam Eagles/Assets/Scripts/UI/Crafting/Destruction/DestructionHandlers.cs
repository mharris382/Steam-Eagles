using System;
using Buildings;
using Items;
using UnityEngine.InputSystem;

namespace UI.Crafting.Destruction
{
    public class DestructionHandlers
    {
        
        private readonly DestructionPreview _destructionPreview;
        private readonly MachineDestructionHandler.Factory _machineDestructFactory;
        private readonly TileDestructionHandler.Factory _tileDestructionHandlerFactory;
        private DestructionHandler _currentHandler;
        private Recipe _currentRecipe;

        public DestructionHandlers(DestructionPreview destructionPreview, MachineDestructionHandler.Factory machineDestructFactory, TileDestructionHandler.Factory tileDestructionHandlerFactory)
        {
            
            _destructionPreview = destructionPreview;
            _machineDestructFactory = machineDestructFactory;
            _tileDestructionHandlerFactory = tileDestructionHandlerFactory;
        }
        public DestructionHandler GetDestructionHandlerFor(Recipe recipe)
        {
            if (_currentHandler != null)
            {
                if (recipe == _currentRecipe) return _currentHandler;
            }
            _currentRecipe = recipe;
            switch (_currentRecipe.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    _currentHandler = _tileDestructionHandlerFactory.Create(_currentRecipe);
                    break;
                case Recipe.RecipeType.MACHINE:
                    _currentHandler = _machineDestructFactory.Create(_currentRecipe);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return _currentHandler;
        }
        
        
        public void UpdateDestructionTarget(PlayerInput playerInput, Recipe recipe, Building building, BuildingCell cell)
        {
            var handler = GetDestructionHandlerFor(recipe);
            handler.UpdateDestructionPreview(building, cell);
            if (playerInput.actions["Ability Primary"].IsPressed())
            {
                handler.Destruct(building, cell);
            }
            _destructionPreview.UpdateTarget(building, cell, handler.GetDestructionTargetSize(building, cell));
        }
        
    }
}
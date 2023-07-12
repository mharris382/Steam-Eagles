using System;
using System.Collections.Generic;
using Buildings;
using Items;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class LoadHelper
    {
        private readonly Recipes _recipes;
        private readonly UICrafting _crafting;
        private Dictionary<Recipe, Object> _loadedObject = new Dictionary<Recipe, Object>();
        private readonly TileLoader _tileLoader;
        private readonly PrefabLoader _prefabLoader;


        public bool IsCurrentRecipeLoaded
        {
            get
            {
                var r = _recipes.CurrentRecipe?.Value;
                if (r == null) return false;
                return IsLoaded(r);
            }
        }

        public Object CurrentRecipeLoadedObject
        {
            get
            {
                var r = _recipes.CurrentRecipe?.Value;
                if(r == null) return null;
                return GetLoadedValue(r);
            }
        }

        public BuildingLayers CurrentTargetLayer
        {
            get
            {
                var r = _recipes.CurrentRecipe?.Value;
                if (r == null || !IsCurrentRecipeLoaded) return BuildingLayers.NONE;
                return r.GetBuildingLayer(CurrentRecipeLoadedObject);
            }
        }
        public LoadHelper(Recipes recipes, UICrafting crafting)
        {
            _recipes = recipes;
            _crafting = crafting;
            _tileLoader = new TileLoader(crafting);
            _prefabLoader = new PrefabLoader(crafting);
        }

        private Object GetLoadedValue(Recipe recipe)
        {
           return GetLoader(recipe).GetLoadedValue(recipe);
        }

        private bool IsLoaded(Recipe recipe)
        {
            LoadRecipe(recipe);
            return  GetLoader(recipe).IsLoaded(recipe);
        }

        LoaderWrapper GetLoader(Recipe recipe)
        {
            switch (recipe.GetRecipeType())
            {
                case Recipe.RecipeType.TILE:
                    return _tileLoader;
                case Recipe.RecipeType.MACHINE:
                    return _prefabLoader;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void LoadRecipe(Recipe recipe)
        {
            GetLoader(recipe).Load(recipe);
        }
        
        public abstract class LoaderWrapper
        {
            protected readonly UICrafting _crafting;
            public LoaderWrapper(UICrafting crafting) => _crafting = crafting;
            
            public abstract bool IsLoaded(Recipe recipe);
            public abstract void Load(Recipe recipe);
            public abstract Object GetLoadedValue(Recipe recipe);
        }
        public class TileLoader : LoaderWrapper<TileBase>
        {
            public TileLoader(UICrafting crafting) : base(crafting) { }
        }
        public class PrefabLoader : LoaderWrapper<GameObject>
        {
            public PrefabLoader(UICrafting crafting) : base(crafting)
            {
            }
        }
        public abstract class LoaderWrapper<T> : LoaderWrapper where T : Object
        {
            protected LoaderWrapper(UICrafting crafting) : base(crafting) { }

            public T GetLoadedObject(Recipe recipe)
            {
                return recipe.GetLoader<T>(_crafting).LoadedObject;
            }

            public override void Load(Recipe recipe)
            {
                recipe.GetLoader<T>(_crafting);
            }

            public override bool IsLoaded(Recipe recipe)
            {
                return recipe.GetLoader<T>(_crafting).IsLoaded;
            }

            public override Object GetLoadedValue(Recipe recipe)
            {
                return GetLoadedObject(recipe);
            }
        }
    }
    
}
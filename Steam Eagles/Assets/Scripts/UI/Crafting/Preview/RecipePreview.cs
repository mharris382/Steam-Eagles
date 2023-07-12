using System;
using Buildings;
using Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public abstract class RecipePreview : IDisposable
    {
        public Recipe Recipe { get; }
        public Object LoadedObject { get; }
        public CraftingPreviewResources Resources => CraftingPreviewResources.Instance; 
        
        private GameObject _previewObject;
        

        public GameObject PreviewObject
        {
            get => _previewObject;
            set => _previewObject = value;
        }
        public RecipePreview(Recipe recipe, Object loadedObject)
        {
            Recipe = recipe;
            LoadedObject = loadedObject;
        }


        public abstract GameObject CreatePreview(Recipe recipe, Object loadedObject, Building building, BuildingCell aimedPosition);

        public  void Dispose()
        {
            if(_previewObject != null)
                GameObject.Destroy(_previewObject);
        }


        public void UpdatePreview(Building building, BuildingCell aimedPosition, bool isValid, bool flipped)
        {
            if(_previewObject == null) _previewObject = CreatePreview(Recipe, LoadedObject, building, aimedPosition);
            UpdatePreviewInternal(building, aimedPosition, isValid ,flipped);
        }
        
        protected abstract void UpdatePreviewInternal(Building building, BuildingCell aimedPosition, bool isValid,
            bool flipped);

        public abstract void BuildFromPreview(Building building, BuildingCell gridPosition, bool isFlipped);
    }

    public abstract class RecipePreview<T> : RecipePreview where T : Object
    {
        public T LoadedObject { get; }

        public RecipePreview(Recipe recipe, T loadedObject) : base(recipe, loadedObject)
        {
            LoadedObject = loadedObject;
        }

        public override GameObject CreatePreview(Recipe recipe, Object loadedObject, Building building, BuildingCell aimedPosition)
        {
            return CreatePreviewFrom(recipe, (T)loadedObject , building, aimedPosition);
        }

        public abstract GameObject CreatePreviewFrom(Recipe recipe, T loadedObject, Building building,
            BuildingCell aimedPosition);
    }
}
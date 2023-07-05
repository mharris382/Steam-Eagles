using System;
using Buildables;
using Buildings;
using Items;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class PrefabPreview : RecipePreview<GameObject>
    {
        private readonly PrefabPreviewCache _cache;
        private PrefabPreviewWrapper _wrapper;
        private BuildableMachineBase _machinePrefab;

        public class Factory : PlaceholderFactory<Recipe, GameObject, PrefabPreview> {}
        public PrefabPreview(Recipe recipe, GameObject loadedObject, PrefabPreviewCache cache) : base(recipe, loadedObject)
        {
            _cache = cache;
            _machinePrefab = loadedObject.GetComponent<BuildableMachineBase>();
        }
        
        protected override void UpdatePreviewInternal(Building building, BuildingCell aimedPosition, bool isValid,
            bool flipped)
        {
            var color = isValid ? Color.green : Color.red;
            if (_wrapper == null || _wrapper.Preview == null)
            {
                _wrapper = _cache.GetPreview(Recipe, LoadedObject, building, aimedPosition);
                Debug.Assert(_wrapper != null && _wrapper.Preview != null);
            }
            _wrapper.Color = color;
            _wrapper.Flipped = flipped;
            _wrapper.Update(building, aimedPosition);
        }

        public override void BuildFromPreview(Building building, BuildingCell gridPosition, bool isFlipped)
        {
            var bMachines = building.GetComponent<BMachines>();
            Debug.Assert(bMachines != null, $"Building {building.name} does not have a BMachines component", building);
            if (bMachines == null) return;
            bMachines.Build(_machinePrefab, gridPosition.cell2D, isFlipped);
        }

        public override GameObject CreatePreviewFrom(Recipe recipe, GameObject loadedObject, Building building, BuildingCell aimedPosition)
        {
            _wrapper = _cache.GetPreview(recipe, loadedObject, building, aimedPosition);
            return _wrapper.Preview;
        }
    }
}
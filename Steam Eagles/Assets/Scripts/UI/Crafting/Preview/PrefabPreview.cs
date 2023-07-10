using System;
using Buildables;
using Buildings;
using Items;
using UI.Crafting.Events;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class PrefabPreview : RecipePreview<GameObject>
    {
        private readonly PrefabPreviewCache _cache;
        private readonly CraftingEventPublisher _eventPublisher;
        private PrefabPreviewWrapper _wrapper;
        private BuildableMachineBase _machinePrefab;

        public class Factory : PlaceholderFactory<Recipe, GameObject, PrefabPreview> {}
        public PrefabPreview(Recipe recipe, GameObject loadedObject, PrefabPreviewCache cache, CraftingEventPublisher eventPublisher) : base(recipe, loadedObject)
        {
            _cache = cache;
            _eventPublisher = eventPublisher;
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
            var builtMachine = bMachines.Build(_machinePrefab, gridPosition.cell2D, isFlipped);
            _eventPublisher.OnPrefabBuilt(gridPosition, builtMachine.gameObject, _machinePrefab.gameObject, builtMachine.IsFlipped);
        }

        public override GameObject CreatePreviewFrom(Recipe recipe, GameObject loadedObject, Building building, BuildingCell aimedPosition)
        {
            _wrapper = _cache.GetPreview(recipe, loadedObject, building, aimedPosition);
            return _wrapper.Preview;
        }
    }
}
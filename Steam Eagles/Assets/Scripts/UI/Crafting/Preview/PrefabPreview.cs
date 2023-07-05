using System;
using Buildings;
using Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.Crafting
{
    public class PrefabPreview : RecipePreview<GameObject>
    {
        public PrefabPreview(Recipe recipe, GameObject loadedObject) : base(recipe, loadedObject) {}
        
        protected override void UpdatePreviewInternal(Building building, BuildingCell aimedPosition, bool isValid)
        {
            throw new NotImplementedException();
        }

        public override void BuildFromPreview(Building building, BuildingCell gridPosition)
        {
            throw new NotImplementedException();
        }

        public override GameObject CreatePreviewFrom(Recipe recipe, GameObject loadedObject, Building building, BuildingCell aimedPosition)
        {
            throw new NotImplementedException();
        }
    }
}
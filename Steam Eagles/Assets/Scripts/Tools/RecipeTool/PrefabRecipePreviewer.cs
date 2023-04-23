using Items;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tools.RecipeTool
{
    [UsedImplicitly]
    public class PrefabRecipePreviewer : RecipePreviewerBase<GameObject>
    {
        public PrefabRecipePreviewer(MonoBehaviour caller, PreviewConfig config, Recipe recipe) : base(caller, config, recipe) { }
        public override void SetVisible(bool isPreviewVisible)
        {
            throw new System.NotImplementedException();
        }

        public override AsyncOperationHandle<GameObject> GetLoader(MonoBehaviour caller, Recipe recipe) => recipe.GetPrefabLoader(caller).LoadOp;
        
        
        
    }
}
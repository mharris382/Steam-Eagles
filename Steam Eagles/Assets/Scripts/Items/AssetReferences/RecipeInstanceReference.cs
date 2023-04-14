using UnityEngine.AddressableAssets;

namespace Items
{
    [System.Serializable]
    public class RecipeInstanceReference : ComponentReference<RecipeInstance>
    {
        public RecipeInstanceReference(string guid) : base(guid)
        {
        }
    }


  
}
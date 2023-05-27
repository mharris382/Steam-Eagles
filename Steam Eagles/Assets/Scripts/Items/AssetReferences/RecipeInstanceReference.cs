using UnityEngine.AddressableAssets;
using Utilities.AddressablesUtils;

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
using UnityEditor;
using UnityEngine.Tilemaps;
using Utilities.AddressablesUtils;

namespace Items
{
    [System.Serializable]
    public class RecipeReference : SOReference<Recipe>
    {
        public RecipeReference(string guid) : base(guid)
        {
        }
        
    }
    
    
    [System.Serializable]
    public class TileReference : SOReference<TileBase>
    {
        public TileReference(string guid) : base(guid)
        {
        }
    }
}
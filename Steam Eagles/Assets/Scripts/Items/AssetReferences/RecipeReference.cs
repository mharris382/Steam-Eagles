using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;

namespace Items
{
    public abstract class SOReference<T> : AssetReference where T : ScriptableObject
    {
        public SOReference(string guid) : base(guid)
        {
        }
        public override bool ValidateAsset(Object obj)
        {
            var so = obj as T;
            return so != null;
       }
        public override bool ValidateAsset(string path)
        {
#if UNITY_EDITOR
            var recipe = AssetDatabase.LoadAssetAtPath<T>(path);
            return recipe != null;
#else
            return false;
#endif
        }
    }
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
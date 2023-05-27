#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Utilities.AddressablesUtils
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
}
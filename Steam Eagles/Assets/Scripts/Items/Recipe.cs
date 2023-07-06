using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace Items
{
    [CreateAssetMenu(menuName = "Steam Eagles/Items/Recipe")]
    public class Recipe : SerializedScriptableObject, IIconable
    {
        public enum RecipeType
        {
            TILE,
            MACHINE
        }

        [HorizontalGroup("Recipe", width:0.2f, marginLeft:15),HideLabel] public Sprite icon;

        [HorizontalGroup("Recipe", width:0.8f),TableList(AlwaysExpanded = true)] public List<ItemStack> components;


        [OdinSerialize, NonSerialized]   public string category = "Misc";
        [SerializeField, EnumPaging] private RecipeType recipeType;

        [ShowIf(nameof(UseInstanceReference)),SerializeField] private RecipeInstanceReference instanceReference;

        [ShowIf(nameof(UseTileReference)),SerializeField] private TileReference tileReference;


        private PrefabLoader _prefabLoader;
        private TileLoader _tileLoader;
        
        
        bool UseTileReference => recipeType == RecipeType.TILE;

        bool UseInstanceReference => recipeType == RecipeType.MACHINE;


        public bool HasComponent(ItemBase item) => components.Any(t => t.Item == item);

        public AssetReference InstanceReference
        {
            get
            {
                switch (recipeType)
                {
                    case RecipeType.TILE:
                        return tileReference;
                    case RecipeType.MACHINE:
                        return instanceReference;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public Sprite GetIcon() => icon;
        
        public RecipeType GetRecipeType() => recipeType;

        public bool IsAffordable(Inventory inventory)
        {
            foreach (var component in components)
            {
                if (!inventory.ContainsItem(component.Item, component.Count))
                {
                    return false;
                }
            }
            return true;
        }
        
        public Loader<GameObject> GetPrefabLoader(MonoBehaviour caller) => _prefabLoader ??= new PrefabLoader(caller, this);
        public Loader<TileBase> GetTileLoader(MonoBehaviour caller) => _tileLoader ??= new TileLoader(caller, this);

        public Loader<T> GetLoader<T>(MonoBehaviour caller) where T : UnityEngine.Object
        {
            if (typeof(T) == typeof(TileBase))
            {
                return  GetTileLoader(caller) as Loader<T>;
            }
            else if (typeof(T) == typeof(GameObject))
            {
                return GetPrefabLoader(caller) as Loader<T>;
            }
            else
            {
                throw new InvalidCastException("TileBase or GameObject");
            }
        }
        
        private class TileLoader : Loader<TileBase>
        {
            public TileLoader(MonoBehaviour caller, Recipe recipe) : base(caller, recipe) { }
        }
        private class PrefabLoader : Loader<GameObject>
        {
            public PrefabLoader(MonoBehaviour caller, Recipe recipe) : base(caller, recipe) { }
        }
        
        public abstract class Loader<T>  where T : UnityEngine.Object
        {
            private readonly Recipe _recipe;
            private AsyncOperationHandle<T> _loadOp;

            public bool IsLoaded { get; private set; }
            public T LoadedObject { get; private set; }
            public AsyncOperationHandle<T> LoadOp => _loadOp;
            
            public Loader(MonoBehaviour caller, Recipe recipe)
            {
                _recipe = recipe;
          
                IsLoaded = false;
                caller.StartCoroutine(UniTask.ToCoroutine(async () =>
                {
                    _loadOp = recipe.InstanceReference.LoadAssetAsync<T>();
                    await _loadOp.ToUniTask();
                    LoadedObject = _loadOp.Result;
                    IsLoaded = true;
                }));
                
            }
        }
    }

    

    
}
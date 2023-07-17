using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
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
        public enum RecipeCategory
        {
            Tiles,
            Machines,
            Decor,
            Security,
            CUSTOM
        }

        private const string TAB_GROUP = "Tabs";
        private const string BASIC_INFO = "Main";
        private const string TYPE_INFO = "/Recipe Type";
        private const string HORIZONTAL_SPLIT_1 = "/h1";
        private const string VERTICAL_SPLIT_1 = "/v1";
        private const int LABEL_WIDTH = 100;
        
        [BoxGroup(BASIC_INFO)]
        [HorizontalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1, width:150, PaddingLeft = 5)]
        [HideLabel,PreviewField(150, ObjectFieldAlignment.Left)] public Sprite icon;
        
        [HorizontalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1, width:.66f, LabelWidth = LABEL_WIDTH)]
        
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1), LabelText("Display Name")]
        public string friendlyName;
        
        
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1),LabelWidth(LABEL_WIDTH), LabelText("Category")]
        public RecipeCategory recipeCategory = RecipeCategory.Tiles;
        
        [ShowIf("@recipeCategory == RecipeCategory.CUSTOM")]
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1),LabelWidth(LABEL_WIDTH), LabelText("Custom Category")]
        [SerializeField] string customCategory = "Misc";
        
        
        


        public string category
        {
            get
            {
                if (recipeCategory == RecipeCategory.CUSTOM) return customCategory;
                return recipeCategory.ToString();
            }
        }


        [TitleGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1 + TYPE_INFO, "Recipe Type")]
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1), LabelText("Type"),LabelWidth(LABEL_WIDTH)]
        [SerializeField, EnumPaging] private RecipeType recipeType;

        [TitleGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1 + TYPE_INFO)]
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1), LabelText("Prefab"),LabelWidth(LABEL_WIDTH)]
        [ShowIf(nameof(UseInstanceReference)),SerializeField] private RecipeInstanceReference instanceReference;

        [TitleGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1 + TYPE_INFO)]
        [VerticalGroup(BASIC_INFO + HORIZONTAL_SPLIT_1 + VERTICAL_SPLIT_1), LabelText("Tile"),LabelWidth(LABEL_WIDTH)]
        [ShowIf(nameof(UseTileReference)),SerializeField] private TileReference tileReference;


        [BoxGroup("Recipe Component")]
        [PropertyOrder(1000), ListDrawerSettings(ShowFoldout = true)]
        public List<ItemStack> components;
        
        private PrefabLoader _prefabLoader;
        private TileLoader _tileLoader;

        
        public string FriendlyName
        {
            get
            {
                if (string.IsNullOrEmpty(friendlyName))
                {
                    var newName = name.Replace("Recipe", "").Replace("Buildable", "").Replace("_", "").Trim();
                    return newName;
                }

                return friendlyName;
            }
        }
        
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
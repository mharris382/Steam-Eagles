using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Items
{
    public class RecipeEditor
    {
        private readonly List<ItemBase> _allItems;

        [TableList(AlwaysExpanded = true, ShowIndexLabels = false)]
        [ShowInInspector]
        public List<RecipeWrapper> r;

        [Button]
        void SortByName()
        {
            r = r.OrderBy(recipe => recipe.Name).ToList();
        }
        [Button]
        void SortByCategory()
        {
            r = r.OrderBy(recipe => recipe.category).ToList();
        }
        public RecipeEditor(IEnumerable<Recipe> recipes,IEnumerable<ItemBase> allItems)
        {
            _allItems = allItems.ToList();
            
            r = new List<RecipeWrapper>();
            foreach (var re in recipes.Where(t => !t.name.Contains("_DEP")))
            {
                r.Add(new RecipeWrapper(re, this));
            }
            
        }
        
        public class RecipeWrapper
        {
        
            [TableColumnWidth(160, true),ShowInInspector]
            public string Name
            {
                get { return _recipe.name; }
            }
            [OnValueChanged(nameof(SaveAsset))]
            [ShowInInspector,InlineEditor(Expanded = false)]
            private readonly Recipe _recipe;
            private readonly RecipeEditor _recipeEditor;
            private readonly SerializedObject _serializedObject;
            private readonly SerializedProperty _category;
            private readonly SerializedProperty _components;
            private readonly SerializedProperty _icon;

            [OnValueChanged(nameof(SaveAsset))]
            [ShowInInspector, TableColumnWidth(50, false), PreviewField(height:50, ObjectFieldAlignment.Left)]
            public Sprite Icon
            {
                get => _recipe.icon;
                set
                {
                    _serializedObject.Update();
                    _recipe.icon = value;
                    _icon.objectReferenceValue = value;
                    _serializedObject.ApplyModifiedProperties();
                }
            }

            [OnValueChanged(nameof(SaveAsset))]
            [TableList(AlwaysExpanded = false)]
            [ShowInInspector, TableColumnWidth(250, true)]
            List<ItemStackWrapper> components
            {
                get
                {
                    var wrappers = new List<ItemStackWrapper>();
                    foreach (var recipeComponent in _recipe.components)
                    {
                        wrappers.Add(new ItemStackWrapper(_recipeEditor, _recipe, recipeComponent));
                    }
                    return wrappers;
                }
                set
                {
                    List<ItemStack> stacks = new List<ItemStack>();
                    foreach (var stackWrapper in value)
                    {
                        stacks.Add(stackWrapper.Stack);
                    }
                    _recipe.components = stacks;
                }
            }

            [OnValueChanged(nameof(SaveAsset))]
            [ShowInInspector, TableColumnWidth(250, true), EnumToggleButtons]
            public Recipe.RecipeCategory category
            {
                get => _recipe.recipeCategory;
                set
                {
                    _serializedObject.Update();
                    
                    _recipe.recipeCategory = value;
                    _category.enumValueIndex = (int) value;
                    _serializedObject.ApplyModifiedProperties();
                }
            }

            public RecipeWrapper(Recipe recipe, RecipeEditor recipeEditor)
            {
                _recipe = recipe;
                _serializedObject = new SerializedObject(_recipe);
                _category = _serializedObject.FindProperty("recipeCategory");
                _icon = _serializedObject.FindProperty("icon");
                _recipeEditor = recipeEditor;
            }

            void SaveAsset()
            {
                var r = AssetDatabase.GetAssetPath(_recipe);
                AssetDatabase.ForceReserializeAssets(new[] {r}, ForceReserializeAssetsOptions.ReserializeMetadata);
                AssetDatabase.SaveAssets();
            }

            public class ItemStackWrapper
            {
                private readonly RecipeEditor _editor;
                private readonly Recipe _recipe;
                private  ItemStack _itemBase;


                public ItemStack Stack => _itemBase;
                [ShowInInspector, HideLabel]
                public string ItemName
                {
                    get => Item == null ? "null" : Item.itemName;
                }

                [ShowInInspector, TableColumnWidth(50,false)]
                [HideLabel, PreviewField(Height = 50, Alignment = ObjectFieldAlignment.Left)]
                public Sprite ItemIcon
                {
                    get => Item == null ? null : Item.icon;
                }

                [ValueDropdown(nameof(GetItemDropdown))]
                [ShowInInspector, TableColumnWidth(50)]
                public ItemBase Item
                {
                    get => _itemBase.item;
                    set => _itemBase.item = value;
                }


                
                [PropertyRange(1, nameof(MaxValue))]
                [ShowInInspector, TableColumnWidth(70)]
                [EnableIf(nameof(HasItem))]
                public int Count
                {
                    get => _itemBase.Count;
                    set => _itemBase.SetCount(Mathf.Max(1, value));
                }

                public int MaxValue => Item == null ? 1 : Item.MaxStackSize;
                public bool HasItem => Item != null;
                

                public ItemStackWrapper(RecipeEditor editor, Recipe recipe, ItemStack itemBase)
                {
                    _editor = editor;
                    this._recipe = recipe;
                    _itemBase = itemBase;
                }
                
                public ValueDropdownList<ItemBase> GetItemDropdown()
                {
                    var list = new ValueDropdownList<ItemBase>();
                    foreach (var item in _editor._allItems)
                    {
                        if (item is Tool) continue;
                        if(_recipe.HasComponent(item)==false)
                            list.Add(item.itemName, item);
                    }
                    return list;
                }
            }
        }
    }
}
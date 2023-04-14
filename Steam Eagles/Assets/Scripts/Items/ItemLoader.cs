using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Items
{
    public class ItemLoader : Singleton<ItemLoader>
    {
        public override bool DestroyOnLoad => false;
        private class LoadedItem
        {
            private readonly AsyncOperationHandle<ItemBase> _loadOp;
            public ItemBase Item => _loadOp.IsDone ? _loadOp.Result : null;
            public bool IsLoaded => _loadOp.IsDone && _loadOp.Status == AsyncOperationStatus.Succeeded;
            public LoadedItem(ItemLoader itemLoader, string key)
            {
                this.Key = key;
                this.Address = GetAddressFromKey(key);
                this._loadOp = Addressables.LoadAssetAsync<ItemBase>(Address);
                this._loadOp.Completed += (op) =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        itemLoader._loadedKeys.Add(op.Result, Key);
                    }
                };
            }

            public string Address { get; set; }

            public string Key { get; set; }

            public async UniTask<ItemBase> GetItem()
            {
                await _loadOp.Task;
                return _loadOp.Result;
            }
        }
        
        private class LoadedRecipe
        {
            private readonly AsyncOperationHandle<Recipe> _loadOp;

            public LoadedRecipe(ItemLoader loader, string key)
            {
                this.Key = key;
                this.Address = key.Contains("Recipe") ? key : $"Recipe_{key}";
                Debug.Log($"Loading Recipe at {Address.Bolded()}");
                _loadOp = Addressables.LoadAssetAsync<Recipe>(Address);
            }

            public string Key { get;  }

            public string Address { get;  }
            
            public bool IsLoaded => _loadOp.IsDone && _loadOp.Status == AsyncOperationStatus.Succeeded;
            
            public async UniTask<Recipe> GetRecipe()
            {
                await _loadOp.Task;
                return _loadOp.Result;
            }
        }
        

        public List<string> autoLoadItems;
        private Dictionary<string, LoadedItem> _loadedItems = new Dictionary<string, LoadedItem>();
        private Dictionary<ItemBase, string> _loadedKeys = new Dictionary<ItemBase, string>();
        private Dictionary<string, LoadedRecipe> _loadedRecipes = new Dictionary<string, LoadedRecipe>();

        protected override void Init()
        {
            if(autoLoadItems == null)
                return;
            foreach (var autoLoadItem in autoLoadItems)
            {
                try
                {
                    _loadedItems.Add(autoLoadItem, new LoadedItem(this, autoLoadItem));
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load addressable {autoLoadItem}",this);
                }
            }
        }

        private static string GetAddressFromKey(string key)
        {
            return $"{key}_item";
        }
        
        public void LoadItem(string key)
        {
            if (_loadedItems.ContainsKey(key)) return;
            Debug.Log($"Now Loading Item: {key}\nAddress:{GetAddressFromKey(key)}");
            
            _loadedItems.Add(key, new LoadedItem(this, key));
        }
     

        public bool IsItemLoaded(string itemName)
        {
            if (!_loadedItems.ContainsKey(itemName))
            {
                LoadItem(itemName);
            }
            return _loadedItems[itemName].IsLoaded;
        }


        public IEnumerator LoadAndWaitForItem(string key)
        {
            if(IsItemLoaded(key))
                yield break;
            LoadItem(key);
            while (!IsItemLoaded(key))
            {
                yield return null;
            }
        }

        public async UniTaskVoid LoadItemsParallel(string key1, string key2)
        {
            var (a, b) = await UniTask.WhenAll(
                LoadItemAsync(key1),
                LoadItemAsync(key2));
        }
        public async UniTask<(ItemBase item1, ItemBase item2, ItemBase item3)> LoadItemsParallel(string key1, string key2, string key3)
        {
            return await UniTask.WhenAll(
                LoadItemAsync(key1),
                LoadItemAsync(key2),
                LoadItemAsync(key3));
            
        }
        public async UniTask<(ItemBase item1, ItemBase item2, ItemBase item3, ItemBase item4)> LoadItemsParallel(string key1, string key2, string key3, string key4)
        {
            return await UniTask.WhenAll(
                LoadItemAsync(key1),
                LoadItemAsync(key2),
                LoadItemAsync(key3),
                LoadItemAsync(key4));
        }

        public async UniTaskVoid LoadItemsParallel(string key1, string key2, string key3, string key4, string key5)
        {
            var (a, b, c, d, e) = await UniTask.WhenAll(
                LoadItemAsync(key1),
                LoadItemAsync(key2),
                LoadItemAsync(key3),
                LoadItemAsync(key4), 
                LoadItemAsync(key5));
        }

        public async UniTask<Recipe> LoadRecipeAsync(string key)
        {
            if(!_loadedRecipes.TryGetValue(key, out var loadedRecipe))
            {
                loadedRecipe = new LoadedRecipe(this, key);
                _loadedRecipes.Add(key, loadedRecipe);
            }
            var res = await _loadedRecipes[key].GetRecipe();
            Debug.Assert(res != null, $"Failed to load recipe for {key}");
            return res;
        }

        public async UniTask<ItemBase> LoadItemAsync(string key)
        {
            if (IsItemLoaded(key))
            {
                return _loadedItems[key].Item;
            }
            else
            {
                LoadItem(key);
            }
            var item = await _loadedItems[key].GetItem();
            return item;
        }
        
        public ItemBase GetItem(string itemName)
        {
            if (!IsItemLoaded(itemName))
            {
                return null;
            }
            return _loadedItems[itemName].Item;
        }

        public bool HasKey(ItemBase itemBase)
        {
            if (itemBase == null) return false;
            return _loadedKeys.ContainsKey(itemBase);
        }

        public string GetKey(ItemBase itemBase)
        {
            if(_loadedKeys.ContainsKey(itemBase))
                return _loadedKeys[itemBase];
            Debug.LogError("Item not found in loaded keys. Item must have been loaded externally!", itemBase);
            return null;
        }
        
        
    }
}
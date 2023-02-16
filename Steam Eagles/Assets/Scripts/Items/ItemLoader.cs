using System.Collections.Generic;
using CoreLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace Items
{
    public class ItemLoader : Singleton<ItemLoader>
    {
        private class LoadedItem
        {
            private readonly AsyncOperationHandle<Item> _loadOp;
            public Item Item => _loadOp.IsDone ? _loadOp.Result : null;
            public bool IsLoaded => _loadOp.IsDone && _loadOp.Status == AsyncOperationStatus.Succeeded;
            public LoadedItem(string address)
            {
                this._loadOp = Addressables.LoadAssetAsync<Item>(address);
            }
        }

        public List<string> autoLoadItems;
        private Dictionary<string, LoadedItem> _loadedItems = new Dictionary<string, LoadedItem>();

        protected override void Init()
        {
            foreach (var autoLoadItem in autoLoadItems)
            {
                _loadedItems.Add(autoLoadItem, new LoadedItem(autoLoadItem));
            }
        }

        private static string GetAddressFromKey(string key)
        {
            return key;
        }
        
        public void LoadItem(string key)
        {
            if (_loadedItems.ContainsKey(key)) return;
            Debug.Log($"Now Loading Pickup: {key}\nAddress:{GetAddressFromKey(key)}");
            
            _loadedItems.Add(key, new LoadedItem(key));
        }
     

        public bool IsItemLoaded(string itemName)
        {
            if (!_loadedItems.ContainsKey(itemName))
            {
                LoadItem(itemName);
            }
            return _loadedItems[itemName].IsLoaded;
        }

        public Item GetItem(string itemName)
        {
            if (!IsItemLoaded(itemName))
            {
                return null;
            }
            return _loadedItems[itemName].Item;
        }
    }
}
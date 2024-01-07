using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Items
{
    public class ItemCache
    {
        private Dictionary<string, ItemBase> _items = new Dictionary<string, ItemBase>();


        public bool IsItemLoaded(string name)
        {
            return _items.ContainsKey(name) && _items[name] != null;
        }

        public async UniTask<ItemBase> GetItem(string name)
        {
            if (_items.TryGetValue(name, out var item))
            {

                if (item != null) return item;
                _items.Remove(name);
            }
            item = await ItemLoader.Instance.LoadItemAsync(name);
            if (item != null)
            {
                _items.Add(name, item);
            }
            else
            {
                Debug.LogError("Failed to load item: " + name);
                throw new NullReferenceException();
            }
            return item;
        }
    }
}
using System;
using System.Collections.Generic;
using RuntimeItemStack = Items.ItemStack;
namespace Items.SaveLoad
{
    [Serializable]
    public class InventorySaves
    {
        [Serializable]
        public class ItemStack
        {
            public string itemKey;
            public int stackCount;

            public ItemStack(RuntimeItemStack runtimeStack)
            {
                this.itemKey = runtimeStack.Key;
                stackCount = runtimeStack.Count;
            }
        }
        
        [Serializable]
        public class InventorySave
        {
            public string key;
            public List<ItemStack> stacks;

            public InventorySave(string key)
            {
                this.key = key;
                stacks = new List<ItemStack>();
            }

            public List<RuntimeItemStack> GetRuntimeStack()
            {
                throw new NotImplementedException();
            }
        }
    
   
        private List<InventorySave> _saves;
        private Dictionary<string, int> _cache;

        public InventorySaves()
        {
            _saves = new List<InventorySave>();
            _cache = new Dictionary<string, int>();
        }

        public int Count => _saves.Count;
        
        
        public InventorySave this[string key]
        {
            get
            {
                //check if we have already seen this key before
                if (_cache.ContainsKey(key))
                {

                    return _saves[_cache[key]];
                }

                
                //check if key already exists in loaded save file
                for (int i = 0; i < _saves.Count; i++)
                {
                    var inventorySave = _saves[i];
                    if (inventorySave.key == key)
                    {
                        _cache.Add(key, i);
                        return _saves[i];
                    }
                }
                
                
                //key does not exist so we need to create one and add it to the save file
                var newSave = new InventorySave(key);
                _saves.Add(newSave);
                _cache.Add(newSave.key, _saves.Count-1);
                return newSave;
            }
        }
    }
}
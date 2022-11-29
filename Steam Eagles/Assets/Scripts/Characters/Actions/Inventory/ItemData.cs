using System.Collections.Generic;
using UnityEngine;

namespace Characters.Actions.Inventory
{
    public class ItemData : ScriptableObject
    {
        public string itemName;
        
        public string groupName;
        
        public UnityEngine.Object[] itemData;

        public virtual string GetItemGroup() => groupName;

        public virtual T GetItemData<T>() where T : UnityEngine.Object
        {
            foreach (var item in itemData)
            {
                if (item is T)
                {
                    return item as T;
                }
            }
            Debug.LogError($"Requested typeof not found in itemData {itemName}", this);
            return null;
        }
    }
}
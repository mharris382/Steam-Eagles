using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Items
{
    public static class ItemExtensions 
    {
        /// <summary>
        /// spawns an item in the world
        /// </summary>
        /// <param name="itemBase"></param>
        /// <returns></returns>
        public static bool SpawnPickup(this ItemBase itemBase, Vector3 position, out GameObject spawnedObject)
        {
            spawnedObject = null;
            if (!itemBase.canDrop) return false;
            string pickupKey = itemBase.dropKey;
            if(PickupLoader.Instance.TryGetPickup(pickupKey, out Pickup pickup))
            {
                spawnedObject = pickup.SpawnPickup(position).gameObject;
                return true;
            }
            return true;
        }


        public static string GetItemKey(this ItemBase itemBase) => ItemLoader.Instance.GetKey(itemBase);

        public static bool GetPickup(this ItemBase itemBase, out Pickup pickup)
        {
            pickup = null;
            if (!itemBase.canDrop) return false;
            string pickupKey = itemBase.dropKey;
            if(PickupLoader.Instance.TryGetPickup(pickupKey, out pickup))
            {
                
                return true;
            }
            return true;
        }


        public static IEnumerator GetPickupAsync(this ItemBase itemBase, Action<Pickup> callback)
        {

            if (itemBase.GetPickup(out var pickup))
            {
                callback?.Invoke(pickup);
                yield break;
            }

            if (!ItemLoader.Instance.HasKey(itemBase))
            {
                Debug.LogError("Item not found in loaded keys. Item must have been loaded externally!", itemBase);
                yield break;
            }
            PickupLoader.Instance.LoadPickup(itemBase.GetItemKey());
            while (!PickupLoader.Instance.IsPickupLoaded(itemBase.GetItemKey()))
            {
                yield return null;
            }

            Pickup res = PickupLoader.Instance.GetPickup(itemBase.GetItemKey());
            callback?.Invoke(res);
        }

        public static bool IsItemLoaded(this string itemName)
        {
            return ItemLoader.Instance.IsItemLoaded(itemName);
        }

        public static IEnumerator LoadItem(this string itemName, Action<ItemBase> callback)
        {
            yield return ItemLoader.Instance.LoadAndWaitForItem(itemName);
            ItemBase item = ItemLoader.Instance.GetItem(itemName);
            callback?.Invoke(item);
        }


        public static UniTask<ItemBase> LoadItemAsync(this string itemName) => ItemLoader.Instance.LoadItemAsync(itemName);

        public static UniTask<Recipe> LoadRecipe(this string recipeName) =>
            ItemLoader.Instance.LoadRecipeAsync(recipeName);

        public static ItemBase GetItem(this string itemName)
        {
            if(ItemLoader.Instance.IsItemLoaded(itemName))
            {
                return ItemLoader.Instance.GetItem(itemName);
            }
            return null;
        }

        public static UniTask<Pickup> LoadPickup(this ItemBase item) => LoadPickup(item.itemName.Trim());
        public static UniTask<Pickup> LoadPickup(this string itemName) => PickupLoader.Instance.LoadPickupAsync(itemName.Trim());

        public static IEnumerator LoadPickup(this string itemName, Action<Pickup> callback)
        {
            yield return PickupLoader.Instance.LoadAndWaitForPickup(itemName);
            Pickup pickup = PickupLoader.Instance.GetPickup(itemName);
            callback?.Invoke(pickup);
        }

        public static bool IdentifyItem(this GameObject gameObject, out ItemBase itemBase)
        {
            itemBase = null;
            var pickupBody = gameObject.GetComponent<PickupID>();
            if (pickupBody == null)
                return false;
            
            return false;
        }


        public static void Log(this IEnumerable<InventorySlot> slots)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var inventorySlot in slots)
            {
                stringBuilder.AppendLine(inventorySlot.ToString());
            }
            Debug.Log(stringBuilder.ToString());
        }
    }
}
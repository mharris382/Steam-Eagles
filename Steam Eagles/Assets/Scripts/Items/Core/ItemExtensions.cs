using CoreLib.Pickups;
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


        public static bool IdentifyItem(this GameObject gameObject, out ItemBase itemBase)
        {
            itemBase = null;
            var pickupBody = gameObject.GetComponent<PickupBody>();
            if (pickupBody == null)
                return false;
            
            return false;
        }
    }
}
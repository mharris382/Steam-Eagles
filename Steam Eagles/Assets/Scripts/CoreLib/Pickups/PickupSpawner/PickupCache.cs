using System.Collections.Generic;
using System.Threading.Tasks;
using Items;

namespace CoreLib.Pickups.PickupSpawner
{
    public class PickupCache
    {
        private Dictionary<string, Pickup> _loadedPickups = new();

        public async Task<Pickup> GetPickup(string itemName)
        {
            if (_loadedPickups.TryGetValue(itemName, out var pickup) )
            {
                if (pickup != null)
                {
                    return pickup;
                }
                _loadedPickups.Remove(itemName);
            }
            pickup = await itemName.LoadPickup();
            _loadedPickups.Add(itemName, pickup);
            return pickup;
        }
    }
}
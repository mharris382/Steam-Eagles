using System.Collections.Generic;
using CoreLib.Pickups;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
namespace Items
{
    public class PickupInventoryHandler : MonoBehaviour
    {
        public  Inventory inventory;
        public CharacterPickupController pickupController;

        Dictionary<Pickup, Coroutine> _activeCoroutines = new Dictionary<Pickup, Coroutine>();

        public void Awake()
        {
            pickupController.PickupFilter = PickupFilter;
            pickupController.OnPickup.Subscribe(OnPickup).AddTo(this);
        }

        private bool PickupFilter(Pickup obj)
        {
            var result = ItemLoader.Instance.IsItemLoaded(obj.key);
            if (!result && !_activeCoroutines.ContainsKey(obj))
            {
                _activeCoroutines.Add(obj,StartCoroutine(UniTask.ToCoroutine(async () =>
                {
                    var item = await ItemLoader.Instance.LoadItemAsync(obj.key);
                    if (item != null) Debug.Log($"Loaded item: {item.name}");
                    else Debug.LogError($"Failed to load item: {obj.key}");
                })));
            }
            return result;
        }


        private void OnPickup(Pickup obj)
        {
            Debug.Assert(ItemLoader.Instance.IsItemLoaded(obj.key), "Filter should have prevented this",this);
            var item = ItemLoader.Instance.GetItem(obj.key);
            inventory.AddItem(item, 1);
            Debug.Log($"Picked up {item.name}",this);
        }
    }
}
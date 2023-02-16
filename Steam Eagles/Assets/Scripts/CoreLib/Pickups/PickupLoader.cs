using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreLib.Pickups
{
    public class PickupLoader : Singleton<PickupLoader>
    {
    
        public List<string> autoLoadPickups;
    
    
        private Dictionary<string, LoadedPickupSpawner> _pickupSpawners = new Dictionary<string, LoadedPickupSpawner>();
        private List<LoadedPickupSpawner> _loadedPickupSpawners;
        private class LoadedPickupSpawner
        {
            public string key;
            public Pickup spawner;
            public readonly AsyncOperationHandle<Pickup> loadOp;

            public LoadedPickupSpawner(string key)
            {
                this.key = key;
                this.loadOp = Addressables.LoadAssetAsync<Pickup>(GetAddressFromKey(key));
                loadOp.Completed += res =>
                {
                    Debug.Assert(res.Result != null, $"Failed to find pickup spawner with key {key}\nAddress:{GetAddressFromKey(key)}" );
                    spawner = res.Result;
                };
            }
        }

        protected override void Init()
        {
            _loadedPickupSpawners = new List<LoadedPickupSpawner>();
            foreach (var autoLoadPickup in autoLoadPickups)
            {
                var spawner = new LoadedPickupSpawner(autoLoadPickup);
                _loadedPickupSpawners.Add(spawner);
                _pickupSpawners.Add(autoLoadPickup,spawner);
            }
        }

        protected internal static string GetAddressFromKey(string key)
        {
            return $"{key}_pickup";
        }
        public bool HasPickup(string pickupName)
        {
            return _pickupSpawners.ContainsKey(pickupName);
        }
    
        public bool TryGetPickup(string pickupName, out Pickup pickup)
        {
            pickup = null;
            if (_pickupSpawners.TryGetValue(pickupName, out var spawner))
            {
                if (spawner.loadOp.IsDone)
                {
                    pickup = spawner.spawner;
                    return true;
                }
                return false;
            }
        
            pickup = null;
            return false;
        }

        public void LoadPickup(string key)
        {
            if (_pickupSpawners.ContainsKey(key)) return;
            Debug.Log($"Now Loading Pickup: {key}\nAddress:{GetAddressFromKey(key)}");
            var spawner = new LoadedPickupSpawner(key);
            _pickupSpawners.Add(key, spawner);
        }
        
        public bool IsPickupLoaded(string pickupName)
        {
            if (!_pickupSpawners.ContainsKey(pickupName))
            {
                LoadPickup(pickupName);
            }
            return _pickupSpawners[pickupName].loadOp.IsDone &&
                   _pickupSpawners[pickupName].loadOp.Status == AsyncOperationStatus.Succeeded;
        }

        public Pickup GetPickup(string pickupName)
        {
            if (!IsPickupLoaded(pickupName))
            {
                LoadPickup(pickupName);
                return null;
            }
            return _pickupSpawners[pickupName].spawner;
        }
    }
}

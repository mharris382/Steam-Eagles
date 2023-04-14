using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CoreLib.Pickups
{
    public class PickupLoader : Singleton<PickupLoader>
    {
    
        public List<string> autoLoadPickups;
        public override bool DestroyOnLoad => false;
    
        private Dictionary<string, LoadedPickupSpawner> _pickupSpawners = new Dictionary<string, LoadedPickupSpawner>();
        private List<LoadedPickupSpawner> _loadedPickupSpawners;
        private class LoadedPickupSpawner
        {
            public string key;
            public Pickup spawner;
            public readonly AsyncOperationHandle<Pickup> loadOp;

            public LoadedPickupSpawner(string key)
            {
                this.key = key.Replace(" ", "");
                this.loadOp = Addressables.LoadAssetAsync<Pickup>(GetAddressFromKey(key));
                loadOp.Completed += res =>
                {
                    Debug.Assert(res.Result != null, $"Failed to find pickup spawner with key {key}\nAddress:{GetAddressFromKey(key)}" );
                    spawner = res.Result;
                };
            }

            public async UniTask<Pickup> GetPickup()
            {
                await loadOp.Task;
                return spawner;
            }
        }

        protected override void Init()
        {
            _loadedPickupSpawners = new List<LoadedPickupSpawner>();
            if (autoLoadPickups == null) return;
            foreach (var autoLoadPickup in autoLoadPickups)
            {
                var spawner = new LoadedPickupSpawner(autoLoadPickup);
                _loadedPickupSpawners.Add(spawner);
                _pickupSpawners.Add(autoLoadPickup,spawner);
            }
        }

        public static string GetAddressFromKey(string key)
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
            key = key.Replace(" ", "");
            if (_pickupSpawners.ContainsKey(key)) return;
            Debug.Log($"Now Loading Pickup: {key}\nAddress:{GetAddressFromKey(key)}");
            var spawner = new LoadedPickupSpawner(key);
            _pickupSpawners.Add(key, spawner);
        }
        
        public bool IsPickupLoaded(string pickupName)
        {
            pickupName = pickupName.Replace(" ", "");
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

        public IEnumerator LoadAndWaitForPickup(string itemName)
        {
            if (IsPickupLoaded(itemName))
            {
                yield break;
            }
            LoadPickup(itemName);
            while (!IsPickupLoaded(itemName))
            {
                yield return null;
            }
            
        }

        public async UniTask<Pickup> LoadPickupAsync(string key)
        {
            if (IsPickupLoaded(key))
            {
                return _pickupSpawners[key].spawner;
            }
            LoadPickup(key);
            var pickup = await _pickupSpawners[key].GetPickup();
            return pickup;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Items.UI
{
    public class UIInventoryItemSlots : Singleton<UIInventoryItemSlots>
    {
        public string defaultItemSlotPrefabAddress = "ItemSlot";

        public string[] autoLoadKeys = new[]
        {
            "ItemSlot"
        };


        private Transform _inactiveItemsParent;
        private Queue<UIInventoryItem> _inactiveItems = new Queue<UIInventoryItem>();

        private UIInventoryItem _itemSlotPrefab;

        private Dictionary<string, LoadedItemDisplay> _displays = new Dictionary<string, LoadedItemDisplay>();

        private class LoadedItemDisplay
        {
            private Queue<UIItem> _inactiveItems = new Queue<UIItem>();
            private Transform _inactiveItemsParent;
            private UIItem _itemPrefab;
            private AsyncOperationHandle<GameObject> _loadPrefabHandle;

            public LoadedItemDisplay(string key, UIInventoryItemSlots slots)
            {
                _inactiveItemsParent = new GameObject($"Inactive Item Slots: {key}").transform;
                _inactiveItemsParent.SetParent(slots._inactiveItemsParent, false);
                _inactiveItems = new Queue<UIItem>();
                Debug.Log($"Loading Item UI Prefab with Key: {key}\nAddress: {GetAddressFromKey(key)}");
                _loadPrefabHandle = Addressables.LoadAssetAsync<GameObject>(GetAddressFromKey(key));
                _loadPrefabHandle.Completed += handle =>
                {
                    Debug.Assert(handle.Result != null,
                        "No Addressable Prefab found for key: " + GetAddressFromKey(key));
                    _itemPrefab = handle.Result.GetComponent<UIItem>();
                    Debug.Assert(_itemPrefab != null,
                        $"No UIItem component found on prefab {handle.Result.name}! Add A UIItem", handle.Result);
                    if (_itemPrefab == null) return;
                    for (int i = 0; i < 20; i++)
                    {
                        _inactiveItems.Enqueue(Instantiate());
                    }
                };
            }

            public bool IsReady => _itemPrefab != null;

            private UIItem Instantiate()
            {
                if (!IsReady) return null;
                return GameObject.Instantiate(_itemPrefab, _inactiveItemsParent);
            }

            public UIItem GetItem()
            {
                if (!IsReady)
                    return null;
                if (_inactiveItems.Count > 0)
                {
                    return _inactiveItems.Dequeue();
                }
                else
                {
                    return Instantiate();
                }
            }

            public void ReturnUIItemSlot(UIItem itemSlot)
            {
                if (!IsReady)
                {
                    Destroy(itemSlot.gameObject);
                    return;
                }

                itemSlot.transform.SetParent(_inactiveItemsParent);
                itemSlot.Clear();
                _inactiveItems.Enqueue(itemSlot);
            }
        }


        protected override void Init()
        {
            _inactiveItemsParent = new GameObject("Inactive Items").transform;

            foreach (var autoLoadKey in autoLoadKeys)
            {
                _displays.Add(autoLoadKey, new LoadedItemDisplay(autoLoadKey, this));
            }
        }

        public bool IsReady { get; private set; }

        private IEnumerator Start()
        {
            IsReady = false;
            yield return null;
            var loadOp = Addressables.LoadAssetAsync<GameObject>(defaultItemSlotPrefabAddress);
            yield return loadOp;
            Debug.Assert(loadOp.Status == AsyncOperationStatus.Succeeded,
                $"Failed to load item slot prefab: Address={defaultItemSlotPrefabAddress}");
            _itemSlotPrefab = loadOp.Result.GetComponent<UIInventoryItem>();
            IsReady = true;

        }



        public bool IsUIReady(string key)
        {
            if (!_displays.ContainsKey(key))
            {
                _displays.Add(key, new LoadedItemDisplay(key, this));
                return false;
            }
            return _displays[key].IsReady;
        }

        public UIItem GetUIItem(string key)
        {
            if (!_displays.ContainsKey(key))
            {
                _displays.Add(key, new LoadedItemDisplay(key, this));
                return null;
            }

            if (!_displays[key].IsReady)
                return null;
            
            return _displays[key].GetItem();
        }

        public UIItem GetUIItem(string key, InventorySlot slot)
        {
            var itemUI = GetUIItem(key);
            if (itemUI != null)
            {
                itemUI.DisplaySlot(slot);
            }
            return itemUI;
        }
        
        public void ReturnUIItemSlot(string key, UIItem itemSlot)
        {
            if (itemSlot == null) return;
            if (!_displays.ContainsKey(key))
            {
                _displays.Add(key, new LoadedItemDisplay(key, this));
            }
            _displays[key].ReturnUIItemSlot(itemSlot);
        }

        private static string GetAddressFromKey(string key)
        {
            return key;
        }
        
        [System.Obsolete("Use Instantiate(string key) instead")]
        private UIInventoryItem Instantiate()
        {
            if(!IsReady)
                throw new InvalidOperationException("UIInventoryItemSlots is not ready yet");
            return Instantiate(_itemSlotPrefab, _inactiveItemsParent);
        }
        
        [System.Obsolete("Use GetUIItemSlot(string key) instead")]
        public UIInventoryItem GetUIItemSlot(InventorySlot slot)
        {
            if (_inactiveItems.Count == 0)
            {
                _inactiveItems.Enqueue(Instantiate());
            }
            var uiSlot = _inactiveItems.Dequeue();
            uiSlot.DisplaySlot(slot);
            return uiSlot;
        }
        
        [System.Obsolete("Use GetUIItemSlot(string key) instead")]
        public void ReturnUIItemSlot(UIInventoryItem itemSlot)
        {
            itemSlot.transform.SetParent(_inactiveItemsParent);
            itemSlot.Clear();
        }
        
    }
}
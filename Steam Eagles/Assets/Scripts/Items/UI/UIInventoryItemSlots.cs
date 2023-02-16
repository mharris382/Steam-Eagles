using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Items.UI
{
    public class UIInventoryItemSlots : Singleton<UIInventoryItemSlots>
    {
        public string defaultItemSlotPrefabAddress = "ItemSlot";
        private Transform _inactiveItemsParent;
        private Queue<UIInventoryItem> _inactiveItems = new Queue<UIInventoryItem>();
        
        private UIInventoryItem _itemSlotPrefab;

        protected override void Init()
        {
            _inactiveItemsParent = new GameObject("Inactive Items").transform;
            
            for (int i = 0; i < 50; i++)
            {
                
            }
        }

        public bool IsReady { get; private set; }
        private IEnumerator Start()
        {
            IsReady = false;
            yield return null;
            var loadOp = Addressables.LoadAssetAsync<GameObject>(defaultItemSlotPrefabAddress);
            yield return loadOp;
            Debug.Assert(loadOp.Status == AsyncOperationStatus.Succeeded, $"Failed to load item slot prefab: Address={defaultItemSlotPrefabAddress}");
            _itemSlotPrefab = loadOp.Result.GetComponent<UIInventoryItem>();
            IsReady = true;
            for (int i = 0; i < 50; i++)
            {
                if (_inactiveItems.Count == 0)
                {
                    _inactiveItems.Enqueue(Instantiate());
                }
            }
        }

        private UIInventoryItem Instantiate()
        {
            if(!IsReady)
                throw new InvalidOperationException("UIInventoryItemSlots is not ready yet");
            return Instantiate(_itemSlotPrefab, _inactiveItemsParent);
        }
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
        public void ReturnUIItemSlot(UIInventoryItem itemSlot)
        {
            itemSlot.transform.SetParent(_inactiveItemsParent);
            itemSlot.Clear();
        }
        
    }
}
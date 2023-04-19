using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Items.UI
{
    public class UIItemCollection : MonoBehaviour
    {
        public bool fixedSize;
        public int size = 10;

        public string slotKey = "ItemSlot";

        public Transform itemContainer;
        public bool clearOnStart = true;
        public bool IsVisible
        {
            set
            {
                foreach (var componentsInChild in GetComponentsInChildren<UIItem>())
                {
                    componentsInChild.IsVisible = value;
                }
            }
        }


        private UIItem itemPrefab;
        
        private IEnumerator Start()
        {
            yield return LoadPrefab();
            if (clearOnStart)
            {
                ClearContainer();
            }
        }

        public void LinkContainer(List<InventorySlot> slots)
        {
            int count = fixedSize ? size : slots.Count;
            for (int i = 0; i < count; i++)
            {
                var uiSlot =  UIInventoryItemSlots.Instance.GetUIItem(slotKey);
                uiSlot.transform.SetParent(itemContainer);
                if(slots.Count > i)
                {
                    //show item
                    uiSlot.DisplaySlot(slots[i]);
                }
                else
                {
                    //show empty slot
                    uiSlot.Clear();
                }
                uiSlot.IsVisible = true;
            }
        }


        IEnumerator LoadPrefab()
        {
            yield return  UniTask.ToCoroutine(async () =>
            {
                var prefabLoadOp = Addressables.LoadAssetAsync<GameObject>(slotKey);
                await prefabLoadOp.ToUniTask();
                Debug.Assert(prefabLoadOp.Status == AsyncOperationStatus.Succeeded,
                    $"Failed to load prefab with key {slotKey}",this);
                itemPrefab = prefabLoadOp.Result.GetComponent<UIItem>();
                Debug.Assert(itemPrefab != null, $"Prefab with key {slotKey} does not contain a UIItem component",this);
            });
        }
        IEnumerator LoadAndPopulate(List<ItemStack> items)
        {
            if (itemPrefab != null)
            {
                
                INTERNAL_PopulateContainer(items);
                yield break;
            }
            else
            {
                yield return LoadPrefab();
                INTERNAL_PopulateContainer(items);
            }
        }
        
        public void PopulateContainer(List<ItemStack> items)
        {
            if(itemPrefab == null)
                StartCoroutine(LoadAndPopulate(items));
            else
            {
                ClearContainer();
                INTERNAL_PopulateContainer(items);
            }
        }

        private void INTERNAL_PopulateContainer(List<ItemStack> items)
        {
            int count = fixedSize ? size : items.Count;
            for (int i = 0; i < count; i++)
            {
                var uiSlot = Instantiate(itemPrefab, itemContainer);
                if(items.Count > i)
                {
                    //show item
                    uiSlot.DisplayStack(items[i]);
                }
                else
                {
                    //show empty slot
                    uiSlot.Clear();
                }
                uiSlot.IsVisible = true;
            }
        }

        public void ClearContainer()
        {
            var uiElements = itemContainer.GetComponentsInChildren<UIItem>();
            foreach (var uiElement in uiElements)
            {
                uiElement.IsVisible = false;
                Destroy(uiElement.gameObject);
            }
        }
    }
}
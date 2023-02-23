using System;
using System.Collections;
using Sirenix.OdinInspector;
using UI;
using UniRx;
using UnityEngine;

namespace Items.UI
{
    public class UIInventory : PlayerWindow
    {
       
        public Transform uiItemParent;
        
        public Inventory inventory;

        public string slotKey = "ItemSlot";


        private IDisposable _disposable;

        IEnumerator WaitForSlotToBeReady()
        {
            while (!UIInventoryItemSlots.Instance.IsUIReady(slotKey))
            {
                yield return null;
            }
            Open();
        }
        public override void Open()
        {
            if (!UIInventoryItemSlots.Instance.IsUIReady(slotKey))
            {
                Close();
                StartCoroutine(WaitForSlotToBeReady());
                return;
            }

            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }

            CompositeDisposable disposable = new CompositeDisposable();
            foreach (var inventoryItemSlot in inventory.itemSlots)
            {
                var uiItemSlot = UIInventoryItemSlots.Instance.GetUIItem(slotKey, inventoryItemSlot);
                uiItemSlot.transform.SetParent(uiItemParent);
                disposable.Add(Disposable.Create(() => UIInventoryItemSlots.Instance.ReturnUIItemSlot(slotKey, uiItemSlot)));
            }

            _disposable = disposable;
            base.Open();
            
        }

        public override void Close()
        {
            if (_disposable != null)
            {
                _disposable.Dispose();
                _disposable = null;
            }
            base.Close();
        }
    }
}
using System;
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



        private IDisposable _disposable;

        public override void Open()
        {
            if (!UIInventoryItemSlots.Instance.IsReady)
            {
                Close();
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
                var uiItemSlot = UIInventoryItemSlots.Instance.GetUIItemSlot(inventoryItemSlot);
                uiItemSlot.transform.SetParent(uiItemParent);
                disposable.Add(Disposable.Create(() => UIInventoryItemSlots.Instance.ReturnUIItemSlot(uiItemSlot)));
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
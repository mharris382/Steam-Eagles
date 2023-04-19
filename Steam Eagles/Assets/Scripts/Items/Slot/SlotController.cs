using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Items.Slot
{
    public class SlotController : IDisposable
    {
        private readonly ReactiveProperty<InventorySlot> _activeSlot;// = new ReactiveProperty<InventorySlot>();
        private readonly ReactiveProperty<InventorySlot> _selectedSlot = new ReactiveProperty<InventorySlot>();
        
        public IReadOnlyReactiveProperty<InventorySlot> ActiveSlot => _activeSlot;
        public IReadOnlyReactiveProperty<InventorySlot> SelectedSlot => _selectedSlot;

        public readonly List<InventorySlot> slots;
        
        private readonly IDisposable _disposable;
        public SlotController(IEnumerable<InventorySlot> slots)
        {
            CompositeDisposable cd = new CompositeDisposable();
            var inventorySlots = slots as InventorySlot[] ?? slots.ToArray();
            this.slots = new List<InventorySlot>(inventorySlots);
            
            
            IObservable<InventorySlot> active = null;
            IObservable<InventorySlot> selected = null;
            foreach (InventorySlot slot in inventorySlots)
            {
                active = active == null ? slot.StateReadOnly.Where(t => t == SlotState.ACTIVE).Select(_ => slot) : active.Merge(slot.StateReadOnly.Where(t => t == SlotState.ACTIVE).Select(_ => slot));
                selected = selected == null ? slot.StateReadOnly.Where(t => t == SlotState.SELECTED).Select(_ => slot) : selected.Merge(slot.StateReadOnly.Where(t => t == SlotState.SELECTED).Select(_ => slot));
            }

            _activeSlot = new ReactiveProperty<InventorySlot>();
            _selectedSlot = new ReactiveProperty<InventorySlot>();
            active.Subscribe(t =>
            {
                var lastActive = _activeSlot.Value;
                if(lastActive != null) lastActive.State = SlotState.NONE;
                _activeSlot.Value = t;
            });
            selected.Subscribe(t =>
            {
                var lastSelected = _selectedSlot.Value;
                if(lastSelected != null) lastSelected.State = SlotState.NONE;
                _selectedSlot.Value = t;
            });
            _disposable = cd;
        }


        public void Dispose()
        {
            _disposable.Dispose();
            _activeSlot.Dispose();
            _selectedSlot.Dispose();
        }
    }
    
    public enum SlotState
    {
        NONE,
        ACTIVE,
        SELECTED
    }
}
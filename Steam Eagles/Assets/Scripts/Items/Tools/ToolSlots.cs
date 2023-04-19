using System;
using System.Collections.Generic;
using System.Linq;
using Items.Slot;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Items
{
    public class ToolSlots : MonoBehaviour
    {
        [SerializeField]
        List<ToolSlot> toolSlots = new List<ToolSlot>();

        public UnityEvent<Tool> onToolEquipped = new UnityEvent<Tool>();
        
        private LinkedList<ToolSlot> _equippableSlots = new LinkedList<ToolSlot>();
        private LinkedListNode<ToolSlot> _currentSlot;

        public bool HasToolEquipped =>
            _currentSlot != null && _currentSlot.Value != null && _currentSlot.Value.Tool != null;

        public int EquippableToolCount => _equippableSlots.Count;
        public Tool EquippedTool => HasToolEquipped ? _currentSlot.Value.Tool : null;

        public bool activateToolOnStart = true;
        
        private SlotController _slotController;

        private void Start()
        {
            foreach (var toolSlot in toolSlots)
            {
                Debug.Assert(toolSlot.GetComponent<InventorySlot>() != null, "ToolSlot must be have an InventorySlot", toolSlot);
                //rebuild the linked list whenever any tool slot is changed
                toolSlot.OnToolChanged.AsObservable()
                    .Subscribe(_ => UpdateLinkedListOnAnyToolSlotChanged())
                    .AddTo(this);
            }

            _slotController = new SlotController(toolSlots.Select(t => t.GetComponent<InventorySlot>()));
            if (activateToolOnStart && _slotController.ActiveSlot.Value == null)
            {
                Debug.Assert(_slotController.slots.Count > 0, "No tool slots found", this);
                
                var firstSlot = _slotController.slots.First();
                
                Debug.Assert(firstSlot != null, "No tool slots found");
                
                firstSlot.State = SlotState.ACTIVE;
                Debug.Log($"Activating {firstSlot.name}");
            }
            //build the linked list on start
            UpdateLinkedListOnAnyToolSlotChanged();
        }

        void UpdateLinkedListOnAnyToolSlotChanged()
        {
            var currentTool = EquippedTool;
            _equippableSlots.Clear();

            //add all non-empty slots to the linked list
            foreach (var toolSlot in toolSlots)
            {
                if (!toolSlot.IsEmpty)
                {
                    _equippableSlots.AddLast(toolSlot);
                }
            }

            EquipTool(currentTool);
        }

        /// <summary>
        /// tries to equip the slot containing the specified tool
        /// </summary>
        /// <param name="toolToEquip"></param>
        /// <returns>true if the tool was found and equipped, otherwise the tool was not found in the tool slots and could not be equipped</returns>
        public bool EquipTool(Tool toolToEquip)
        {
            // if we have a tool equipped, try to find it in the new list
            var next = _equippableSlots.First;
            if (next == null) return false;
            while (next.Value.Tool != toolToEquip)
            {
                next = next.Next;
                if (next == null)
                {
                    // current tool is not in the list, so we just equip the first one
                    next = _equippableSlots.First;
                    break;
                }
            }

            _currentSlot = next;
            OnEquippedToolChanged();
            return _currentSlot.Value.Tool == toolToEquip;
        }


        public void EquipNextTool()
        {
            if (_equippableSlots.Count == 0) 
                return;
            if (_currentSlot == null)
                _currentSlot = _equippableSlots.First;
            else
                _currentSlot = _currentSlot.Next ?? _equippableSlots.First;

            OnEquippedToolChanged();
        }

        public void EquipPrevTool()
        {
            if (_equippableSlots.Count == 0) 
                return;
            if (_currentSlot == null)
                _currentSlot = _equippableSlots.Last;
            else
                _currentSlot = _currentSlot.Previous ?? _equippableSlots.Last;

            OnEquippedToolChanged();
        }

        public IEnumerable<Tool> GetEquippableTools()
        {
            return _equippableSlots.Select(slot => slot.Tool);
        }

        
        
        void OnEquippedToolChanged()
        {
            if (_currentSlot == null || _currentSlot.Value == null)
            {
                OnEquippedToolChanged(null);
            }
            else
            {
                OnEquippedToolChanged(_currentSlot.Value.Tool);
            }
        }

        void OnEquippedToolChanged(Tool tool)
        {
            if (tool == null && _equippableSlots.Count > 0)
            {
                UpdateLinkedListOnAnyToolSlotChanged();
                return;
            }
            else 
            {
                onToolEquipped?.Invoke(tool);    
            }
            
        }
    }
}
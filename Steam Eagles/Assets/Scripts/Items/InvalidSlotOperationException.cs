using System;
using UnityEngine;

namespace Items
{
    public sealed class InvalidSlotOperationException : InvalidOperationException
    {
        public InvalidSlotOperationException(InventorySlot slot) : base($"Invalid Operation on slot {slot.name}")
        {
            Debug.LogError(message:this.Message, slot);
        }
        public InvalidSlotOperationException(InventorySlot slot, string message) : base($"Invalid Operation on slot {slot.name}. {message}")
        {
            Debug.LogError(message:this.Message, slot);
        }
    }
}
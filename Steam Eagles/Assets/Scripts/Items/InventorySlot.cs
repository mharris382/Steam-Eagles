using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Items
{
    public class InventorySlot : MonoBehaviour
    {
        public IInventory inventory
        {
            get;
            set;
        }

        public bool IsEmpty => itemStack.item == null;


        public UnityEvent<ItemStack> onStackChanged;

        [SerializeField]
        [OnValueChanged(nameof(OnStackChanged))]
        private ItemStack itemStack;

        public ItemStack ItemStack
        {
            get => itemStack;
            private set
            {
                itemStack = value;
                OnStackChanged(value);
            }
        }

        public bool IsSlotEmpty => ItemStack.IsEmpty;

        public ItemBase Item => IsSlotEmpty ? null : ItemStack.item;
        public bool IsSlotStackable => !IsSlotEmpty && Item.IsStackable;
        public int StackSize => IsSlotStackable ? ItemStack.Count : (IsSlotEmpty ? 0 : 1);

        public void OnStackChanged(ItemStack stack)
        {
            if (stack.item == null)
            {
                name = "Empty InventorySlot";
            }
            else
            {
                if (stack.item.IsStackable)
                {
                    name = $"SLOT:{stack.item.name} ({stack.Count})";    
                }
                else
                {
                    name = $"SLOT:{stack.item.name}";
                }
            }
            onStackChanged?.Invoke(stack);
        }


        /// <summary>
        /// sets the item stack in the slot, provided the stack is valid
        /// </summary>
        /// <param name="itemStack"></param>
        /// <returns>true if the slot was changed, false if the provided stack was invalid</returns>
        public bool SetItemStack(ItemStack itemStack)
        {
            if (itemStack.item == null || itemStack.Count <= 0)
            {
                ItemStack = itemStack;
                return true;
            }
            if(!itemStack.item.IsStackable && itemStack.Count > 1)
            {
                Debug.LogError($"Tried to set an item stack with a count greater than the 1 for item {itemStack.item.name}");
                return false;
            }
            if(itemStack.Count > itemStack.item.MaxStackSize)
            {
                Debug.LogError($"Tried to set an item stack with a count greater than the max stack size of item {itemStack.item.name}");
                return false;
            }

            ItemStack = itemStack;
            return true;
        }

        public void SetItemStackSafe(ItemStack itemStack)
        {
            if (itemStack.item == null || itemStack.Count <= 0)
            {
                ItemStack = itemStack;
            }
            else if (!itemStack.item.IsStackable)
            {
                ItemStack = new ItemStack(itemStack.item, 1);
            }
            else if (itemStack.Count > itemStack.item.MaxStackSize)
            {
                ItemStack = new ItemStack(itemStack.item, itemStack.item.MaxStackSize);
            }
            else
            {
                ItemStack = itemStack;
            }
        }


        public bool AddCount(int amount)
        {
            if(IsSlotEmpty)
            {
                return false;
            }
            if(!IsSlotStackable)
            {
                return false;
            }
            int newCount = ItemStack.Count + amount;
            if(newCount > ItemStack.item.MaxStackSize)
            {
                return false;
            }
            ItemStack = new ItemStack(ItemStack.item, newCount);
            return true;
        }
        
        public void SwapSlots(InventorySlot other)
        {
            (ItemStack, other.ItemStack) = (other.ItemStack, ItemStack);
        }
    }
}
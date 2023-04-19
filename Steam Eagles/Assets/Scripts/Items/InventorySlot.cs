using System;
using Items.Slot;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Items
{
    public class InventorySlot : MonoBehaviour, IComparable<InventorySlot>
    {
        public IInventory inventory
        {
            get;
            set;
        }

        public bool IsEmpty => itemStack.item == null;

        
        private ReactiveProperty<bool> _isSlotLoading = new ReactiveProperty<bool>(false);
        private ReactiveProperty<SlotState> _state = new ReactiveProperty<SlotState>(SlotState.NONE);
        
        public ReadOnlyReactiveProperty<SlotState> StateReadOnly => _state.ToReadOnlyReactiveProperty();

        public bool IsSlotLoading
        {
            get => _isSlotLoading.Value;
            set => _isSlotLoading.Value = value;
        }

        public SlotState State
        {
            get => _state.Value;
            set => _state.Value = value;
        }
        
      

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

        public bool IsFull
        {
            get
            {
                if (ItemStack.item == null) return false;
                return ItemStack.Count >= ItemStack.item.MaxStackSize;
            }
        }


        public int GetRemainingStackableSpace()
        {
            if (IsSlotEmpty) return 0;
            if (!IsSlotStackable) return 0;
            return Item.MaxStackSize - ItemStack.Count;
        }
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
        public bool SetItemStack(ItemStack iStack)
        {
            if (itemStack.item == null || itemStack.Count <= 0)
            {
                RaiseEvent();
                ItemStack = iStack;
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
            if(!AllowItem(itemStack.item))
                return false;
            if (!AllowsStack(ItemStack))
            {
                Debug.LogWarning($"Tried to set an item {iStack} stack that was not allowed!", this);
                return false;
            }
            ItemStack = iStack;
            RaiseEvent();
            return true;
        }
        
        public virtual bool AllowsStack(ItemStack stack) => true;
        public virtual bool AllowItem(ItemBase item) => true;
        public bool SetItemStack(ItemBase itemBase, int cnt) => SetItemStack(new ItemStack(itemBase, cnt));
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
            RaiseEvent();
        }

        public void SetItemStackSafe(ItemBase itemBase, int cnt)
        {
            SetItemStackSafe(new ItemStack(itemBase, cnt));
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

        public void Clear()
        {
            SetItemStack(new ItemStack());
        }
        public bool SwapSlots(InventorySlot other)
        {
            if(other.AllowsStack(this.ItemStack) && other.AllowItem(Item) && this.AllowsStack(other.ItemStack) && this.AllowItem(other.Item))
            {
                (ItemStack, other.ItemStack) = (other.ItemStack, ItemStack);
                RaiseEvent();
                other.RaiseEvent();
                return true;
            }
            return false;
        }

        private void RaiseEvent() => this.onStackChanged?.Invoke(this.ItemStack);

        public override string ToString()
        {
            return $"{transform.GetSiblingIndex()} - {name} - {ItemStack}";
        }

        public int CompareTo(InventorySlot other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return itemStack.CompareTo(other.itemStack);
        }
    }


    public class InventorySlot<T> : InventorySlot where T : ItemBase
    {
        public override bool AllowItem(ItemBase item) => item is T;
    }
}
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
            set
            {
                itemStack = value;
                OnStackChanged(value);
            }
        }
        

        void OnStackChanged(ItemStack stack)
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
    }
}
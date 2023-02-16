using Sirenix.OdinInspector;
using UnityEngine;

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


        [OnValueChanged(nameof(OnStackChanged))]
        public ItemStack itemStack;

        

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
        }
    }
}
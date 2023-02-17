using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items.UI
{
    public class UIItemCollectionTester : MonoBehaviour
    {
        [Required]
        public UIItemCollection collection;
        
        [TableList]
        public List<ItemStack> stacks;
        
        
        
        private IEnumerator Start()
        {
            while (!UIInventoryItemSlots.Instance.IsUIReady(collection.slotKey))
            {
                yield return null;
            }

            
            collection.PopulateContainer(stacks);
        }
    }
}
using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items.UI
{
    public class UIInventoryTester : MonoBehaviour
    {
        [Required]
        public UIInventory uiInventory;
        
        private IEnumerator Start()
        {
            while (!UIInventoryItemSlots.Instance.IsReady)
            {
                yield return null;
            }
            Debug.Log("UIInventoryItemSlots is ready! Opening UIInventory...");
            uiInventory.Open();
        }


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (uiInventory.IsVisible)
                {
                    uiInventory.Close();
                }
                else
                {
                    uiInventory.Open();
                }
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                uiInventory.Open();
            }
        }
    }
}
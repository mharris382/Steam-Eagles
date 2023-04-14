using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Items.UI.HUDScrollView
{
    public class ToolRecipeHUDController : MonoBehaviour
    {
        [OnValueChanged(nameof(OnToolChange))]public Tool tool;
        [OnValueChanged(nameof(OnInventoryChange))] public Inventory inventory;

        public HUDFancyScrollView scrollView;

        void OnInventoryChange(Inventory i) => Setup(tool, i);

        void OnToolChange(Tool t) => Setup(t, inventory);

        public bool autoSetup = true;

        

        public void Setup(Tool tool, Inventory characterInventory)
        {
            if(tool ==null|| characterInventory == null)
                return;
            this.tool = tool;
            this.inventory = characterInventory;
            if (tool == null || characterInventory == null)
            {
                scrollView.gameObject.SetActive(false);
                return;
            }

            scrollView.SetContextInfo(tool, inventory);
            scrollView.UpdateData(tool.Recipes.ToList());
            scrollView.gameObject.SetActive(true);
        }

    }
}
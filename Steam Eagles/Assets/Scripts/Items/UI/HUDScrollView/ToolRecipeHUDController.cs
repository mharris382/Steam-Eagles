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
        [Required] public CanvasGroup canvasGroup;
        public HUDFancyScrollView scrollView;

        protected  void OnInventoryChange(Inventory i) => Setup(tool, i);

        protected void OnToolChange(Tool t) => Setup(t, inventory);

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
            scrollView.SelectCell(0);
        }

        
    }
}
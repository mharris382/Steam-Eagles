using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Items.UI
{
    public class ItemTooltipWindow : TooltipWindow
    {
        public UIItem tooltipUI;
        protected override bool IsValidSelectableForTooltip(GameObject selectable)
        {
            var uiItem = selectable.GetComponent<UIItem>();
            if (uiItem == null)
            {
                return false;
            }
            return !uiItem.IsEmpty && !uiItem.DisplayedStack.IsEmpty;
        }

        protected override void UpdateContent(GameObject selected)
        {
            var itemStack = selected.GetComponent<UIItem>().DisplayedStack;
            tooltipUI.DisplayStack(itemStack);
        }

        protected override void OnBecameVisible()
        {
            tooltipUI.IsVisible = true;
        }

        protected override void OnBecameHidden()
        {
            tooltipUI.IsVisible = false;
        }
    }
}
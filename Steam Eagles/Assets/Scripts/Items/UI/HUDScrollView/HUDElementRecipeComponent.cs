using System;
using System.Drawing;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Items.UI.HUDScrollView
{
    public class HUDElementRecipeComponent : MonoBehaviour
    {
        public Color validColor = Color.white;
        public Color invalidColor = Color.red;

        public UnityEngine.UI.Image iconImage;
        public TextMeshProUGUI amountText;
        
        [InfoBox("Use {0} for required amount and {1} for total amount")]
        public string textFormat = "{0}/{1}";
        
        public void Show(HUDToolRecipeContext context, ItemStack componentStack)
        {
            if (componentStack.item == null)
            {
                Debug.LogError("Cannot show null item",this);
                return;
            }
            var count = context.Inventory.GetItemCount(componentStack.item);
            var hasEnough = count >= componentStack.Count;
            var color = hasEnough ? validColor : invalidColor;
            string s = string.Format(textFormat, componentStack.Count, count);
            amountText.text = s;
            amountText.color = color;
            iconImage.sprite = componentStack.item.icon;
        }
    }
}
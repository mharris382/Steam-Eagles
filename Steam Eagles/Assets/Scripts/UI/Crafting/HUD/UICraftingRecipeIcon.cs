using Items;
using UnityEngine.UI;

namespace UI.Crafting.HUD
{
    public class UICraftingRecipeIcon : UICraftingCellListener
    {
        public Image iconImage;
        public override void UpdateCellContents(Recipe recipe, CategoryScrollContext context)
        {
            iconImage.sprite = recipe.GetIcon();
        }
    }
}
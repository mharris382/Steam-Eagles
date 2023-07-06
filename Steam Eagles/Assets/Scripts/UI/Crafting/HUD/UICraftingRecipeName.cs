using Items;
using TMPro;

namespace UI.Crafting.HUD
{
    public class UICraftingRecipeName : UICraftingCellListener
    {
        public TextMeshProUGUI nameText;
        public override void UpdateCellContents(Recipe recipe, CategoryScrollContext context)
        {
            nameText.text = recipe.name;
        }
    }
}
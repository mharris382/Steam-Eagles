using Items;
using UnityEngine;

namespace UI.Crafting.HUD
{
    public abstract class UICraftingCellListener : MonoBehaviour
    {
        public abstract void UpdateCellContents(Recipe recipe, CategoryScrollContext context);
    }
}
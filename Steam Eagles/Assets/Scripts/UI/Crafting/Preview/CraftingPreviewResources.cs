using Sirenix.Utilities;
using UnityEngine;

namespace UI.Crafting
{
    [GlobalConfig("Resources/CraftingPreviewResources")]
    public class CraftingPreviewResources : GlobalConfig<CraftingPreviewResources>
    {
        public Color validColor = Color.green;
        public Color invalidColor = Color.red;
        public Color destructColor = Color.yellow;
        public Sprite destructionSprite;
        public SpriteRenderer tilePreviewSprite;
    }
}
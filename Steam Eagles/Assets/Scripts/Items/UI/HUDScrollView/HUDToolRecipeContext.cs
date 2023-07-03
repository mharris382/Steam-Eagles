using System.Collections.Generic;

namespace Items.UI.HUDScrollView
{
    public class HUDToolRecipeContext
    {
        public int SelectedIndex { get; set; }
        public Tool Tool { get; set; }
        public Inventory Inventory { get; set; }
        
        public Recipe Recipe { get; set; }
        public List<Recipe> Recipes { get; set; }
    }

    
}
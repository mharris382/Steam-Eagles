using UnityEngine;

namespace Items.SaveLoad
{
    
    
    public class CharacterInventory : MonoBehaviour
    {
        public static string[] characterInventories { get; } = new string[2]
        {
            "Inventory",
            "ToolBelt"
        };
        
    }
}
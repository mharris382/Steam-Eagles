using Sirenix.OdinInspector;
using UnityEngine;
#if ODIN_INSPECTOR
#endif
namespace Characters
{
    /// <summary>
    /// creates a concrete connection between the Player's Camera, Avatar, Input, and Movement
    /// </summary>
    [CreateAssetMenu(menuName = "Steam Eagles/Player", order = -102)]
    public class Player : ScriptableObject
    {
        [Range(0,1)]
        public int playerNumber = 0;

#if ODIN_INSPECTOR
        [ValueDropdown(nameof(GetCharacterTags))]
#endif
        public string characterTag = "Builder";
        
        
#if ODIN_INSPECTOR
        ValueDropdownList<string> GetCharacterTags()
        {
            var vdl = new ValueDropdownList<string>();
            vdl.Add(new ValueDropdownItem<string>("Builder", "Builder"));
            vdl.Add(new ValueDropdownItem<string>("Transporter", "Transporter"));
            return vdl;
        }
#endif
    }

    [CreateAssetMenu(menuName = "Steam Eagles/Save Slot", order = -101)]
    public class SaveSlot : ScriptableObject
    {
        public int saveSlotNumber = 0;
        
    }
}
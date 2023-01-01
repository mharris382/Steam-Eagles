using Characters;
using Sirenix.OdinInspector;
using StateMachine;
using UnityEngine;
#if ODIN_INSPECTOR
#endif
namespace Players
{
    /// <summary>
    /// creates a concrete connection between the Player's Camera, Avatar, Input, and Movement
    /// </summary>
    [CreateAssetMenu(menuName = "Steam Eagles/Player", order = -102)]
    public class Player : ScriptableObject
    {
        [Range(0,1)] public int playerNumber = 0;
        
        [ValueDropdown(nameof(GetCharacterTags))] public string characterTag = "Builder";
        
        
        public SharedTransform characterTransform;
        
        
        
        
        public void AssignPlayer(PlayerCharacterInput playerCharacterInput, Camera assignedCamera, CharacterState assignedCharacter)
        {
            
        }




        #region [Editor]

#if ODIN_INSPECTOR
        ValueDropdownList<string> GetCharacterTags()
        {
            var vdl = new ValueDropdownList<string>();
            vdl.Add(new ValueDropdownItem<string>("Builder", "Builder"));
            vdl.Add(new ValueDropdownItem<string>("Transporter", "Transporter"));
            return vdl;
        }
#else
        void GetCharacterTags(){}
#endif
  #endregion
    }


    public class PlayerSaveData
    {
        public int playerNumber;
        
        public PlayerSaveData()
        {
            
        }
    }

    [CreateAssetMenu(menuName = "Steam Eagles/Save Slot", order = -101)]
    public class SaveSlot : ScriptableObject
    {
        public int saveSlotNumber = 0;
        
    }
}
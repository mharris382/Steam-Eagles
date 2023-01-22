using Characters;
using CoreLib;
using Sirenix.OdinInspector;
using StateMachine;
using UnityEngine;
using UnityEngine.Tilemaps;
using World;
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
        
        
        public SharedTilemap foundationTilemap;
        public SharedTilemap wallTilemap;
        public SharedTilemap solidTilemap;
        public SharedTilemap pipeTilemap;
        public SharedTilemap decorTilemap;
        
        public PlayerCharacterInput CharacterInput { get; private set; }
        public CharacterState State { get; private set; }
        
        
        public void AssignPlayer(PlayerCharacterInput playerCharacterInput, SharedCamera assignedCamera, CharacterState assignedCharacter)
        {
            this.State = assignedCharacter;
            this.CharacterInput = playerCharacterInput;
            
        }

        public void EnableStructureEditing(EditableTilemapStructure structure)
        {
            pipeTilemap.Value = structure.pipeTilemap;
            solidTilemap.Value = structure.solidTilemap;
            wallTilemap.Value = structure.wallTilemap;
            foundationTilemap.Value = structure.foundationTilemap;
            decorTilemap.Value = structure.decorTilemap;
        }

        public void DisableStructureEditing()
        {
            foundationTilemap.Value = null;
            wallTilemap.Value = null;
            solidTilemap.Value = null;
            pipeTilemap.Value = null;
            decorTilemap.Value = null;
        } 
        
        
        public void SetCharacterInput(PlayerCharacterInput characterInput)
        {
            this.CharacterInput = characterInput;
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
using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using CoreLib.SharedVariables;
using Sirenix.OdinInspector;
using SteamEagles.Characters;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
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
        [System.Serializable]
        public class Events
        {
            public UnityEvent<PlayerInput> onPlayerInputAssigned;
            public UnityEvent<Transform> onPlayerCharacterChanged;

            private PlayerInput _lastInput;

            public void Update(Player player)
            {
                onPlayerCharacterChanged?.Invoke(player.characterTransform.Value);
                var playerInput = player.PlayerInput;
                if (_lastInput != playerInput)
                {
                    _lastInput = playerInput;
                    onPlayerInputAssigned?.Invoke(playerInput);
                }
            }
        }
        
        [System.Serializable]
        public class Tilemaps
        {
            [SerializeField, Required] public SharedTilemap foundationTilemap;
            [SerializeField, Required] public SharedTilemap wallTilemap;
            [SerializeField, Required] public SharedTilemap solidTilemap;
            [SerializeField, Required] public SharedTilemap pipeTilemap;
            [SerializeField, Required] public SharedTilemap decorTilemap;
        }
        
        [Range(0, 1)] public int playerNumber = 0;

        //TODO: rename this to default character, and add a character selection system
        [ValueDropdown(nameof(GetCharacterTags))]
        public string characterTag = "Builder";


        public Events events;
        public Tilemaps tilemaps;
        public SharedTransform characterTransform;
        public SharedCamera playerCamera;




        #region [TILEMAPS]

        public SharedTilemap foundationTilemap => tilemaps.foundationTilemap;

        public SharedTilemap wallTilemap => tilemaps.wallTilemap;

        public SharedTilemap solidTilemap => tilemaps.solidTilemap;

        public SharedTilemap pipeTilemap => tilemaps.solidTilemap;

        public SharedTilemap decorTilemap => tilemaps.decorTilemap;

        #endregion

        public PlayerInputWrapper InputWrapper { get; private set; }

        public PlayerInput PlayerInput => InputWrapper == null ? null : InputWrapper.PlayerInput;
        public CharacterState State { get; private set; }

        
        private Dictionary<Action<Camera>, IDisposable> _cameraSubscriptions = new Dictionary<Action<Camera>, IDisposable>();
        
        //TODO: write test for this event property
        public event Action<Camera> OnCameraChanged
        {
            add
            {
                if(_cameraSubscriptions.ContainsKey(value)) return;
                _cameraSubscriptions.Add(value,playerCamera.onValueChanged.AsObservable().Subscribe(value));
            }
            remove
            {
                if(_cameraSubscriptions.ContainsKey(value))
                {
                    _cameraSubscriptions[value].Dispose();
                    _cameraSubscriptions.Remove(value);
                }
            }
        }

        public void AssignPlayer(PlayerInputWrapper playerInputWrapper,CharacterState assignedCharacterState)
        {
            this.State = assignedCharacterState;
            this.InputWrapper = playerInputWrapper;
            playerInputWrapper.Assign(State.GetComponent<CharacterInputState>());
            events.Update(this);
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
        
        
        public void SetCharacterInput(PlayerInputWrapper inputWrapper)
        {
            this.InputWrapper = inputWrapper;
        }


        public Tilemap SolidTilemap
        {
            get => solidTilemap.Value;
            set => solidTilemap.Value = value;
        }

        public Tilemap PipeTilemap
        {
            get => pipeTilemap.Value;
            set => pipeTilemap.Value = value;
        }
        
        public Tilemap WallTilemap
        {
            get => wallTilemap.Value;
            set => wallTilemap.Value = value;
        }
        
        public Tilemap DecorTilemap
        {
            get => decorTilemap.Value;
            set => decorTilemap.Value = value;
        }
        
        public Tilemap FoundationTilemap
        {
            get => foundationTilemap.Value;
            set => foundationTilemap.Value = value;
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
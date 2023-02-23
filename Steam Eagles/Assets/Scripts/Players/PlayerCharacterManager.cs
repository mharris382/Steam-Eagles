using System;
using System.Collections.Generic;
using System.Linq;
using Characters;
using CoreLib.SharedVariables;
using Players;
using StateMachine;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerCharacterManager : MonoBehaviour
    {
        [SerializeField] private PlayerWrapper[] playerWrappers;
        [SerializeField] private CameraAssignments cameraAssignments;
        [SerializeField] private List<SharedTransform> playerInputs;
        [SerializeField] List<CharacterAssignment> characterAssignments;
        
        private PlayerInputManager _inputManager;
        
        
        
        [SerializeField] private List<Player> players; 
        
        
        
        [Obsolete("the player character manager should not be handling character spawning and character assignment")]
        private LinkedList<CharacterInputState> _unassignedCharacters = new LinkedList<CharacterInputState>();

        private Dictionary<CharacterInputState, CharacterAssignment> _characterAssignmentLookup =
            new Dictionary<CharacterInputState, CharacterAssignment>();
        
        private Dictionary<PlayerInputWrapper, CharacterInputState> _joinedPlayers = new Dictionary<PlayerInputWrapper, CharacterInputState>();
        private Dictionary<CharacterInputState, PlayerInputWrapper> _assignedCharacters = new Dictionary<CharacterInputState, PlayerInputWrapper>();

       
        [Obsolete("this can be removed, instead use the playerInputManager's limit number of players")]
        private Queue<PlayerInputWrapper> _unassignedPlayers = new Queue<PlayerInputWrapper>(); //TODO: this can be removed, instead use the playerInputManager's limit number of players
        
        
     
        
        private void Awake()
        {
            _inputManager = GetComponent<PlayerInputManager>();
            _inputManager.onPlayerJoined += OnPlayerJoined;
            _inputManager.onPlayerLeft += OnPlayerLeft;
            SpawnCharacters();
            //Invoke(nameof(SpawnCharacters), 0.1f);
        }

        void SpawnCharacters()
        {
            foreach (var characterAssignment in characterAssignments)
            {
                var character = characterAssignment.InstantiateCharacter();
                
                character.OnDestroyAsObservable().Select(t => (character, characterAssignment))
                    .TakeUntilDestroy(this).Subscribe(OnCharacterDestroyed);
                
                Enqueue(character);
            }
        }
        private void OnPlayerLeft(PlayerInput obj)
        {
            Debug.Log($"Player left, {obj.playerIndex}");
            var characterInput = GetCharacterInput(obj);
            if (characterInput == null) return;
            
            PlayerInputWrapper playerInputWrapper = GetCharacterInput(obj);
            
            
            //if the player is assigned to a character, remove them from the dictionary
            UnAssignCharacterToPlayer(playerInputWrapper);
        }

        private void OnPlayerJoined(PlayerInput obj)
        {
            var characterInput = GetCharacterInput(obj);
            
            AssignCharacterToPlayer(characterInput);
            players[obj.playerIndex].SetCharacterInput(characterInput);
        }

        private void AssignCharacterToPlayer(PlayerInputWrapper playerInputWrapper, CharacterInputState characterInputState = null)
        {
            characterInputState = characterInputState == null ? Dequeue(): characterInputState;
            characterInputState.gameObject.SetActive(true);
            _joinedPlayers.Add(playerInputWrapper, characterInputState);
            _assignedCharacters.Add(characterInputState, playerInputWrapper);
            playerInputWrapper.Assign(characterInputState);
            Debug.Log($"Assigned {characterInputState.name} to Player #{playerInputWrapper.PlayerInput.playerIndex}");
        }
        
        private void UnAssignCharacterToPlayer(PlayerInputWrapper playerInputWrapper)
        {
            if (!_joinedPlayers.ContainsKey(playerInputWrapper)) return;
            
            CharacterInputState characterInputState = _joinedPlayers[playerInputWrapper];
            playerInputWrapper.UnAssign(characterInputState);
            
            _joinedPlayers.Remove(playerInputWrapper);
            _assignedCharacters.Remove(characterInputState);
            
            Enqueue(characterInputState);
            if (_unassignedPlayers.Count > 0) AssignCharacterToPlayer(_unassignedPlayers.Dequeue());
            Debug.Log($"Unassigned Character {characterInputState.name} from Player #{playerInputWrapper.PlayerInput.playerIndex}");
        }
       

        /// <summary>
        /// respawns and reassigns a character if the character GameObject is destroyed for some reason.  the character
        /// </summary>
        /// <param name="destroyedCharacter"></param>
        private void OnCharacterDestroyed((CharacterInputState obj, CharacterAssignment prefab) destroyedCharacter)
        {
            //check if assigned character exists in dictionary
            if(!_assignedCharacters.ContainsKey(destroyedCharacter.obj)) return;
            var player = _assignedCharacters[destroyedCharacter.obj];
            UnAssignCharacterToPlayer(player);
            
            _assignedCharacters.Remove(destroyedCharacter.obj);
            
            _joinedPlayers[player] = destroyedCharacter.prefab.InstantiateCharacter();
            _assignedCharacters.Add(_joinedPlayers[player], player);
            
        }

        
        #region [Helper Methods]


        /// <summary>
        /// gets PlayerCharacterInput component if exists, otherwise adds it
        /// </summary>
        /// <param name="playerInput"></param>
        /// <returns></returns>
        public PlayerInputWrapper GetCharacterInput(PlayerInput playerInput)
        {
            PlayerInputWrapper inputWrapper;
            if (!playerInput.gameObject.TryGetComponent(out inputWrapper))
            {
                inputWrapper = playerInput.gameObject.AddComponent<PlayerInputWrapper>();
            }

            var player = playerInputs.FirstOrDefault(t => !t.HasValue || t.Value == playerInput.transform);
            if (player == null)
            {
                player = playerInputs[0];
            }

            player.Value = playerInput.transform;

            return inputWrapper;
        }

        
        
        private  CharacterInputState Dequeue()
        {
            var ret = _unassignedCharacters.First.Value;
            _unassignedCharacters.RemoveFirst();
            ret.gameObject.SetActive(true);
            return ret;
        }
        
        private void Enqueue(CharacterInputState characterInputState)
        {
           // characterInputState.gameObject.SetActive(false);
            _unassignedCharacters.AddLast(characterInputState);
        }

        

        #endregion
   
        #region [Editor Gizmos]

        public void OnDrawGizmos()
        {
            foreach (var characterAssignment in characterAssignments)
            {
                characterAssignment.OnDrawGizmos();
            }
        }
        

        #endregion
        
           
        private void ChangeAssignment(PlayerInputWrapper playerInputWrapper, CharacterAssignment newCharacterAssignment)
        {
            if (!_joinedPlayers.ContainsKey(playerInputWrapper)) return;
            
            CharacterInputState characterInputState = _joinedPlayers[playerInputWrapper];
            playerInputWrapper.UnAssign(characterInputState);
            
            _joinedPlayers.Remove(playerInputWrapper);
            _assignedCharacters.Remove(characterInputState);
            
            Enqueue(characterInputState);
            AssignCharacterToPlayer(playerInputWrapper, newCharacterAssignment.InstantiateCharacter());
            
            Debug.Log($"Changed Character {characterInputState.name} to {newCharacterAssignment.characterName} for Player #{playerInputWrapper.PlayerInput.playerIndex}");
        }

    }
}
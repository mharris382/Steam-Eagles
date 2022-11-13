using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using StateMachine;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace Characters
{
    
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerCharacterManager : MonoBehaviour
    {




        [SerializeField] private List<SharedTransform> playerInputs;
        [SerializeField] List<CharacterAssignment> characterAssignments;
        
        private PlayerInputManager _inputManager;
        private LinkedList<CharacterInputState> _unassignedCharacters = new LinkedList<CharacterInputState>();
        private Dictionary<PlayerCharacterInput, CharacterInputState> _joinedPlayers = new Dictionary<PlayerCharacterInput, CharacterInputState>();
        private Dictionary<CharacterInputState, PlayerCharacterInput> _assignedCharacters = new Dictionary<CharacterInputState, PlayerCharacterInput>();

       
        [Obsolete("this can be removed, instead use the playerInputManager's limit number of players")]
        private Queue<PlayerCharacterInput> _unassignedPlayers = new Queue<PlayerCharacterInput>(); //TODO: this can be removed, instead use the playerInputManager's limit number of players
        
        
     
        
        private void Awake()
        {
            _inputManager = GetComponent<PlayerInputManager>();
            
            _inputManager.onPlayerJoined += OnPlayerJoined;
            _inputManager.onPlayerLeft += OnPlayerLeft;
            
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
            
            PlayerCharacterInput playerCharacterInput = GetCharacterInput(obj);
            
            
            //if the player is assigned to a character, remove them from the dictionary
            UnAssignCharacterToPlayer(playerCharacterInput);
        }

        private void OnPlayerJoined(PlayerInput obj)
        {
            var characterInput = GetCharacterInput(obj);
            if(_unassignedCharacters.Count == 0)
            {
                _unassignedPlayers.Enqueue(characterInput);
                return;
            }
            AssignCharacterToPlayer(characterInput);
        }

        private void AssignCharacterToPlayer(PlayerCharacterInput playerCharacterInput, CharacterInputState characterInputState = null)
        {
            //if there are no more characters to assign, wait for a character to be unassigned
            if (_unassignedCharacters.Count == 0)
            {
                _unassignedPlayers.Enqueue(playerCharacterInput);
                return;
            }

            characterInputState = characterInputState == null ? Dequeue(): characterInputState;
            _joinedPlayers.Add(playerCharacterInput, characterInputState);
            _assignedCharacters.Add(characterInputState, playerCharacterInput);
            playerCharacterInput.Assign(characterInputState);
            Debug.Log($"Assigned {characterInputState.name} to Player #{playerCharacterInput.PlayerInput.playerIndex}");
        }
        
        private void UnAssignCharacterToPlayer(PlayerCharacterInput playerCharacterInput)
        {
            if (!_joinedPlayers.ContainsKey(playerCharacterInput)) return;
            
            CharacterInputState characterInputState = _joinedPlayers[playerCharacterInput];
            playerCharacterInput.UnAssign(characterInputState);
            
            _joinedPlayers.Remove(playerCharacterInput);
            _assignedCharacters.Remove(characterInputState);
            
            Enqueue(characterInputState);
            if (_unassignedPlayers.Count > 0) AssignCharacterToPlayer(_unassignedPlayers.Dequeue());
            
            Debug.Log($"Unassigned Character {characterInputState.name} from Player #{playerCharacterInput.PlayerInput.playerIndex}");
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
        public PlayerCharacterInput GetCharacterInput(PlayerInput playerInput)
        {
            PlayerCharacterInput characterInput;
            if (!playerInput.gameObject.TryGetComponent(out characterInput))
            {
                characterInput = playerInput.gameObject.AddComponent<PlayerCharacterInput>();
            }

            var player = playerInputs.FirstOrDefault(t => !t.HasValue || t.Value == playerInput.transform);
            if (player == null)
            {
                player = playerInputs[0];
            }

            player.Value = playerInput.transform;

            return characterInput;
        }

        [Obsolete("Should never have spectating players")]
        private bool IsPlayerSpectating(PlayerCharacterInput playerCharacterInput)
        {
            return _unassignedPlayers.Contains(playerCharacterInput);
        }
        
        
        private  CharacterInputState Dequeue()
        {
            var ret = _unassignedCharacters.First.Value;
            _unassignedCharacters.RemoveFirst();
            return ret;
        }
        
        private void Enqueue(CharacterInputState characterInputState)
        {
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
        
           
        private void ChangeAssignment(PlayerCharacterInput playerCharacterInput, CharacterAssignment newCharacterAssignment)
        {
            if (!_joinedPlayers.ContainsKey(playerCharacterInput)) return;
            
            CharacterInputState characterInputState = _joinedPlayers[playerCharacterInput];
            playerCharacterInput.UnAssign(characterInputState);
            
            _joinedPlayers.Remove(playerCharacterInput);
            _assignedCharacters.Remove(characterInputState);
            
            Enqueue(characterInputState);
            AssignCharacterToPlayer(playerCharacterInput, newCharacterAssignment.InstantiateCharacter());
            
            Debug.Log($"Changed Character {characterInputState.name} to {newCharacterAssignment.characterName} for Player #{playerCharacterInput.PlayerInput.playerIndex}");
        }

    }   
    
    
        [Serializable]
        public class CharacterAssignment
        {
            public string characterName;
            [SerializeField] CharacterState prefab;
            [Tooltip("This variable is required to support the dynamic split-screen feature")]
            public SharedCamera playerCamera;
            
            [Header("Spawn Position")]
            public SharedTransform spawnPoint;
            public Vector3 defaultPosition;
            
            [Header("Debugging")]
            public Color characterColor = Color.red;
            public bool hideTransformInEditor = false;
            
            
            
            private CharacterInputState _spawnedCharacter;
            
            
            
            public Vector3 SpawnPosition => SpawnTransform.position;

            
            
            
            /// <summary>
            /// used to move the character's spawn point in the world
            /// </summary>
            public Transform SpawnTransform
            {
                get
                {
                    if (spawnPoint == null) spawnPoint = ScriptableObject.CreateInstance<SharedTransform>();
                    if (!spawnPoint.HasValue)
                    {
                        spawnPoint.Value = new GameObject($"{characterName} Spawn Transform").transform;
                        if(hideTransformInEditor)spawnPoint.Value.hideFlags = HideFlags.HideInHierarchy;
                        spawnPoint.Value.position = defaultPosition;
                        spawnPoint.name = $"{characterName} Spawn Transform";
                    }
                    return spawnPoint.Value;
                }
            }

            public CharacterInputState InstantiateCharacter()
            {
                if (_spawnedCharacter != null) return _spawnedCharacter;
                var character = Object.Instantiate(prefab, SpawnPosition, Quaternion.identity);
                character.name = characterName;
                CharacterInputState characterInputState;
                if (!character.gameObject.TryGetComponent(out characterInputState))
                {
                    characterInputState = character.gameObject.AddComponent<CharacterInputState>();
                }
                _spawnedCharacter = characterInputState;
                return characterInputState;
            }
        
            
            public void DestroyCharacter(CharacterState character)
            {
                Object.Destroy(character.gameObject);
            }
            
            public void MoveCharacterToSpawn(CharacterState character)
            {
                character.transform.position = SpawnPosition;
            }
            
            public void ResetSpawnPosition()
            {
                SpawnTransform.position = defaultPosition;
            }
            
            public void OnDrawGizmos()
            {
                Gizmos.color = characterColor;
                Gizmos.DrawSphere(defaultPosition, 0.125f);
            }
        }
}

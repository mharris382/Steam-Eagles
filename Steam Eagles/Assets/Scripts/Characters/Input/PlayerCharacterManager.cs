using System;
using System.Collections.Generic;
using CoreLib;
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
        [SerializeField] List<CharacterAssignment> characterAssignments;

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
            public Vector3 SpawnPosition => SpawnTransform.position;
            
            /// <summary>
            /// used to move the character's spawn point in the world
            /// </summary>
            public Transform SpawnTransform
            {
                get
                {
                    if (spawnPoint == null)
                    {
                        spawnPoint = ScriptableObject.CreateInstance<SharedTransform>();
                        
                    }

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

            void ResetSpawnPosition()
            {
                SpawnTransform.position = defaultPosition;
            }
            
            
            public CharacterInputState InstantiateCharacter()
            {
                var character = Instantiate(prefab, SpawnPosition, Quaternion.identity);
                character.name = characterName;
                CharacterInputState characterInputState;
                if (!character.gameObject.TryGetComponent(out characterInputState))
                {
                    characterInputState = character.gameObject.AddComponent<CharacterInputState>();
                }
                
                return characterInputState;
            }
            public void DestroyCharacter(CharacterState character)
            {
                Destroy(character.gameObject);
            }
            public void MoveCharacterToSpawn(CharacterState character)
            {
                character.transform.position = SpawnPosition;
            }
            

            public void OnDrawGizmos()
            {
                Gizmos.color = characterColor;
                Gizmos.DrawSphere(defaultPosition, 0.125f);
            }
        }

        
        private LinkedList<CharacterInputState> _unassignedCharacters = new LinkedList<CharacterInputState>();
        private Queue<PlayerCharacterInput> _unassignedPlayers = new Queue<PlayerCharacterInput>();
        
        private Dictionary<PlayerCharacterInput, CharacterInputState> _joinedPlayers = new Dictionary<PlayerCharacterInput, CharacterInputState>();
        private Dictionary<CharacterInputState, PlayerCharacterInput> _assignedCharacters = new Dictionary<CharacterInputState, PlayerCharacterInput>();

        private PlayerInputManager _inputManager;
        
        bool AllCharacterAssigned => _unassignedCharacters.Count == 0;

        private bool IsPlayerSpectating(PlayerCharacterInput playerCharacterInput)
        {
            return _unassignedPlayers.Contains(playerCharacterInput);
        }
        
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

        void OnCharacterDestroyed((CharacterInputState obj, CharacterAssignment prefab) destroyedCharacter)
        {
            var player = _assignedCharacters[destroyedCharacter.obj];
            UnAssignCharacterToPlayer(player);
            
            _assignedCharacters.Remove(destroyedCharacter.obj);
            
            _joinedPlayers[player] = destroyedCharacter.prefab.InstantiateCharacter();
            _assignedCharacters.Add(_joinedPlayers[player], player);
            
        }

        private void OnPlayerLeft(PlayerInput obj)
        {
            var characterInput = GetCharacterInput(obj);
            if (characterInput == null) return;
            
            PlayerCharacterInput playerCharacterInput = GetCharacterInput(obj);
            
            //if the player is spectating, remove them from the queue
            if (IsPlayerSpectating(playerCharacterInput))
            {
                for (int i = 0; i < _unassignedPlayers.Count; i++)
                {
                    var p = _unassignedPlayers.Dequeue();
                    if(p == playerCharacterInput)continue;
                    _unassignedPlayers.Enqueue(p);
                }
                return;
            }
            
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

        void AssignCharacterToPlayer(PlayerCharacterInput playerCharacterInput, CharacterInputState characterInputState = null)
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
        
        CharacterInputState Dequeue()
        {
            var ret = _unassignedCharacters.First.Value;
            _unassignedCharacters.RemoveFirst();
            return ret;
        }
        
        void Enqueue(CharacterInputState characterInputState)
        {
            _unassignedCharacters.AddLast(characterInputState);
        }

        void UnAssignCharacterToPlayer(PlayerCharacterInput playerCharacterInput)
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
        
        
        void ChangeAssignment(PlayerCharacterInput playerCharacterInput, CharacterAssignment newCharacterAssignment)
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
        
        public PlayerCharacterInput GetCharacterInput(PlayerInput playerInput)
        {
            PlayerCharacterInput characterInput;
            if (!playerInput.gameObject.TryGetComponent(out characterInput))
            {
                characterInput = playerInput.gameObject.AddComponent<PlayerCharacterInput>();
            }

            return characterInput;
        }
    }   
}

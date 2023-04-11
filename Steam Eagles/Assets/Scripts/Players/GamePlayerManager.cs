using System;
using System.Collections.Generic;
using Characters;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using Game;

namespace Players
{
    /// <summary>
    /// this needs to be majorly refactored.  this class is a butt to deal with
    /// TODO: refactor GameManager and character spawning
    /// </summary>
    public class GamePlayerManager : MonoBehaviour
    {
        public enum GameMode
        {
            SINGLEPLAYER,
            LOCAL_COOP
        }

        [SerializeField] private GameMode gameMode;
        [Required] public SharedInt activePlayerCount;
        
        [ValidateInput(nameof(ValidateWrappers))]
        [BoxGroup("Players")]  public List<PlayerWrapper> players;
        [BoxGroup("Players")]  public CameraAssignments cameraAssignments;
        [HideInInspector]
        [Obsolete("handled external prefab loader systems")]
        [BoxGroup("Players")]  public CharacterAssignments characterAssignments;
        [BoxGroup("Players")] public EventSystemAssignments eventSystemAssignments;
        
        public GameObject waitingForFirstPlayerToJoinGUI;
        public GameObject playerDisconnectedGUI;
        public Subject<Unit> _update = new Subject<Unit>();
        private ReactiveCollection<PlayerWrapper> _joinedPlayers = new ReactiveCollection<PlayerWrapper>();


        public bool debug = false;
        IDisposable _singlePlayerCameraSubscription;
        IPlayerDependencyResolver<Camera> CameraResolver => cameraAssignments;

        private void Awake()
        {
            activePlayerCount.Value = 0;
            MessageBroker.Default.Receive<RequestPlayerCharacterSpawn>().Subscribe(OnSpawnPlayerRequested).AddTo(this);
        }

        private void Start()
        {
            waitingForFirstPlayerToJoinGUI.SetActive(true);
            
            _joinedPlayers.ObserveAdd()
                .Subscribe(t =>
                {
                    HideMessageGUIs();
                })
                .AddTo(this);

            _joinedPlayers.ObserveCountChanged()
                .Where(t => t == 0)
                .Subscribe(_ =>
                {
                    waitingForFirstPlayerToJoinGUI.gameObject.SetActive(true);
                })
                .AddTo(this);
            _joinedPlayers.ObserveCountChanged()
                .Where(t => t == 1)
                .Subscribe(_ =>
                {
                    HideMessageGUIs();
                    SwitchToSinglePlayerCamera();
                })
                .AddTo(this);

            _joinedPlayers.ObserveCountChanged()
                .Where(t => t > 1)
                .Subscribe(_ =>
                {
                    HideMessageGUIs();
                })
                .AddTo(this);
            
            _joinedPlayers.ObserveCountChanged().Subscribe(cnt => activePlayerCount.Value = cnt).AddTo(this);
        }

        bool ValidateWrappers(List<PlayerWrapper> wrappers)
        {
            if (wrappers.Count < 2) return false;
            for (int i = 0; i < wrappers.Count; i++)
            {
                if (wrappers[i] == null) return false;
                if(wrappers[i].player == null) return false;
                if(wrappers[i].player.characterTransform==null) return false;
                if(wrappers[i].player.playerNumber != i) return false;
            }
            return true;
        }


        [System.Obsolete("Device registration should not be handled by GameManager, it needs to be handled by PlayerDeviceManager")]
        public void OnPlayerLeft(PlayerInput obj)
        {
            Debug.Log($"Player {obj.playerIndex} Left");
            _joinedPlayers.Remove(players[obj.playerIndex]);
        }

        public void OnSpawnPlayerRequested(RequestPlayerCharacterSpawn request)
        {
            var building = GameObject.FindGameObjectWithTag("Building");
            Debug.Assert(building != null, "No building found in scene", this);

           var characterInstance = SetupPlayer(request.characterPrefab, request.playerCharacterIndex, building, request.spawnPositionLocal);
           var assignmentNotification = new CharacterAssignedPlayerInputInfo() {
                characterName = request.characterName,
                characterState = characterInstance,
                inputGo = GameManager.Instance.GetPlayerDevice(request.playerCharacterIndex),
                playerNumber = request.playerCharacterIndex
           };
            MessageBroker.Default.Publish(assignmentNotification);
        }

        CharacterState SetupPlayer(GameObject prefab, int id, GameObject parent, Vector2 localOffset)
        {
            var wrapper = players[id];
            var obj = GameManager.Instance.GetPlayerDevice(id);
            Debug.Assert(obj != null, "Player Device was null", this);
            PlayerInputWrapper input ;//= obj.GetComponent<PlayerInputWrapper>();

            if (!obj.TryGetComponent(out input))
            {
                input = obj.AddComponent<PlayerInputWrapper>();
            }
                
            var camera = cameraAssignments.GetDependency(id);
            Debug.Assert(camera.Value != null, "Camera Assignment was returned with null camera!", this);
            Debug.Assert(wrapper.player.playerCamera == camera, "wrapper.player.playerCamera != camera", this);

            
            var character = Instantiate(prefab,parent.transform).GetComponent<CharacterState>();
            character.transform.localPosition = localOffset;
            
            Debug.Assert(character.CompareTag(wrapper.player.characterTag), $"Player {wrapper.player} assigned the wrong character {character.name} or Character tag is incorrect", this);
            MessageBroker.Default.Publish(new CharacterSpawnedInfo(character.tag, character.gameObject));
            //TODO: remove this, character is bound to input not the other way around
            wrapper.BindToPlayer(input);
            wrapper.AssignCamera(camera.Value);
            wrapper.AssignCharacter(character);
            Debug.Assert(wrapper.IsFullyBound);
                
            _joinedPlayers.Add(wrapper);
            SwitchToSinglePlayerCamera();
            return character;
        }
        public void OnPlayerJoined(PlayerInput obj)
        {
            return;
            var id = obj.playerIndex;
            switch (gameMode)
            {
                case GameMode.SINGLEPLAYER:
                    
                    if (_joinedPlayers.Count == 0)
                    {
                        
                    }
                    break;
                case GameMode.LOCAL_COOP:
                    if (_joinedPlayers.Count > 0)
                    {
                        SwitchToMultiPlayerCamera();
                    }
                    try
                    {
                        var wrapper = players[id];
                        var input = obj.GetComponent<PlayerInputWrapper>();
                
                        var camera = cameraAssignments.GetDependency(id);
                        Debug.Assert(camera.Value != null, "Camera Assignment was returned with null camera!", this);
                        Debug.Assert(wrapper.player.playerCamera == camera, "wrapper.player.playerCamera != camera", this);
                
                        var character = characterAssignments.GetDependency(id);
                        Debug.Assert(character.CompareTag(wrapper.player.characterTag), $"Player {wrapper.player} assigned the wrong character {character.name} or Character tag is incorrect", this);
                
                        wrapper.BindToPlayer(input);
                        wrapper.AssignCamera(camera.Value);
                        wrapper.AssignCharacter(character);
                        Debug.Assert(wrapper.IsFullyBound);
                
                        _joinedPlayers.Add(wrapper);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Player {obj.playerIndex} failed to join\n{e.GetType()} \n{e.Message}");
                        throw;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
           
        }


        private void SwitchToSinglePlayerCamera()
        {
            if (_singlePlayerCameraSubscription != null)
            {
                _singlePlayerCameraSubscription.Dispose();
                _singlePlayerCameraSubscription = null;
            }

            var cd = new CompositeDisposable();
            
            cameraAssignments.DisableMultiPlayerCameras().AddTo(cd);
            _joinedPlayers[0].OverrideCamera(CameraResolver.GetDependency(0)).AddTo(cd);
            
            _singlePlayerCameraSubscription = cd;
        }

        private void SwitchToMultiPlayerCamera()
        {
            if (_singlePlayerCameraSubscription != null)
            {
                _singlePlayerCameraSubscription.Dispose();
                _singlePlayerCameraSubscription = null;
            }
        }
        

        private void HideMessageGUIs()
        {
            waitingForFirstPlayerToJoinGUI.gameObject.SetActive(false);
            playerDisconnectedGUI.gameObject.SetActive(false);
        }

        private void Update()
        {
            _update.OnNext(Unit.Default);
        }
    }


    [System.Obsolete("replace these IPlayerDependencyResolver nonsense")]
    [Serializable]
    public class CharacterAssignments : IPlayerDependencyResolver<CharacterState>
    {
        [SerializeField] public List<CharacterAssignment> characterAssignments;
        
        public CharacterState GetDependency(int playerNumber)
        {
            var assignment = characterAssignments[playerNumber];
            var character = assignment.InstantiateCharacter();
            return character.GetComponent<CharacterState>();
        }
    }


    [System.Obsolete("replace these IPlayerDependencyResolver nonsense")]
    [Serializable]
    public class EventSystemAssignments : IPlayerDependencyResolver<InputSystemUIInputModule>
    {
        public InputSystemUIInputModule[] eventSystems;
        public InputSystemUIInputModule GetDependency(int playerNumber)
        {
            var eventSystem = eventSystems[playerNumber];
            eventSystem.gameObject.SetActive(true);
            return eventSystem;
        }
    }
    
    
    
}


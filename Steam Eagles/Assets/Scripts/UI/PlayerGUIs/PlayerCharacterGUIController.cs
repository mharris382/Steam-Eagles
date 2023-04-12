using System;
using CoreLib;
using CoreLib.Entities;
using Game;
using Players.Shared;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using PlayerInput = UnityEngine.InputSystem.PlayerInput;
// ReSharper disable InconsistentNaming

namespace UI.PlayerGUIs
{
    public class PlayerCharacterGUIController : MonoBehaviour
    {
        [OnValueChanged(nameof(OnPlayerIDChanged)), Range(0, 1), SerializeField]
        private int playerID;

        [SerializeField, ChildGameObjectsOnly, Required,
         Tooltip("Root window of the player GUI. Holds all other windows")]
        private PlayerCharacterGUIWindowRoot rootWindow;
        

        private ReactiveProperty<bool> _isGuiActive = new ReactiveProperty<bool>(false);
        private IDisposable _entityListener;
        private Entity _pcEntity;
        private PlayerInput _playerInput;

        private PlayerInput playerInput
        {
            get => _playerInput;
            set
            {
                _playerInput = value;
                UpdateGUIActiveState();
            }
        }

        private Entity pcEntity
        {
            get => _pcEntity;
            set
            {
                _pcEntity = value;
                UpdateGUIActiveState();
            }
        }

        /// <summary> game object that represents the player character </summary>
        private GameObject PlayerCharacter => pcEntity != null ? pcEntity.gameObject : null;

        
        

        /// <summary>
        /// awake has two steps.
        /// 1. check for any existing objects and update player input and character
        /// 2. subscribe to events to object changes so that controller can reactively update player gui
        /// </summary>
        private void Awake()
        {
            var playerAlreadyJoined = GameManager.Instance.PlayerHasJoined(playerID);
            if (playerAlreadyJoined)
            {
                var existingPlayer = GameManager.Instance.GetPlayerDevice(playerID).GetComponent<PlayerInput>();
                UpdatePlayerInput(existingPlayer);
            }

            var playerAlreadyAssigned = GameManager.Instance.PlayerHasCharacterAssigned(playerID);
            if (playerAlreadyAssigned)
            {
                var characterName = GameManager.Instance.GetPlayerCharacterName(playerID);
                UpdatePlayerControlledCharacter(characterName);
            }

            if (GameManager.Instance.PlayerHasCharacterAssigned(playerID))
            {
                var entityListener = EntityManager.Instance.GetEntityProperty(GameManager.Instance.GetPlayerCharacterName(playerID));
                var entityAlreadyExists = entityListener.Value != null;
                if (entityAlreadyExists)
                {
                    var entity = entityListener.Value;
                    UpdateEntity(entity);
                }
            }
           
            
            //react to player joining
            MessageBroker.Default.Receive<PlayerDeviceJoined>()
                .Where(t => t.Index == playerID)
                .Select(t => t.PlayerInput.GetComponent<PlayerInput>())
                .Where(t => t != null)
                .Subscribe(UpdatePlayerInput).AddTo(this);

            //react to player device lost (either left or accidental disconnect)
            MessageBroker.Default.Receive<PlayerDeviceLost>()
                .Where(t => t.Index == playerID)
                .Subscribe(_ => UpdatePlayerInput(null))
                .AddTo(this);
            
            //react to player character being assigned
            
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>()
                .Where(t => t.playerNumber == playerID)
                .Select(t=> t.characterName)
                .Subscribe(UpdatePlayerControlledCharacter).AddTo(this);
            
            MessageBroker.Default.Receive<CharacterAssignedPlayerInputInfo>()
                .Where(t=> t.playerNumber == playerID)
                .Select(t => EntityManager.Instance.GetEntityProperty(t.characterName))
                .Switch().Subscribe(UpdateEntity).AddTo(this);
          
        }

        
        private void Start()
        {
            _isGuiActive.Subscribe(rootWindow.SetWindowVisible).AddTo(this);
            _isGuiActive.Subscribe(active =>
            {
                if (active) rootWindow.SetWindowActive(playerInput, pcEntity, PlayerCharacter);
                else rootWindow.DisableWindow();
            }).AddTo(this);
        }
        
        private void OnDestroy()
        {
            if (_entityListener != null)
            {
                _entityListener.Dispose();
                _entityListener = null;
            }
        }

        private void UpdateGUIActiveState() => _isGuiActive.Value = HasAllResources();
        public bool HasAllResources() => pcEntity!=null && PlayerCharacter != null && playerInput != null;

        #region [Event Listeners]
        
        /// <summary>
        /// called whenever player is assigned to a playable character. This could be either because
        /// the player switched controlled characters, or the player loaded to this character from a
        /// save file, or because the player selected this character as their starting character in the
        /// new game panel of main menu
        /// </summary>
        /// <param name="characterName"></param>
        private void UpdatePlayerControlledCharacter(string characterName)
        {
            if (_entityListener != null)
            {
                _entityListener.Dispose();
                _entityListener = null;
            }
            var entityListener = EntityManager.Instance.GetEntityProperty(characterName);
            var entityAlreadyExists = entityListener != null;
            if (entityAlreadyExists)
            {
                var entity = entityListener.Value;
                UpdateEntity(entity);
            }

            _entityListener=  entityListener.Subscribe(e => pcEntity = e);
        }


        /// <summary>
        /// called whenever the player controlled character is spawned and assigned to an entity (may also be called
        /// when the player controlled character is destroyed and the entity is set to null)
        /// </summary>
        /// <param name="entity"></param>
        private void UpdateEntity(Entity entity)
        {
            if (entity == null)
            {
                //TODO: hide player GUI until entity is spawned again
            }
            else if (pcEntity == null && entity != null)
            {
                //TODO: show player GUI from hidden state
            }
            pcEntity = entity;
            if (_entityListener != null)
            {
                _entityListener.Dispose();
                _entityListener = null;
            }

            //always update active state if linked game object changes
            if (entity != null)
                _entityListener = entity.LinkedGameObjectProperty.Subscribe(_ => UpdateGUIActiveState());

            UpdateGUIActiveState();
        }

        /// <summary>
        /// called when player is assigned a device by pressing a button on the device. Also called if the device
        /// is unexpectedly lost (e.g. unplugged).
        /// </summary>
        /// <param name="existingPlayer"></param>
        private void UpdatePlayerInput(PlayerInput existingPlayer)
        {
            if (existingPlayer == null)
            {
                //TODO: display device lost message
            }
            playerInput = existingPlayer;
        }

        #endregion

        
        #region [Editor]

        void OnPlayerIDChanged(int value) => name = $"PLAYER {value} GUI";


        [BoxGroup("Debug")]
        [ShowInInspector]
        public bool IsActive
        {
            get
            {
                if (!Application.isPlaying)
                    return false;
                if(_isGuiActive == null)_isGuiActive = new ReactiveProperty<bool>(false);
                return _isGuiActive.Value;
            }
        }
        
        #endregion
    }
}
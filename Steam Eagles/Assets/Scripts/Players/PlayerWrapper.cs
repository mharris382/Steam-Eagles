using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Players;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Characters
{
    public class PlayerWrapper : MonoBehaviour
    {
        private static List<PlayerWrapper> _playerWrappers;
        private static List<PlayerWrapper> PlayerWrappers => _playerWrappers ??= (_playerWrappers = new List<PlayerWrapper>());
        
        [InlineEditor()]
        public Player player;

        private CharacterState _characterState;
        private PlayerInputWrapper _playerInputWrapper;
        private Camera _playerCamera;
        private InputSystemUIInputModule _eventSystem;
        private GameObject _localMultiplayerUIRoot;

        public static event Action<Player> onPlayerInitialized;
        public bool IsInputBound => _playerInputWrapper != null;

        public bool IssCameraBound => _playerCamera != null;

        public bool IsCharacterBound => _characterState != null;
        
        public bool IsUIModuleBound => _eventSystem != null;
        public bool IsUIRootBound => _localMultiplayerUIRoot != null;

        public bool IsFullyBound => IsInputBound && IssCameraBound && IsCharacterBound;// && IsUIModuleBound;

        private void Awake()
        {
            if (player == null)
            {
                Debug.LogWarning("Player Wrapper does not have player asset assigned!", this);
                return;
            }
            
        }

        private IEnumerator Start()
        {
            while (player == null)
            {
                yield return null;
            }
        }


        public IDisposable OverrideCamera(Camera overrideCamera)
        {
            if (!IsFullyBound)
            {
                Debug.LogError("PlayerWrapper is not fully bound!");
                return null;
            }

            if (overrideCamera == null)
            {
                return null;
            }
            player.playerCamera.Value = overrideCamera;
            _playerCamera.gameObject.SetActive(false);
            return Disposable.Create(() =>
            {
                _playerCamera.gameObject.SetActive(true);
                player.playerCamera.Value = _playerCamera;
            });
        }
        

        void OnCameraChanged(Camera camera)
        {
            if (IssCameraBound && camera == null)
                player.playerCamera.Value = _playerCamera;
        }

        public void AssignCharacter(CharacterState state)
        {
            this._characterState = state;
            InitializeIfFullyBound();
        }

        public void AssignCamera(Camera camera)
        {
            _playerCamera = camera;
            InitializeIfFullyBound();
        }

        public void AssignUIModule(InputSystemUIInputModule uiInputModule)
        {
            
        }

        public void BindToPlayer(PlayerInputWrapper playerInputWrapper)
        {
            _playerInputWrapper =playerInputWrapper;
            
            InitializeIfFullyBound();
        }

        public void InitializeIfFullyBound()
        {
            void InitCharacter()
            {
                player.characterTransform.Value = _characterState.transform;
            }

            void InitInput()
            {
                var characterInputState = _characterState.GetComponent<CharacterInputState>();
                _playerInputWrapper.Assign(characterInputState);
                player.AssignPlayer(_playerInputWrapper, _characterState);
                MessageBroker.Default.Publish(new PlayerJoinedInfo()
                {
                    playerNumber = player.playerNumber,
                    playerObject = player
                });
            }

            //ensure player has camera bound at all times
            void InitCam()
            {
                if(!player.playerCamera.HasValue)
                    player.playerCamera.Value = _playerCamera;
                player.playerCamera.onValueChanged.AsObservable().Subscribe(OnCameraChanged).AddTo(this);
            }

            void InitEventSystem()
            {
                
            }

            
            if (IsFullyBound)
            {
                InitCam();
                InitCharacter();
                InitInput();
                onPlayerInitialized?.Invoke(player);
            }
            else
            {
                if(!IssCameraBound) Debug.Log($"Player {player.playerNumber} Still waiting for camera", player);
                if(!IsCharacterBound) Debug.Log($"Player {player.playerNumber} Still waiting for character", player);
                if(!IsInputBound) Debug.Log($"Player {player.playerNumber} Still waiting for input", player);
            }
        }
}
}
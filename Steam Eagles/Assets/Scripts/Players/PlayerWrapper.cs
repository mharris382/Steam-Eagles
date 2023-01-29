using System;
using CoreLib;
using Players;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters
{
    public class PlayerWrapper : MonoBehaviour
    {
        [InlineEditor()]
        public Player player;

        private CharacterState _characterState;
        private PlayerCharacterInput _playerCharacterInput;
        private Camera _playerCamera;


        public bool IsInputBound => _playerCharacterInput != null;

        public bool IssCameraBound => _playerCamera != null;

        public bool IsCharacterBound => _characterState != null;

        public bool IsFullyBound => IsInputBound && IssCameraBound && IsCharacterBound;

        private void Awake()
        {
           
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

        public void BindToPlayer(PlayerCharacterInput playerInput)
        {
            _playerCharacterInput =playerInput;
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
                _playerCharacterInput.Assign(characterInputState);
                player.AssignPlayer(_playerCharacterInput, _characterState);
            }

            //ensure player has camera bound at all times
            void InitCam()
            {
                if(!player.playerCamera.HasValue)
                    player.playerCamera.Value = _playerCamera;
                player.playerCamera.onValueChanged.AsObservable().Subscribe(OnCameraChanged).AddTo(this);
            }

            
            if (IsFullyBound)
            {
                InitCam();
                InitCharacter();
                InitInput();
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
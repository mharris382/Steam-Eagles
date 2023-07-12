using System.Collections;
using UI.PlayerGUIs;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI
{
   
    public abstract class UIPlayerSystemBase : MonoBehaviour
    {
        private bool _hasStarted;
        
        private PlayerCharacterGUIController _controller;
        private PlayerCharacterGUIController Controller => _controller ? _controller : _controller = GetComponentInParent<PlayerCharacterGUIController>();

        protected PlayerInput PlayerInput => Controller.playerInput;
        protected GameObject PlayerCharacter => Controller.PlayerCharacter;
        protected Camera PlayerCamera => Controller.PlayerCamera;
        
        
        private IEnumerator Start()
        {
            while (!Controller.HasAllResources())
                yield return new WaitForSeconds(5f);
            Controller.PlayerCharacterProperty.Subscribe(OnCharacterChanged).AddTo(this);
            Controller.PlayerInputProperty.Subscribe(OnPlayerInputChanged).AddTo(this);
            Controller.PlayerCameraProperty.Subscribe(OnCameraChanged).AddTo(this);
            _hasStarted = true;
            OnGameStarted(Controller.playerInput, Controller.PlayerCharacter, Controller.PlayerCamera);
        }
        

        protected abstract void OnPlayerJoined(PlayerInput playerInput);
        protected abstract void OnGameStarted(PlayerInput playerInput, GameObject character, Camera camera);
        protected virtual void OnCameraChanged(Camera playerCamera) { }
        protected virtual void OnPlayerInputChanged(PlayerInput playerInput) { }
        protected virtual void OnCharacterChanged(GameObject character) { }
        
    }
}
using CoreLib.Signals;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace UI
{
    public class UIPauseGame : Window
    {
        [Required]
        public GameObject defaultSelectable;
        private PlayerInput _openedByPlayerInput;
        public UIWindowPanel windowPanel;

        private void Awake()
        {
            MessageBroker.Default.Receive<PlayerPressedPausedSignal>().Subscribe(OnPlayerPressedPause).AddTo(this);
            Close();
        }

        private void OnPlayerPressedPause(PlayerPressedPausedSignal pressedPausedSignal)
        {
            var pressedInput = pressedPausedSignal.PlayerInput as PlayerInput;
            if (IsVisible)
            {
               CloseBy(pressedInput);
            }
            else
            {
                Open();
                _openedByPlayerInput = pressedInput;
                var go = GetSelectable(pressedInput);
                windowPanel.OnSelected(go);
                EventSystem.current.SetSelectedGameObject(go);
            }
        }

        private void CloseBy(PlayerInput pressedInput)
        {
            if (pressedInput != _openedByPlayerInput)
            {
                return;
            }
            this.SetLastSelectable(pressedInput, EventSystem.current.currentSelectedGameObject);
            Close();
            _openedByPlayerInput = null;
         
        }

        GameObject GetSelectable(PlayerInput playerInput)
        {
            return this.GetLastSelectable(playerInput, defaultSelectable.gameObject) ?? defaultSelectable;
        }

        public void ResumeButton()
        {
            if (_openedByPlayerInput == null)
            {
                Close();
                return;
            }
            CloseBy(_openedByPlayerInput);
            Close();
        }
    }
}
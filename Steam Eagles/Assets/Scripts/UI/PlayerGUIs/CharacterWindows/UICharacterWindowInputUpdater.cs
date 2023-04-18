using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs.CharacterWindows
{
    /// <summary>
    /// changes current input to gameplay when all windows are closed and to UI when any window is open.
    /// uses <see cref="UICharacterWindowClosed"/> messages to determine which window is open. 
    /// </summary>
    public class UICharacterWindowInputUpdater : MonoBehaviour
    {
        ReactiveProperty<UICharacterWindowBase> _lastOpenedWindow;
        private PlayerInput _lastPlayerInput;
        private void Awake()
        {
            _lastOpenedWindow = new ReactiveProperty<UICharacterWindowBase>(null);
            
            MessageBroker.Default.Receive<UICharacterWindowClosed>()
                .Where(t => t.CharacterWindow == _lastOpenedWindow.Value)
                .Subscribe(OnWindowClosed).AddTo(this);
            
            MessageBroker.Default.Receive<UICharacterWindowOpened>()
                .Subscribe(OnWindowOpened).AddTo(this);

            _lastOpenedWindow.Where(t => _lastPlayerInput != null)
                .Select(t => t != null)
                .Subscribe(OnWindowStateChanged)
                .AddTo(this);
        }

        private void OnWindowStateChanged(bool b)
        {
            string actionMapName = b ? "UI" : "Gameplay";
            _lastPlayerInput.SwitchCurrentActionMap(actionMapName);
            Debug.Log($"Switching {_lastPlayerInput.name} to {actionMapName}");
        }

        void OnWindowClosed(UICharacterWindowClosed closed)
        {
            if (closed.CharacterWindow == _lastOpenedWindow.Value)
            {
                _lastOpenedWindow.Value = null;
            }
        }
        void OnWindowOpened(UICharacterWindowOpened opened)
        {
            _lastOpenedWindow.Value = opened.CharacterWindow;
            _lastPlayerInput = opened.PlayerInput;
            
        }
    }
}
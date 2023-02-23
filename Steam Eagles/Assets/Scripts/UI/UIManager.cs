using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        public EventSystem eventSystem;
        [Required] public Transform canvas;
        public string quitToDesktopMessagePrompt = "Are you sure you want to quit?";
        public string quitToMainMenuMessagePrompt = "Are you sure you want to quit?";
        
        private QuitAction _quitToMainMenuAction;
        private QuitAction _quitToDesktopAction;



        private Selectable _lastSelectable;
        
        private class QuitAction
        {
            private readonly string _message;
            private readonly Action _yesAction;

            private ConfirmationWindow _window;
            private readonly UIManager _uiManager;

            public QuitAction(string msg, Action yesAction, ConfirmationWindow window, UIManager uiManager)
            {
                this._message = msg;
                _yesAction = yesAction;
                _window = window;
                _uiManager = uiManager;
            }
            public void OnButtonPressed()
            {
                _window.Show(_message, _yesAction, OnCancel);
                _window.SelectNo(_uiManager.eventSystem);
                _window.RectTransform.anchoredPosition = Vector2.zero;
                _window.SetWindowVisible(true);
            }

            private void OnCancel()
            {
                _window.ClearListeners();
                _window.SetWindowVisible(false);
                if (_uiManager._lastSelectable != null)
                {
                    _uiManager.eventSystem.SetSelectedGameObject(_uiManager._lastSelectable.gameObject);
                }
            }
        }


        private void CreateQuitActions()
        {
            void QuitToMainMenu()
            {
                SceneManager.LoadScene(0);
            }
            void QuitToDesktop()
            {
                Application.Quit();
            }

            var confirmationWindow = GetConfirmationWindow();
            confirmationWindow.SetWindowVisible(false);
            _quitToMainMenuAction = new QuitAction(quitToMainMenuMessagePrompt, QuitToMainMenu, confirmationWindow, this);
            _quitToDesktopAction = new QuitAction(quitToDesktopMessagePrompt, QuitToDesktop, confirmationWindow, this);
        }
        
        private ConfirmationWindow GetConfirmationWindow()
        {
            var confirmationWindow = WindowLoader.GetWindow<ConfirmationWindow>();
            confirmationWindow.transform.SetParent(canvas);
            confirmationWindow.RectTransform.anchoredPosition = Vector2.zero;
            return confirmationWindow;
        }

        public void SetLastSelectable(Selectable selectable)
        {
            _lastSelectable = selectable;
        }
        
        public void OnQuitToMainMenu()
        {
            if (WindowLoader.Instance.IsFinishedLoading())
            {
                if(_quitToMainMenuAction == null)
                    CreateQuitActions();
                _quitToMainMenuAction.OnButtonPressed();
            }
            else
            {
                Debug.LogError("WindowManager is not finished loading!", this);
            }
        }
        
        public void OnQuitButton()
        {
            if (WindowLoader.Instance.IsFinishedLoading())
            {
                if(_quitToDesktopAction == null)
                    CreateQuitActions();
                _quitToDesktopAction.OnButtonPressed();
            }
            else
            {
                Debug.LogError("WindowManager is not finished loading!", this);
            }
        }
    }
}
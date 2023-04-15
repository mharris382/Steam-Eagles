using CoreLib;
using CoreLib.Signals;
using SaveLoad;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using Observable = UnityEngine.InputSystem.Utilities.Observable;

namespace UI
{
    public class UIPauseGame : Window
    {
        [Required]
        public GameObject defaultSelectable;
        private PlayerInput _openedByPlayerInput;
        public UIWindowPanel windowPanel;
        [Required]
        public Animator pauseAnimator;

        private static readonly int IsOpen = Animator.StringToHash("IsOpen");

        public override void Open()
        {
            base.Open();
            pauseAnimator.SetBool(IsOpen, true);
        }
        
        public override void Close()
        {
            base.Close();
            pauseAnimator.SetBool(IsOpen, false);
        }

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
        
        public void QuitButton()
        {
            
            using (UniRx.Observable.FromEvent<string>(
                       t => PersistenceManager.Instance.GameSaved += t,
                       t => PersistenceManager.Instance.GameSaved -= t).Subscribe((_ =>
                   {
                        Debug.Log("Save Completed!");
                        Application.Quit();
                   })))
            {
                MessageBroker.Default.Publish(new SaveGameRequestedInfo(PersistenceManager.SavePath));
            }
        }

        public void QuitToMainMenu()
        {
            using (UniRx.Observable.FromEvent<string>(
                       t => PersistenceManager.Instance.GameSaved += t,
                       t => PersistenceManager.Instance.GameSaved -= t).Subscribe((_ =>
                   {
                       Debug.Log("Save Completed!");
                       SceneManager.LoadScene(0);
                       Close();
                   })))
            {
                MessageBroker.Default.Publish(new SaveGameRequestedInfo(PersistenceManager.SavePath));
            }
        }
    }
}
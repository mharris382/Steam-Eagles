﻿using System;
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
using Zenject;
using Observable = UnityEngine.InputSystem.Utilities.Observable;

namespace UI
{
    public class UIPauseGame : Window
    {
        public int quitToMainMenuSceneIndex = 0;
        [Required]
        public GameObject defaultSelectable;
        private PlayerInput _openedByPlayerInput;
        public UIWindowPanel windowPanel;
        [Required]
        public Animator pauseAnimator;

        private static readonly int IsOpen = Animator.StringToHash("IsOpen");
        private GlobalSaveLoader saveLoader;
        private GlobalSavePath savePath;


        [Inject]
        public void InjectMe(GlobalSaveLoader saveLoader, GlobalSavePath savePath)
        {
            this. saveLoader = saveLoader;
            this.savePath = savePath;
        }
        
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
                Debug.Assert(_openedByPlayerInput != null);
                _openedByPlayerInput.SwitchCurrentActionMap("Gameplay");
               CloseBy(pressedInput);
            }
            else
            {
                Open();
                Debug.Assert(_openedByPlayerInput != null, "Should always be opened by a player input");
                _openedByPlayerInput = pressedInput;
                _openedByPlayerInput.SwitchCurrentActionMap("UI");
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
            PersistenceManager.Instance.GameSaved += s =>
            {
                Debug.Log($"Saving and quitting: {s}");
                Application.Quit();
            };
            throw new NotImplementedException();
            MessageBroker.Default.Publish(new SaveGameRequestedInfo(PersistenceManager.SavePath));
        }

        public void QuitToMainMenu()
        {
            saveLoader.SaveGame(res =>
            {
                Debug.Assert(res, $"Failed to save game at path: {savePath.FullSaveDirectoryPath}");
                SceneManager.LoadScene("Main Menu");
            });
        }
    }
}
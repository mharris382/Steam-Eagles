using System;
using System.Collections;
using System.Diagnostics;
using CoreLib;
using CoreLib.Entities;
using CoreLib.Signals;
using Cysharp.Threading.Tasks;
using SaveLoad;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using Zenject;
using Debug = UnityEngine.Debug;
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
        private EntitySaveHandler entitySaveHandler;


        [Inject]
        public void InjectMe(GlobalSaveLoader saveLoader, GlobalSavePath savePath, EntitySaveHandler entitySaveHandler)
        {
            this.entitySaveHandler = entitySaveHandler;
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
            Debug.Log("Saving and quiting to main menu");
            StartCoroutine(SaveAndQuit());
        }

        IEnumerator SaveAndQuit()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Debug.Log($"Starting Entity Save {savePath.FullSaveDirectoryPath}");
                var entitySaveSuccess = await entitySaveHandler.SaveEntities();
                Debug.Log($"Finished Entity Save in {sw.ElapsedMilliseconds} ms");
                sw.Reset();
                sw.Start();
                Debug.Log($"Starting Standard Save");
                var success = await saveLoader.SaveGameAsync();
                sw.Stop();
                Debug.Log($"Finished Standard Save in {sw.ElapsedMilliseconds} ms");
                if (success && entitySaveSuccess)
                {
                    Debug.Log("Save successful!");
                    SceneManager.LoadScene("Main Menu");
                }
            });
        }
    }
}
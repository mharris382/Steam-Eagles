using System;
using System.Collections;
using System.IO;
using CoreLib;
using CoreLib.GameTime;
using CoreLib.SaveLoad;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using UnityEngine.Events;
using Zenject;

namespace UI
{
    public class SaveMenuController : MonoBehaviour
    {
        [Required] public EventSystem eventSystem;
        [Required] public Transform promptCanvas;
        [Required] public LoadMenuController loadDisplays;

        [Required] public Button saveButton;
        [Required] public TMP_InputField saveNameInput;
        private CanvasGroup _loadDisplayCanvasGroup;

        public bool promptForOverwrite = true;
        public bool useDefaultSaveName = true;
        public string defaultSaveName = "New Save";
     
        
        public UIDisplayGameTime timeDisplay;
        public UIDisplayGameDate dateDisplay;
        private WindowLoader _windowLoader;
        
        private ConfirmationPromptID _overwriteSavePromptID;
        public UnityEvent onFinishedSaving;
        private UIPromptBuilder _promptBuilder;


        [Inject] void InjectMe(WindowLoader windowLoader, UIPromptBuilder promptBuilder)
        {
            _windowLoader = windowLoader;
            _promptBuilder = promptBuilder;
        }

        private void Awake()
        {
            if (!loadDisplays.gameObject.TryGetComponent<CanvasGroup>(out var cg))
            {
                cg = loadDisplays.gameObject.AddComponent<CanvasGroup>();
            }
            _loadDisplayCanvasGroup = cg;
        }

        private IEnumerator Start()
        {
            while (_windowLoader.IsFinishedLoading() == false)
            {
                yield return null;
            }

            _overwriteSavePromptID = _promptBuilder.CreatePrompt(promptCanvas, eventSystem,
                () => saveNameInput,
                "Are you sure you want to overwrite this save?");
            
            saveButton.onClick.AddListener(OnSavePressed);
        }

        private void OnEnable()
        {
            _loadDisplayCanvasGroup.alpha = 1;
            _loadDisplayCanvasGroup.interactable = false;
            _loadDisplayCanvasGroup.blocksRaycasts = false;
            _loadDisplayCanvasGroup.enabled = true;
            saveNameInput.text = useDefaultSaveName ? defaultSaveName : "";
            var currentGameTime = TimeManager.Instance.CurrentGameTime;
            timeDisplay.Display(TimeManager.Instance.CurrentGameTime);
            dateDisplay.Display(TimeManager.Instance.CurrentGameDate);
            TimeManager.Instance.isGameTimePaused = true;
        }

        private void OnDisable()
        {
            _loadDisplayCanvasGroup.alpha = 0;
            _loadDisplayCanvasGroup.enabled = false;
            TimeManager.Instance.isGameTimePaused = false;
        }


        private string CurrentSaveName => saveNameInput.text;
        
        public string GetPathForSaveName(string saveName)
        {
            return $"{Application.persistentDataPath}/{saveName}";
        }


        public void OnSavePressed()
        {
            if (WillSaveOverwrite(CurrentSaveName) && promptForOverwrite)
            {
                StartCoroutine(PromptForOverwrite(SaveGame));
            }
            else
            {
                SaveGame();
            }
        }

        private void SaveGame()
        {
            throw new NotImplementedException();
            MessageBroker.Default.Publish(new SaveGameRequestedInfo(GetPathForSaveName(CurrentSaveName)));
            onFinishedSaving.Invoke();
        }
        
        public bool WillSaveOverwrite(string saveName)
        {
            var path = GetPathForSaveName(saveName);
            if (Directory.Exists(path))
            {
                return true;
            }

            return false;
        }


        public IEnumerator PromptForOverwrite(Action confirmOverwrite)
        {
            bool waiting = true;
            _promptBuilder.ShowPrompt(_overwriteSavePromptID)
                .Subscribe(
                    res =>
                    {
                        if (res)
                        {
                            confirmOverwrite();
                        }
                    },
                    _ => waiting = false, 
                    () => waiting = false);
            while (waiting)
            {
                yield return null;
            }
            yield break;
        }
    }
}
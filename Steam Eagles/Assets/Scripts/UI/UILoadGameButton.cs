using System;
using System.IO;
using CoreLib;
using CoreLib.GameTime;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    /// <summary>
    /// displays a button that when pressed loads game into a specific save file. Should be populated in the LoadMenuPage
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UILoadGameButton : MonoBehaviour
    {
        public Button _button;
        public Button loadButton => _button != null ? _button : _button = GetComponent<Button>();
        private TextMeshProUGUI _text;
        public TextMeshProUGUI Text => _text != null ? _text : _text = GetComponentInChildren<TextMeshProUGUI>();

        public UIDisplayGameTime timeDisplay;
        public UIDisplayGameDate dateDisplay;

        public string savePath;
        private GlobalSaveLoader _saveLoader;
        private CoroutineCaller _coroutineCaller;
        private bool _isLoading;
        public string GetFullSavePath()
        {
            return !savePath.StartsWith(Application.persistentDataPath)
                ? Path.Combine(Application.persistentDataPath, savePath)
                : savePath;
        }

        [Inject]
        public void InjectSaver(GlobalSaveLoader saveLoader, CoroutineCaller coroutineCaller)
        {
            this._saveLoader = saveLoader;
            _coroutineCaller = coroutineCaller;
        }

        protected virtual void Awake()
        {
            loadButton.interactable = true;
            loadButton.onClick.AsObservable().Where(_ => !_isLoading).Subscribe(_ => TriggerLoadGame(GetFullSavePath()));
        }

        void TriggerLoadGame(string loadPath)
        {
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                _isLoading = true;
                var result = await _saveLoader.LoadGameAsync(loadPath);
                if (result) Debug.Log($"Load successful: {loadPath.Bolded().InItalics()}");
                else Debug.LogError($"Load unsuccessful: {loadPath.Bolded().InItalics()}");
                _isLoading = false;
            }));
        }
        
        public void SetSavePath(string path)
        {
            savePath = path;
            Text.text = Path.GetFileName(path);
            var dateTime = TimeManager.GetTimeSavedAtPath(path);
            timeDisplay.Display(dateTime.gameTime);
            dateDisplay.Display(dateTime.gameDate);
        }
    }
}
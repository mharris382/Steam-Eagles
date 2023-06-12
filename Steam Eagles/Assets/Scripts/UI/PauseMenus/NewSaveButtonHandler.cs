using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI.PauseMenus
{
    public class NewSaveButtonHandler : IInitializable, IDisposable
    {
        private readonly Button _button;
        private readonly TMP_InputField _inputField;
        private readonly SavePathButtonsList _pathButtonsList;
        private readonly GlobalSaveLoader _globalSaveLoader;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly List<char> _invalidCharacters;
        private CompositeDisposable _disposable;
        private ReadOnlyReactiveProperty<bool> _saveNameIsValid;

        public NewSaveButtonHandler(Button button, 
            TMP_InputField inputField,
            SavePathButtonsList pathButtonsList,
            GlobalSaveLoader globalSaveLoader,
            CoroutineCaller coroutineCaller)
        {
            _button = button;
            _inputField = inputField;
            _pathButtonsList = pathButtonsList;
            _globalSaveLoader = globalSaveLoader;
            _coroutineCaller = coroutineCaller;
            _invalidCharacters = new List<char>(new[] { '#', '/', '\\', '%','*', '?', '<','>', '|', ':', '"', '{','}' });
        }

        public void Initialize()
        {
            var cd = new CompositeDisposable();
            var inputFieldStream = _inputField.onValueChanged.AsObservable().Select(FixInvalidChars);
            var saveGameButtonStream = _button.onClick.AsObservable().Select(_ => _inputField.text).Where(SaveNameIsValid);
            var validInputFieldStream = inputFieldStream.Where(SaveNameIsValid);
            var isValidInputStream = inputFieldStream.Select(SaveNameIsValid);
            
            validInputFieldStream.Subscribe(_inputField.SetTextWithoutNotify).AddTo(cd);
            isValidInputStream.Subscribe(isValid => _button.interactable = isValid).AddTo(cd);
            saveGameButtonStream.Subscribe(TriggerSave).AddTo(cd);
            
            this._disposable = cd;
        }

        string FixInvalidChars(string input)
        {
            foreach (var invalidCharacter in _invalidCharacters)
                if (input.Contains(invalidCharacter))
                    input = input.Replace(invalidCharacter, ' ');
            return input;
        }
        bool SaveNameIsValid(string saveName)
        {
            if (string.IsNullOrEmpty(saveName))
            {
                Debug.LogError("Save name is empty");
                return false;
            }
            return true;
        }

        void TriggerSave(string saveName)
        {
            var path = GetNewSavePath();
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () =>
            {
                
                var result = await _globalSaveLoader.SaveGameAsync();
                if (result)
                {
                    Debug.Log($"Save successful: {path}");   
                }
            }));
        }

        string GetNewSavePath() =>Path.Combine(Application.persistentDataPath,_inputField.text);

        public void Dispose() => _disposable?.Dispose();
    }
}
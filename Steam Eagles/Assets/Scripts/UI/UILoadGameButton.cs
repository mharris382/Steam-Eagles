using System.IO;
using CoreLib;
using CoreLib.GameTime;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// displays a button that when pressed loads game into a specific save file. Should be populated in the LoadMenuPage
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UILoadGameButton : MonoBehaviour
    {
        public Button _button;
        public Button loadButton => _button != null ? _button : _button =GetComponent<Button>();
        private TextMeshProUGUI _text;
        public TextMeshProUGUI Text => _text != null ? _text : _text = GetComponentInChildren<TextMeshProUGUI>();
        
        public UIDisplayGameTime timeDisplay;
        public UIDisplayGameDate dateDisplay;
        
        public string savePath;

        public string GetFullSavePath()
        {
            return !savePath.StartsWith(Application.persistentDataPath) ?  $"{Application.persistentDataPath}/{savePath}" : savePath;
        }

        protected virtual void Awake()
        {
            var fullPath = GetFullSavePath();
            if (Directory.Exists(fullPath))
            {
                loadButton.interactable = true;
                loadButton.onClick.AsObservable().Subscribe(_ => MessageBroker.Default.Publish(new LoadGameRequestedInfo(GetFullSavePath()))).AddTo(this);
            }
            else
            {
                loadButton.interactable = false;
            }
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
using System;
using System.IO;
using UnityEngine;

namespace UI.MainMenu
{
    /// <summary>
    /// displays possible options to load game from all existing 
    /// </summary>
    [System.Serializable]
    public class UILoadMenu : MainMenuWindow, IDisposable
    {
        private readonly RectTransform _loadButtonLayoutGroup;
        private readonly UILoadGameButton _loadButtonPrefab;

        public UILoadMenu(RectTransform loadButtonLayoutGroup, UILoadGameButton loadButtonPrefab)
        {
            this._loadButtonLayoutGroup = loadButtonLayoutGroup;
            _loadButtonPrefab = loadButtonPrefab;
            var savePathRoot = Application.persistentDataPath;
            var directories = Directory.GetDirectories(savePathRoot);
            foreach (var directory in directories)
            {
                Debug.Log($"{directory}");
            }
        }

        public void Dispose()
        {
            int childCount = _loadButtonLayoutGroup.childCount;
            for (int i = 0; i < childCount; i++)
            {
                GameObject.Destroy(_loadButtonLayoutGroup.GetChild(0));
            }
        }

        public override void Init(UIMainMenu mainMenu)
        {
        
        }
    }
}
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.MainMenu
{
    [Serializable]
    public abstract class MainMenuWindow
    {
        [Required][ChildGameObjectsOnly]public GameObject window;
        public abstract void Init(UIMainMenu mainMenu);
           
        public void Show()
        {
            window.gameObject.SetActive(true);
        }
           
        public void Hide()
        {
            window.gameObject.SetActive(false);
        }
    }
}
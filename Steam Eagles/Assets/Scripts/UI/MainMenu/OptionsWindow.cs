using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.MainMenu
{
    [Serializable]
    public class OptionsWindow : MainMenuWindow
    {
        [Required][ChildGameObjectsOnly]public GameObject volume;
        public override void Init(UIMainMenu mainMenu)
        {
               
        }
    }
}
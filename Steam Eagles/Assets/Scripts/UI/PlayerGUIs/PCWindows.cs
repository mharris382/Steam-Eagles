using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.PlayerGUIs
{
    public class PCWindows : MonoBehaviour , IPCGUIController
    {
        private  PlayerCharacterGUIController _playerCharacterGUIController;
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject loadingScreen; 
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject savingScreen;  
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject characterWind; 
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject characterHUD;  
        public GameObject LoadingScreen => loadingScreen;
        public GameObject SavingScreen => savingScreen;
        public GameObject CharacterWindow => characterWind;
        public GameObject CharacterHUD => characterHUD;
        [UsedImplicitly]
        public GameObject CharacterGameObject { get; private set; }

        [UsedImplicitly]
        public PlayerInput Input { get; private set; }
        public void SetCharacter(PlayerInput input, GameObject characterGo)
        {
            CharacterGameObject = characterGo;
            Input = input;
            
        }


       
    }
}
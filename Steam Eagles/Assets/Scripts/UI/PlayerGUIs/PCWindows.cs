using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.PlayerGUIs
{
    public class PCWindows : MonoBehaviour
    {

        [SerializeField, Required, ChildGameObjectsOnly]private GameObject loadingScreen; 
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject savingScreen;  
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject characterWind; 
        [SerializeField, Required, ChildGameObjectsOnly]private GameObject characterHUD;  
        public GameObject LoadingScreen => loadingScreen;
        public GameObject SavingScreen => savingScreen;
        public GameObject CharacterWindow => characterWind;
        public GameObject CharacterHUD => characterHUD;

    }
}
using System;
using CoreLib.GameTime;
using UnityEngine;

namespace UI
{
    public class UIGameClock : MonoBehaviour
    {
        [SerializeField] private UIDisplayGameTime timeDisplay;
        
        private void Awake()
        {
            TimeManager.Instance.GameTimeIncremented += OnGameTimeChanged;
        }

        private void OnEnable()
        {
            OnGameTimeChanged(TimeManager.Instance.CurrentGameTime);
        }

        private void OnDestroy()
        {
            TimeManager.Instance.GameTimeIncremented -= OnGameTimeChanged;
        }


        void OnGameTimeChanged(GameTime gameTime)
        {
            timeDisplay.Display(gameTime);  
        }
        
    }
}
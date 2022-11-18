using System;
using CoreLib;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class UIPauseMenu : MonoBehaviour
    {
        public SharedBool isPaused;
        [Header("Settings")]
        [SerializeField] private bool pauseTime = true;


        private void Awake()
        {
            isPaused.onValueChanged.AsObservable().Where(x => x).Subscribe(_ => Pause()).AddTo(this);
            isPaused.onValueChanged.AsObservable().Where(x => !x).Subscribe(_ => Resume()).AddTo(this);
        }

        private void Pause()
        {
            if(pauseTime)
                Time.timeScale = 0;
        }
        
        private void Resume()
        {
            if(pauseTime)
                Time.timeScale = 1;
        }

        public void TogglePause()
        {
            isPaused.Value = !isPaused.Value;
            if (pauseTime)
            {
                Time.timeScale = isPaused.Value ? 0 : 1;
            }
        }
        
        public void PauseGame()
        {
            isPaused .Value = true;
        }
        
        public void ResumeGame()
        {
            isPaused.Value = false;
        }
        
        
        
        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    
        public void QuitToMainMenu()
        {
            SceneManager.LoadScene(0);
        }
        
        public void QuitButton()
        {
            Application.Quit();   
        }
        
        
    }
    
    public enum UIPauseMenuState
    {
        None,
        Pause,
        Settings
    }
}
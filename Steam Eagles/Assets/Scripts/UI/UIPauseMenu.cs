using System;
using CoreLib;
using StateMachine;
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

        public SharedTransform tpTransform;
        public SharedTransform tpSpawnTransform;
        public SharedTransform bdTransform;
        public SharedTransform bdSpawnTransform;

        private void Awake()
        {
            isPaused.Value = false;
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


        public void RespawnTP()
        {
            if(!tpTransform.HasValue || !tpSpawnTransform.HasValue)
                return;
            tpTransform.Value.position = tpSpawnTransform.Value.position;
        }

        public void RespawnBD()
        {
            if (bdTransform.HasValue == false || bdSpawnTransform.HasValue == false)
                return;
            bdTransform.Value.position = bdSpawnTransform.Value.position;
        }
        
        public void RespawnBoth()
        {
            RespawnBD();
            RespawnTP();
        }
        public void RestartScene()
        {
            RespawnBoth();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
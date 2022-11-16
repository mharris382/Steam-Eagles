using CoreLib;
using UnityEngine;

namespace UI
{
    public class UIPauseMenu : MonoBehaviour
    {
        public SharedBool isPaused;
        [Header("Settings")]
        [SerializeField] private bool pauseTime = true;
    
    
        
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
            if(pauseTime)
                Time.timeScale = 0;
        }
        public void ResumeGame()
        {
            isPaused.Value = false;
            if(pauseTime)
                Time.timeScale = 1;
        }
    
    
        public void QuitToMainMenu()
        {
            Application.Quit();   
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
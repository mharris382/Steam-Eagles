using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    public int startSceneSinglePlayer = 2;
    public int startSceneMultiplayer = 1;
    public void StartNewGame(bool singlePlayer)
    {
        
        SceneManager.LoadScene(singlePlayer ? startSceneSinglePlayer : startSceneMultiplayer);
    }
    

    public void QuitButton()
    {
        Application.Quit();   
    }
}
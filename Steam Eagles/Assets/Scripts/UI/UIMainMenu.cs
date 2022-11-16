using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIMainMenu : MonoBehaviour
{
    public int startScene = 1;
    public void StartNewGame()
    {
        SceneManager.LoadScene(startScene);
    }
    

    public void QuitButton()
    {
        Application.Quit();   
    }
}
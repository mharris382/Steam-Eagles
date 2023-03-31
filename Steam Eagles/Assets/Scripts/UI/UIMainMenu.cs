using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public int startSceneSinglePlayer = 2;
    public int startSceneMultiplayer = 1;

    
    public RectTransform loadMenuButtonRoot;
    public UILoadGameButton loadMenuButtonPrefab;

    private UILoadMenu _loadMenu;
    
    public void StartNewGame(bool singlePlayer)
    {
        
        SceneManager.LoadScene(singlePlayer ? startSceneSinglePlayer : startSceneMultiplayer);
    }

    public void PopulateLoadMenu()
    {
        if(_loadMenu != null)
            CloseLoadMenu();
        _loadMenu = new UILoadMenu(loadMenuButtonRoot, loadMenuButtonPrefab);
    }

    public void CloseLoadMenu()
    {
        if (_loadMenu != null)
        {
            _loadMenu.Dispose();
            _loadMenu = null;
        }
    }

    public void QuitButton()
    {
        Application.Quit();   
    }

    private void Awake()
    {
        
    }
}


/// <summary>
/// displays possible options to load game from all existing 
/// </summary>
public class UILoadMenu : IDisposable
{
    private readonly RectTransform _loadButtonLayoutGroup;
    private readonly UILoadGameButton _loadButtonPrefab;

    public UILoadMenu(RectTransform loadButtonLayoutGroup, UILoadGameButton loadButtonPrefab)
    {
        this._loadButtonLayoutGroup = loadButtonLayoutGroup;
        _loadButtonPrefab = loadButtonPrefab;
        var savePathRoot = Application.persistentDataPath;
        var directories = Directory.GetDirectories(savePathRoot);
        foreach (var directory in directories)
        {
            Debug.Log($"{directory}");
        }
    }

    public void Dispose()
    {
        int childCount = _loadButtonLayoutGroup.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.Destroy(_loadButtonLayoutGroup.GetChild(0));
        }
    }
}
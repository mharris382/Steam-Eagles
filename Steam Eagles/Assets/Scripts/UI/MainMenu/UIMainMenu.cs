using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using Game;
using SaveLoad;
using Sirenix.OdinInspector;
using UI;
using UI.MainMenu;
using UniRx;
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

    public Windows windows;

    [Serializable]
    public class Windows
    {
       [ChildGameObjectsOnly] public GameObject newGameWindow;
       [ChildGameObjectsOnly] public GameObject loadGameWindow;
       
       public OptionsWindow optionsWindow;
       public CharacterSelectWindow characterSelectWindow;

       [Required][ChildGameObjectsOnly]public Button backButton;
       
       
       
       public void Init(UIMainMenu uiMainMenu)
       {
           characterSelectWindow.Init(uiMainMenu);
            optionsWindow.Init(uiMainMenu);
       }
       IEnumerable<GameObject> AllWindows()
       {
            yield return newGameWindow;
            yield return loadGameWindow;
            yield return optionsWindow.window;
            yield return characterSelectWindow.window;
       }
      
       public void ShowNewGameWindow()
       {
           foreach (var window in AllWindows())
           {
                if(window == null)
                    continue;
                window.SetActive(window == newGameWindow);
           }
       }
       
       public void ShowLoadGameWindow()
       {
           foreach (var window in AllWindows())
           {
               if(window == null)
                   continue;
               window.SetActive(window == loadGameWindow);
           }
       }
       public void ShowCharacterSelectWindow()
       {
           foreach (var window in AllWindows())
           {
               if(window == null)
                   continue;
               window.SetActive(window == characterSelectWindow.window);
           }
       }
       public void ShowOptionsWindow()
       {
           foreach (var window in AllWindows())
           {
               if(window == null)
                   continue;
               window.SetActive(window == optionsWindow.window);
           }
       }
       public void BackToMainMenuHome()
       {
           foreach (var window in AllWindows())
           {
               if(window == null)
                   continue;
               window.SetActive(false);
           }
       }
    }

    private void Awake()
    {
        windows.Init(this);
        
    }
    public void ResetNewGameOperations()
    {
        UnbindPlayerCharacter(0);
        UnbindPlayerCharacter(1);
    }
    void UnbindPlayerCharacter(int player)
    {
        if (GameManager.Instance.PlayerHasCharacterAssigned(player))
        {
            MessageBroker.Default.Publish(new PlayerCharacterUnboundInfo() { playerNumber = player });
        }
    }
    public void ShowCharacterSelect()
    {
        UnbindPlayerCharacter(0);
        UnbindPlayerCharacter(1);
        windows.ShowCharacterSelectWindow();
        int playerDevices = GameManager.Instance.GetNumberOfPlayerDevices();
        windows.characterSelectWindow.UpdateSelectionWindow(playerDevices);
    }

    public void StartGame()
    {
        if (GameManager.Instance.CanStartGameInSingleplayer())
        {
            MessageBroker.Default.Publish(new LoadGameRequestedInfo(PersistenceManager.SavePath));
            return;
        }
        throw new NotImplementedException();
    }

    public void StartNewGame(bool singlePlayer)
    {
        if ((singlePlayer && GameManager.Instance.CanStartGameInSingleplayer()) || 
            (!singlePlayer && GameManager.Instance.CanStartGameInMultiplayer()))
        {
            PlayerPrefs.SetString("Last Save Path", PersistenceManager.SavePath);
            MessageBroker.Default.Publish(new LoadGameRequestedInfo(PersistenceManager.SavePath));
        }
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

    public void SetDefaultModeToSingleplayer()
    {
        //TODO: implement singleplayer mode
        windows.ShowCharacterSelectWindow();
    }

    public void SetDefaultModeToMultiplayer()
    {
        //TODO: implement singleplayer mode
        windows.ShowOptionsWindow();
    }

    public void QuitButton()
    {
        Application.Quit();   
    }
    
    public void OnContinueButton()
    {
        var lastPath = PlayerPrefs.GetString("Last Save Path");
        if (lastPath != null)
        {
            MessageBroker.Default.Publish(new LoadGameRequestedInfo(lastPath));
        }
    }
}
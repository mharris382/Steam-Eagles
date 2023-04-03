using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UI;
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
       
       [Serializable]
       public class CharacterSelectWindow : MainMenuWindow
       {
           
           [Required][ChildGameObjectsOnly] public RectTransform undecidedPlayerRoot;
           [Required][ChildGameObjectsOnly] public RectTransform transporterSelectRoot;
           [Required][ChildGameObjectsOnly] public RectTransform builderSelectRoot;
           [Required][ChildGameObjectsOnly]public GameObject[] playerDeviceIcons;
           [Required][ChildGameObjectsOnly]public Button confirmSelectionButton;


           public override void Init(UIMainMenu mainMenu)
           {
               window.gameObject.SetActive(true);
               confirmSelectionButton.onClick.AsObservable().Subscribe(_ =>
               {
                   mainMenu.StartGame();
               });
           }

           public void ShowForPlayers(int numberOfPlayers)
           {
               window.gameObject.SetActive(true);
               foreach (var playerDeviceIcon in playerDeviceIcons)
               {
                   playerDeviceIcon.transform.SetParent(undecidedPlayerRoot);
               }
               for(int i = 0; i < playerDeviceIcons.Length; i++)
               {
                   playerDeviceIcons[i].SetActive(i < numberOfPlayers);
               }
           }

           public void PlayerSelectedTransporter(int playerIndex)
           {
               playerDeviceIcons[playerIndex].transform.SetParent(transporterSelectRoot);
           }

           public void PlayerSelectedBuilder(int playerIndex)
           {
               playerDeviceIcons[playerIndex].transform.SetParent(builderSelectRoot);
           }

           public void PlayerSelectedUndecided(int playerIndex)
            {
                playerDeviceIcons[playerIndex].transform.SetParent(undecidedPlayerRoot);
            }

           public bool AreAllPlayersDecided(int playerCount)
            {
                for (int i = 0; i < playerCount; i++)
                {
                    if (playerDeviceIcons[i].transform.parent == undecidedPlayerRoot)
                        return false;
                }
                return true;
            }


            public void UpdateSelectionWindow(int playerCount)
            {
                
                confirmSelectionButton.interactable = AreAllPlayersDecided(playerCount);
            }
       }
       
       
       [Serializable]
       public class OptionsWindow : MainMenuWindow
       {
           [Required][ChildGameObjectsOnly]public GameObject volume;
           public override void Init(UIMainMenu mainMenu)
           {
               
           }
       }
       [Serializable]
       public abstract class MainMenuWindow
       {
           [Required][ChildGameObjectsOnly]public GameObject window;
           public abstract void Init(UIMainMenu mainMenu);
           
           public void Show()
           {
               window.gameObject.SetActive(true);
           }
           
              public void Hide()
              {
                window.gameObject.SetActive(false);
              }
       }
       
       public void Init(UIMainMenu uiMainMenu)
       {
           characterSelectWindow.Init(uiMainMenu);
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

    private void StartGame()
    {
        throw new NotImplementedException();
    }

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
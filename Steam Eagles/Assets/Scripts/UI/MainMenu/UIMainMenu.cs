using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CoreLib;
using Cysharp.Threading.Tasks;
using Game;
using SaveLoad;
using Sirenix.OdinInspector;
using UI;
using UI.MainMenu;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;
using System.Linq;
public class UIMainMenu : MonoBehaviour
{
    public int startSceneSinglePlayer = 2;
    public int startSceneMultiplayer = 1;

    
    public RectTransform loadMenuButtonRoot;
    public UILoadGameButton loadMenuButtonPrefab;

    private UILoadMenu _loadMenu;

    public Windows windows;

    [SerializeField] private UIPanelGroup panelGroup = new UIPanelGroup() {
        panels = new List<UIPanel>()
        {
            new UIPanel()
            {
                key = "options",
                panelWindow = null,
                panelGameObject = null,
                uiMenuButton = null,
            },
            new UIPanel()
            {
                key = "load game",
                panelWindow = null,
                panelGameObject = null,
                uiMenuButton = null,
            },
            new UIPanel()
            {
                key = "new game",
                panelWindow = null,
                panelGameObject = null,
                uiMenuButton = null,
            },
            new UIPanel()
            {
                key = "credits",
                panelWindow = null,
                panelGameObject = null,
                uiMenuButton = null,
            },
        }
    };

    [ShowInInspector, ReadOnly]
    private GlobalSaveLoader _saveLoader;

    private CoroutineCaller _coroutineCaller;
    private GlobalSavePath _savePath;

    [Serializable]
    public class UIPanelGroup
    {
        public List<UIPanel> panels = new List<UIPanel>();
        internal UIMainMenu _uiMainMenu;
        public void Init(UIMainMenu mainMenu)
        {
            _uiMainMenu = mainMenu;
            foreach (var uiPanel in panels)
            {
                uiPanel.SetGroup(this);
                uiPanel.panelGameObject.SetActive(false);
            }
        }

        public void ShowWindow(string newGame)
        {
            foreach (var panel in panels)
            {
                if (String.Equals(panel.key, newGame, StringComparison.CurrentCultureIgnoreCase))
                {
                    panel.Show();
                    return;
                }
            }
        }
    }

    [Serializable]
    public class UIPanel
    {
        public string key;
        public Window panelWindow;
        public GameObject panelGameObject;
        public Button uiMenuButton;
        
        UIPanelGroup group;
        public void SetGroup(UIPanelGroup group)
        {
            this.group = group;
            panelWindow.IsVisibleProperty
                .Subscribe(open =>
                {
                    if (open)
                    {
                        group.panels.ForEach(panel =>
                        {
                            if (panel != this)
                                panel.Hide();
                        });
                    }
                }).AddTo(group._uiMainMenu);
            uiMenuButton.OnClickAsObservable().Subscribe(_ => Show()).AddTo(group._uiMainMenu);
            if (panelWindow.closeButton != null)
                panelWindow.closeButton.OnClickAsObservable().Subscribe(_ => HideAndSelectButton())
                    .AddTo(group._uiMainMenu);
        }
        public void Show()
        {
            panelWindow.IsVisible = true;
            panelGameObject.gameObject.SetActive(true);
            foreach (var groupPanel in group.panels)
            {
                if(groupPanel == this)
                    continue;
                groupPanel.Hide();
            }
        }

        public void Hide()
        {
            panelWindow.IsVisible = false;
            panelGameObject.gameObject.SetActive(false);
        }   
        
        public void HideAndSelectButton()
        {
            Hide();
            EventSystem.current.SetSelectedGameObject(uiMenuButton.gameObject);
        }
    }

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
           optionsWindow.Show();
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

    
    public UIPanelGroup GetPanelGroup()
    {
        return panelGroup;
    }

    [Inject]
    public void InjectSaveLoader(GlobalSaveLoader saveLoader, CoroutineCaller coroutineCaller, GlobalSavePath savePath)
    {
        Debug.Log("Injecting save loader");
        _saveLoader = saveLoader;
        _savePath = savePath;
        _coroutineCaller = coroutineCaller;
    }
    
    private void Awake()
    {
        windows.Init(this);
        panelGroup.Init(this);

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
            ///MessageBroker.Default.Publish(new LoadGameRequestedInfo(PersistenceManager.SavePath));
            return;
        }
        throw new NotImplementedException();
    }

    public void StartNewGame(bool singlePlayer)
    {
        if ((singlePlayer && GameManager.Instance.CanStartGameInSingleplayer()) || 
            (!singlePlayer && GameManager.Instance.CanStartGameInMultiplayer()))
        {
            // PlayerPrefs.SetString("Last Save Path", PersistenceManager.SavePath);
            // MessageBroker.Default.Publish(new LoadGameRequestedInfo(PersistenceManager.SavePath));
        }
    }
    
    public void ShowOptionsWindow()
    {
        panelGroup.ShowWindow("options");
        windows.ShowOptionsWindow();
    }
    
    public void ShowLoadGameWindow()
    {
        windows.ShowLoadGameWindow();
        panelGroup.ShowWindow("load game");
    }
    
    public void ShowNewGameWindow()
    {
        windows.ShowNewGameWindow();
        panelGroup.ShowWindow("new game");
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
    public string GetFullSavePath()
    {
        return !_savePath.FullSaveDirectoryPath.StartsWith(Application.persistentDataPath)
            ? Path.Combine(Application.persistentDataPath, _savePath.FullSaveDirectoryPath)
            : _savePath.FullSaveDirectoryPath;
    }
    public void OnContinueButton()
    {
        if (_saveLoader != null)
        {
            Debug.Log("Starting load game");
            _coroutineCaller.StartCoroutine(UniTask.ToCoroutine(async () => {
                var result = await _saveLoader.LoadGameAsync();
                if (result) Debug.Log($"Load game successful: {_savePath.FullSaveDirectoryPath}",this);
                else Debug.LogError($"Load game failed: {_savePath.FullSaveDirectoryPath}",this);
            }));
        }
        else
        {
            // throw new NotImplementedException();
            // var lastPath = PlayerPrefs.GetString("Last Save Path");
            // if (lastPath != null)
            // {
            //     PersistenceManager.Instance.LoadGameRequest(new LoadGameRequestedInfo(lastPath));
            // }
            // Debug.LogError("Save loader not injected",this);
        }
        
    }
}
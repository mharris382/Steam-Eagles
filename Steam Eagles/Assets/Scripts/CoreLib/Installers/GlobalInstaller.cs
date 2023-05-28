
using System;
using System.Collections;
using System.Collections.Generic;
using CoreLib;
using CoreLib.GameTime;
using CoreLib.SharedVariables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class TimeInstaller : Installer<TimeInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<IGameTimeConfig>().To<GameTimeConfig>().FromScriptableObjectResource("GameTimeConfig/GameTimeConfig").AsSingle();
        Container.Bind<GameTimeState>().FromNew().AsSingle();
        Container.BindInterfacesAndSelfTo<TimeRunner>().AsSingle().NonLazy();
    }
}


public class GlobalInstaller : MonoInstaller
{
    
    [Required, AssetsOnly] public GameObject pauseMenuPrefab;
    [Required, AssetsOnly] public GameObject playerGUIPrefab;
    public SharedTransform p1Character;
    public SharedTransform p2Character;
    public SlowTickConfig slowTickConfig;


    [ShowInInspector]
    [TitleGroup("In Game Time Settings"), InlineEditor(Expanded = true)]
    public GameTimeConfig timeConfig
    {
        get => GameTimeConfig.Instance;
        set
        {
            
        }
    }
    
    
    
    public override void InstallBindings()
    {
        Container.Bind<SlowTickConfig>().FromInstance(slowTickConfig).AsSingle();
        Container.BindInterfacesAndSelfTo<SlowTickUpdater>().AsSingle().NonLazy();
        Container.BindInterfacesTo<TestSlowTickables>().AsSingle().NonLazy();

        Container.Bind<Canvas>().FromComponentInNewPrefab(pauseMenuPrefab).AsSingle().NonLazy();
        Container.Bind<CoroutineCaller>().FromNewComponentOnNewGameObject().WithGameObjectName("CoroutineCaller").AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PauseMenuSetInactiveOnStart>().FromNew().AsSingle().NonLazy();
        Container.Bind<List<SharedTransform>>().FromInstance(new List<SharedTransform>(new []{p1Character, p2Character})).AsSingle().NonLazy();
        Container.Bind<GlobalGameState>().FromNew().AsSingle();

        TimeInstaller.Install(Container);

    }
    
    private class TestSlowTickables : ISlowTickable, IExtraSlowTickable
    {
        private readonly SlowTickConfig _config;

        public TestSlowTickables(SlowTickConfig config)
        {
            _config = config;
        }
        public void SlowTick(float deltaTime)
        {
            if (_config.logLevel == SlowTickConfig.LogLevel.VERBOSE_WITH_TESTS)
            {
                _config.Log($"SlowTick: {deltaTime}");
            }
            else
            {
                _config.Log("SlowTicking");
            }
        }

        public void ExtraSlowTick(float deltaTime)
        {
            if (_config.logLevel == SlowTickConfig.LogLevel.VERBOSE_WITH_TESTS)
            {
                _config.Log($"ExtraSlowTick: {deltaTime}");
            }
            else
            {
                _config.Log("ExtraSlowTicking");
            }
        }
    }
    
    public class GlobalGameState
    {
        public bool inMainMenu
        {
            get;
            set;
        }

        public event Action<int> onSceneLoad;

        public GlobalGameState()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            inMainMenu = true;
        }

        private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            inMainMenu = arg0.name == "Main Menu";
            onSceneLoad(arg0.buildIndex);
        }
    }
    
    internal class PauseMenuSetInactiveOnStart : IInitializable
    {
        private Canvas _pauseMenuCanvas;
        private readonly CoroutineCaller _coroutineCaller;


        public PauseMenuSetInactiveOnStart(Canvas pauseMenuCanvas, CoroutineCaller coroutineCaller)
        {
            Debug.Log("Creating Pause Menu Set Inactive On Start");
            _pauseMenuCanvas = pauseMenuCanvas;
            _coroutineCaller = coroutineCaller;
        }
        public void Initialize()
        {
            _coroutineCaller.StartCoroutine(WaitFrame());
        }

        IEnumerator WaitFrame()
        {
            while(_pauseMenuCanvas == null)
            {
                Debug.Log("Waiting For Pause Menu Canvas");
                yield return null;
            }
            //_pauseMenuCanvas.gameObject.SetActive(false);
        }
    }
}
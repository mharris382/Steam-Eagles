
using System.Collections;
using CoreLib;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    [Required, AssetsOnly] public GameObject pauseMenuPrefab;
    [Required, AssetsOnly] public GameObject playerGUIPrefab;
    
    
    public override void InstallBindings()
    {
        Container.Bind<Canvas>().FromComponentInNewPrefab(pauseMenuPrefab).AsSingle().NonLazy();
        Container.Bind<CoroutineCaller>().FromNewComponentOnNewGameObject().WithGameObjectName("CoroutineCaller").AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PauseMenuSetInactiveOnStart>().FromNew().AsSingle().NonLazy();
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
            _pauseMenuCanvas.gameObject.SetActive(false);
        }
    }
}
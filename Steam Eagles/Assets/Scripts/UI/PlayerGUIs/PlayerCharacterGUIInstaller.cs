using System;
using System.Collections;
using CoreLib.Signals;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.PlayerGUIs
{
    [RequireComponent(typeof(PlayerCharacterGUIController))]
    public class PlayerCharacterGUIInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<GUIRunner>().AsSingle().NonLazy();
            Container.Bind<PlayerCharacterGUIController>().FromComponentOn(gameObject).AsSingle().NonLazy();
        }
    }


    public class GUIRunner : IInitializable, IDisposable
    {
        private readonly PlayerCharacterGUIController _controller;
        private readonly CoroutineCaller _coroutineCaller;
        private Coroutine _coroutine;
        private CompositeDisposable _cd= new();
        public GUIRunner(PlayerCharacterGUIController controller, CoroutineCaller coroutineCaller)
        {
            _controller = controller;
            _coroutineCaller = coroutineCaller;
        }

        public void Initialize()
        {
            _coroutine = _coroutineCaller.StartCoroutine(RunGUI());
        }

        IEnumerator RunGUI()
        {
            yield return UniTask.WaitUntil(() => _controller.HasAllResources());
            while (true)
            {
                yield return null;
            }
        }
        public void Dispose()
        {
            if (_coroutineCaller != null&& _coroutine != null)
            {
                _coroutineCaller.StopCoroutine(_coroutine);
            }
            _cd.Dispose();
        }
    }
    //public class UIPrefabFactory : Placeholder
}
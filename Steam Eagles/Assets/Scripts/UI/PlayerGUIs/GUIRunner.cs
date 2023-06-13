using System;
using System.Collections;
using CoreLib;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.PlayerGUIs
{
    internal class GUIRunner : IInitializable, IDisposable
    {
        private readonly PlayerCharacterGUIController _controller;
        private readonly CoroutineCaller _coroutineCaller;
        private readonly PersistenceState _persistenceState;
        private Coroutine _coroutine;
        private CompositeDisposable _cd= new();
        private readonly GameState _gameState;

        public GUIRunner(PlayerCharacterGUIController controller, CoroutineCaller coroutineCaller, PersistenceState persistenceState, GameState gameState)
        {
            _gameState = gameState;
            _controller = controller;
            _coroutineCaller = coroutineCaller;
            _persistenceState = persistenceState;
        }

        public void Initialize()
        {
            _coroutine = _coroutineCaller.StartCoroutine(RunGUI());
        }

        IEnumerator RunGUI()
        {
            while (true)
            {
                yield return null;
                switch (_persistenceState.CurrentAction)
                {
                    case PersistenceState.Action.NONE:
                        if (_gameState.IsPaused)
                        {
                            _controller.GUIState = PCGUIState.PAUSE_MENU;
                            continue;
                        }
                        
                        switch (_controller.GUIState)
                        {
                            case PCGUIState.SAVING:
                            case PCGUIState.LOADING:
                            case PCGUIState.WAITING_FOR_CHARACTER:
                                _controller.GUIState = _controller.HasAllResources() ? PCGUIState.DEFAULT : PCGUIState.WAITING_FOR_CHARACTER;
                                break;
                            case PCGUIState.PAUSE_MENU:
                                _controller.GUIState = PCGUIState.DEFAULT;
                                break;
                            case PCGUIState.CHARACTER_MENU:
                            case PCGUIState.CUTSCENE:
                                
                            case PCGUIState.PILOTING:
                            case PCGUIState.DEFAULT:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case PersistenceState.Action.LOADING:
                        _controller.GUIState = PCGUIState.LOADING;
                        continue;
                    case PersistenceState.Action.SAVING:
                        _controller.GUIState = PCGUIState.SAVING;
                        continue;
                }

              
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
}
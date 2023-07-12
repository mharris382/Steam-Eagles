using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Tools.UI
{
    public class InputHelperInstaller : MonoInstaller
    {
        public PCUI pcUI;

        public override void InstallBindings()
        {
            Container.Bind<PCUI>().FromInstance(pcUI).AsSingle().NonLazy();
            Container.Bind<ToolInputHelper>().AsSingle().NonLazy();
        }
       

    }
    public class ToolInputHelper
    {
        private readonly PCUI _controller;

        private PlayerInput _playerInput;
        public PlayerInput PlayerInput => _playerInput ??= _controller.playerInput;
        public bool HasInput() => _controller != null && PlayerInput != null;

        public bool IsUsingKeyboardMouse() => HasInput() && PlayerInput.currentControlScheme.Contains("Keyboard");

        
        public ToolInputHelper(PCUI controller)
        {
            this._controller = (PCUI) controller;
            Debug.Assert(_controller != null);
           
        }
    }
}
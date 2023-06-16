using System.Collections;
using Tools.BuildTool;
using UI.PlayerGUIs;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Tools.UI
{
    public abstract class HUDToolControllerBase : MonoBehaviour
    {
        protected PlayerCharacterGUIController _guiController;
        protected ToolControllerSharedData _toolControllerSharedData;
        
        public PlayerCharacterGUIController GUIController
        {
            get
            {
                if (_guiController == null)
                {
                    _guiController = GetComponentInParent<PlayerCharacterGUIController>();
                }
                return _guiController;
            }
        }

        public ToolControllerSharedData SharedToolData => _toolControllerSharedData;
        
        private IEnumerator Start()
        {
            while (!GUIController.HasAllResources())
            {
                Debug.Log("HUD Tool Controller waiting for GUI Controller to load resources...", this);
                yield return null;
            }

            _toolControllerSharedData = GUIController.PlayerCharacter.GetComponentInChildren<ToolControllerSharedData>();
            var toolStream = _toolControllerSharedData.activeTool.StartWith(_toolControllerSharedData.ActiveToolValue).ToReadOnlyReactiveProperty();
            var emptyToolStream = Observable.Return<ToolControllerBase>(null).ToReadOnlyReactiveProperty();
            var toolsHiddenStream = _toolControllerSharedData.ToolsHiddenStream;
            var toolStream2 = toolsHiddenStream.Select(t => t ? emptyToolStream.StartWith(emptyToolStream.Value) : toolStream.StartWith(toolStream.Value)).Switch().ToReadOnlyReactiveProperty();
            toolStream2.Where(t => t == null).Subscribe(_ => HideToolHUD()).AddTo(this);
            toolStream2.Where(t => t != null).Subscribe(ShowToolHUD).AddTo(this);
           // Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
           // Debug.Assert((bool)(GUIController.PlayerCharacter != null), (string)"GUI Controller is missing Player Character", (Object)GUIController);
           // Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            
            
            OnFullyInitialized();
        }
        
        public abstract void OnFullyInitialized();

        public abstract void HideToolHUD();
        public abstract void ShowToolHUD(ToolControllerBase controllerBase);
    }
}
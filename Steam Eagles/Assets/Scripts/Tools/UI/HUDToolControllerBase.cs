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
        private CharacterTools _characterTools;
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
        public CharacterTools CharacterTools => _characterTools;
        private IEnumerator Start()
        {
            while (!GUIController.HasAllResources())
            {
                Debug.Log("HUD Tool Controller waiting for GUI Controller to load resources...", this);
                yield return null;
            }

            _toolControllerSharedData = GUIController.PlayerCharacter.GetComponentInChildren<ToolControllerSharedData>();
            _characterTools = GUIController.PlayerCharacter.GetComponentInChildren<CharacterTools>();
           // Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
           // Debug.Assert((bool)(GUIController.PlayerCharacter != null), (string)"GUI Controller is missing Player Character", (Object)GUIController);
           // Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            
            _characterTools.OnToolEquipped.Where(t => t == null).Subscribe(_ => HideToolHUD()).AddTo(this);
            _characterTools.OnToolEquipped.Where(_ => _ != null).Select(_ => _.GetComponent<ToolControllerBase>()).Subscribe(ShowToolHUD).AddTo(this);
            OnFullyInitialized();
        }
        
        public abstract void OnFullyInitialized();

        public abstract void HideToolHUD();
        public abstract void ShowToolHUD(ToolControllerBase controllerBase);
    }
}
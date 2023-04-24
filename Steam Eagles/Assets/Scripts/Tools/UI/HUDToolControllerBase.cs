using System.Collections;
using Tools.BuildTool;
using UI.PlayerGUIs;
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
            
            Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            Debug.Assert((bool)(GUIController.PlayerCharacter != null), (string)"GUI Controller is missing Player Character", (Object)GUIController);
            Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            
            OnFullyInitialized();
        }
        
        public abstract void OnFullyInitialized();
    }
}
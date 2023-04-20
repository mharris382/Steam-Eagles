using System;
using System.Collections;
using Tools.BuildTool;
using UI.PlayerGUIs;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Tools.UI
{
    public class HUDToolController : MonoBehaviour
    {
        private PlayerCharacterGUIController _guiController;
        private ToolControllerSharedData _toolControllerSharedData;
        public Image image;
        
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

        private IEnumerator Start()
        {
            while (!GUIController.HasAllResources())
            {
                Debug.Log("HUD Tool Controller waiting for GUI Controller to load resources...", this);
                yield return null;
            }

            _toolControllerSharedData =
                GUIController.PlayerCharacter.GetComponentInChildren<ToolControllerSharedData>();
            Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            Debug.Assert(GUIController.PlayerCharacter != null, "GUI Controller is missing Player Character", GUIController);
            Debug.Assert(_toolControllerSharedData != null, $"Missing Tool Controller Shared Data on {GUIController.PlayerCharacter.name}", this);
            _toolControllerSharedData.ActiveTool.StartWith(_toolControllerSharedData.ActiveToolValue)
                .Subscribe(t =>
                {
                    var toolItemSprite = t.ToolIcon?.GetIcon();
                    if (toolItemSprite != null)
                    {
                        image.enabled = true;
                        image.sprite = toolItemSprite;
                    }
                    else
                    {
                        image.enabled = false;
                    }
                }).AddTo(this);
        }
    }
}
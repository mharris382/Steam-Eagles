using System;
using System.Collections.Generic;
using UI.PlayerGUIs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SelectUnderMouse : MonoBehaviour
    {
        public EventSystem eventSystem;
        public GraphicRaycaster raycaster;
        public UIElement tooltipWindow;
        private PlayerCharacterGUIController _guiController;
        

        private void Awake()
        {
            _guiController = GetComponentInParent<PlayerCharacterGUIController>();
        }

        bool HasResources()
        {
            if (_guiController == null)
            {
                _guiController = GetComponentInParent<PlayerCharacterGUIController>();
            }

            if (raycaster == null)
            {
                raycaster = GetComponentInParent<GraphicRaycaster>();
            }

            if (_guiController.playerInput == null)
            {
                return false;
            }

            if (eventSystem == null)
            {
                eventSystem = _guiController.playerInput.uiInputModule.GetComponent<EventSystem>();   
            }

            return _guiController != null && raycaster != null && eventSystem != null;
        }

        private void Update()
        {
            if (!HasResources())
            {
                return;
            }
            var mousePos = Input.mousePosition;
            var pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = mousePos;
            var results = new List<RaycastResult>();
            raycaster.Raycast(pointerEventData, results);
            bool hit = false;
            foreach (var result in results)
            {
                if (result.gameObject.GetComponent<Selectable>() != null)
                {
                    result.gameObject.GetComponent<Selectable>().Select();
                    hit = true;
                    break;
                }
            }

            if (hit == false)
            {
                tooltipWindow.IsVisible = false;   
            }
            else
            {
                tooltipWindow.IsVisible = true;
            }
        }
    }
}
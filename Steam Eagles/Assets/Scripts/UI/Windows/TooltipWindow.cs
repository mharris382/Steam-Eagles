using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class TooltipWindow : UIElement
    {
        public EventSystem eventSystem;
        private RectTransform _rect;
        public bool followCursor;

        public Vector2 preferredOffsetDirection = Vector2.right;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            if(eventSystem == null)
                eventSystem = FindObjectOfType<EventSystem>();
        }


        protected override bool BlocksRaycasts => false;
        protected override bool IsInteractable => false;

        protected virtual bool IsValidSelectableForTooltip(GameObject selectable)
        {
            return true;
        }

        protected virtual void UpdateContent(GameObject selected)
        {
            
        }

        public void Update()
        {
            var mousePos = Input.mousePosition;
            var selected = eventSystem.currentSelectedGameObject;
        
            if (selected == null)
            {
                return;
            }
            var selectedRect = selected.GetComponent<RectTransform>();
            if (selectedRect == null)
            {
                return;
            }
            if (!IsValidSelectableForTooltip(selected))
            {
                IsVisible = false;
                return;
            }
            IsVisible = true;
            UpdateContent(selected);
            if (!followCursor)
            {
                var selectedPos = selectedRect.position;
                var selectedSize = selectedRect.sizeDelta;
        
                // Calculate the position of the tooltip so that it is adjacent to the selected object but does not overlap it.
                var tooltipPos = selectedPos;
                tooltipPos.x += selectedSize.x * preferredOffsetDirection.x;
                tooltipPos.y += selectedSize.y * preferredOffsetDirection.y;
                // If the tooltip would be off the screen, move it to the other side of the selected object.
                if (tooltipPos.x + _rect.sizeDelta.x > Screen.width)
                {
                    tooltipPos.x = selectedPos.x - _rect.sizeDelta.x;
                }
                if (tooltipPos.y + _rect.sizeDelta.y > Screen.height)
                {
                    tooltipPos.y = selectedPos.y - _rect.sizeDelta.y;
                }
                // If the tooltip is still off the screen, move it to the center of the screen.
                if (tooltipPos.x < 0)
                {
                    tooltipPos.x = Screen.width / 2f;
                }
                if (tooltipPos.y < 0)
                {
                    tooltipPos.y = Screen.height / 2f;
                }
                // Set the position of the tooltip.
                _rect.position = tooltipPos;
            }
            else
            {
                _rect.position = mousePos;
                // if the cursor is not over the selected object, hide the tooltip.
                if (!RectTransformUtility.RectangleContainsScreenPoint(selectedRect, mousePos))
                {
                    IsVisible = false;
                }
                else
                {
                    IsVisible = true;
                    // If the tooltip would be off the screen, move it to the other side of the mouse.
                    if (mousePos.x + _rect.sizeDelta.x > Screen.width)
                    {
                        _rect.position = new Vector2(mousePos.x - _rect.sizeDelta.x, mousePos.y);
                    }
                    if (mousePos.y + _rect.sizeDelta.y > Screen.height)
                    {
                        _rect.position = new Vector2(mousePos.x, mousePos.y - _rect.sizeDelta.y);
                    }
                }
           
            }
        }
    }
}
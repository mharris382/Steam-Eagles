using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class TooltipWindow : UIElement
    {
        public EventSystem eventSystem;
        private RectTransform _rectTransform;
        public bool followCursor;

        public Vector2 preferredOffsetDirection = Vector2.right;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            if(eventSystem == null)
                eventSystem = FindObjectOfType<EventSystem>();
        }


        protected override bool BlocksRaycasts => false;
        protected override bool IsInteractable => false;

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
            if (!followCursor)
            {
                var selectedPos = selectedRect.position;
                var selectedSize = selectedRect.sizeDelta;
        
                // Calculate the position of the tooltip so that it is adjacent to the selected object but does not overlap it.
                var tooltipPos = selectedPos;
                tooltipPos.x += selectedSize.x * preferredOffsetDirection.x;
                tooltipPos.y += selectedSize.y * preferredOffsetDirection.y;
                // If the tooltip would be off the screen, move it to the other side of the selected object.
                if (tooltipPos.x + _rectTransform.sizeDelta.x > Screen.width)
                {
                    tooltipPos.x = selectedPos.x - _rectTransform.sizeDelta.x;
                }
                if (tooltipPos.y + _rectTransform.sizeDelta.y > Screen.height)
                {
                    tooltipPos.y = selectedPos.y - _rectTransform.sizeDelta.y;
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
                _rectTransform.position = tooltipPos;
            }
            else
            {
                _rectTransform.position = mousePos;
                // if the cursor is not over the selected object, hide the tooltip.
                if (!RectTransformUtility.RectangleContainsScreenPoint(selectedRect, mousePos))
                {
                    IsVisible = false;
                }
                else
                {
                    IsVisible = true;
                    // If the tooltip would be off the screen, move it to the other side of the mouse.
                    if (mousePos.x + _rectTransform.sizeDelta.x > Screen.width)
                    {
                        _rectTransform.position = new Vector2(mousePos.x - _rectTransform.sizeDelta.x, mousePos.y);
                    }
                    if (mousePos.y + _rectTransform.sizeDelta.y > Screen.height)
                    {
                        _rectTransform.position = new Vector2(mousePos.x, mousePos.y - _rectTransform.sizeDelta.y);
                    }
                }
           
            }
        
        }
    }
}
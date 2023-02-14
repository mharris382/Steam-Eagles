using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private bool _isVisible;
        public CanvasGroup CanvasGroup => _canvasGroup ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();

        public bool IsVisible
        {
            get => _isVisible;
            set => SetVisible(value);
        }

        protected virtual bool IsInteractable => true;
        protected virtual bool BlocksRaycasts => true;
        private void Awake()
        {
            SetVisible(false);
        }


        private void SetVisible(bool isVisible)
        {
            if (isVisible == _isVisible) return;
            _isVisible = isVisible;
            if (isVisible)
                Show();
            else
                Hide();
        }

        private void Show()
        {
            CanvasGroup.alpha = 1;
            CanvasGroup.interactable = IsInteractable;
            CanvasGroup.blocksRaycasts = BlocksRaycasts;
            OnBecameVisible();
        }
        protected void Hide()
        {
            CanvasGroup.alpha = 0;
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            OnBecameHidden();
        }
        
        protected virtual void OnBecameVisible() { }

        protected virtual void OnBecameHidden() { }
    }
    
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class HUDElement : UIElement
    {
        protected override bool BlocksRaycasts => false;
        protected override bool IsInteractable => false;
    }
}
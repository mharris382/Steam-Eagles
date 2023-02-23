using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : MonoBehaviour
    {
        private bool _isOpen;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                {
                    _rectTransform = GetComponent<RectTransform>();
                }

                return _rectTransform;
            }
        }

        public CanvasGroup CanvasGroup => _canvasGroup ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();


        protected virtual bool BlockRaycastsWhenVisible => true;

        public virtual void Open()
        {
            IsVisible = true;
        }

        public virtual void Close()
        {
            IsVisible = false;
        }

        public bool IsVisible
        {
            get  => CanvasGroup.alpha > 0;
            set => SetWindowVisible(value);
        }
        
        public void SetWindowVisible(bool visible)
        {
            if (visible)
            {
                CanvasGroup.alpha = 1;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = BlockRaycastsWhenVisible;
            }
            else
            {
                CanvasGroup.alpha = 0;
                CanvasGroup.interactable = false;
                CanvasGroup.blocksRaycasts = false;
            }
        }
    }
}
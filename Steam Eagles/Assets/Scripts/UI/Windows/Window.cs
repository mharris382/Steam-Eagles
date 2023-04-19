using System;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Window : MonoBehaviour
    {
        
        [Serializable]
        public class ActivationEvents
        {
            public UnityEvent onWindowOpened;
            public UnityEvent onWindowClosed;
            public UnityEvent<bool> onWindowStateChanged;

            public void SetWindow(bool open)
            {
                onWindowStateChanged?.Invoke(open);
                if (open)
                {
                    onWindowOpened?.Invoke();
                }
                else
                {
                    onWindowClosed?.Invoke();
                }
            }
        }
        
        public ActivationEvents activationEvents;
        
        private bool _isOpen;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        [ShowIf(nameof(UsesCloseButton))]
        public UnityEngine.UI.Button closeButton;
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
        private ReactiveProperty<bool> _isVisible = new ReactiveProperty<bool>();

        public IReadOnlyReactiveProperty<bool> IsVisibleProperty => _isVisible;
        public CanvasGroup CanvasGroup => _canvasGroup ? _canvasGroup : _canvasGroup = GetComponent<CanvasGroup>();


        protected virtual bool BlockRaycastsWhenVisible => true;
        
        protected virtual bool UsesCloseButton => true;

        private void Start()
        {
            Init();
        }

        public virtual void Init()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }
        }

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
            set
            {
                SetWindowVisible(value);
                _isVisible.Value = value;
            }
        }
        
        public void SetWindowVisible(bool visible)
        {
            activationEvents.SetWindow(visible);
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


 

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class StateWindow : MonoBehaviour
    {
        
    }
}
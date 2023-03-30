using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.Wheel
{
    [RequireComponent(typeof(Image))]
    public class UIWheelSegment : MonoBehaviour
    {
        private Image _image;
        private RectTransform _rectTransform;
        private List<Image> filledImages = new List<Image>();
        public Image Image => _image ? _image : _image = GetComponent<Image>();
        public RectTransform RectTransform => _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        [SerializeField] private Events events;

        public Color selectedColor;
        public Color normalColor;

        private UIWheelSelectable _selectable;
        public UIWheelSelectable Selectable => _selectable;
    
        [System.Serializable]
        public class Events
        {
            public UnityEvent<Sprite> OnSpriteChanged;
            public UnityEvent<string> OnNameChanged;
            public UnityEvent<bool> OnLockStateChanged;

            public void InvokeEvents(UIWheelSelectable selectable)
            {
                OnSpriteChanged?.Invoke(selectable.sprite);
                OnNameChanged?.Invoke(selectable.name);
                OnLockStateChanged?.Invoke(selectable.isLocked);
            }
        }

        private void Awake()
        {
            Image.fillMethod = Image.FillMethod.Radial360;
            normalColor = Image.color;
        }

        public void SetSelected(bool isSelected)
        {
            Image.color = isSelected ? selectedColor : normalColor;
        }
    
        public void Display(UIWheelSelectable selectable)
        {
            events.InvokeEvents(selectable);
            _selectable = selectable;
        }
    
        public float fillAmount
        {
            get => Image.fillAmount;
            set
            {
                Image.fillAmount = value;
                foreach (var filledImage in filledImages)
                {
                    filledImage.fillAmount = value;
                }
            }
        }

        public Vector3 Direction { get; set; }

        [Required]
        public RectTransform uiSlotContainer;


   

    }
}

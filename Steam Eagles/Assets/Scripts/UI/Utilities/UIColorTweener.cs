using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Utilities
{
    public class UIColorTweener : MonoBehaviour
    {
        public Graphic graphic;
        public Color selectedColor = Color.white;
        public Color highlightedColor = Color.green;
        public Color pressedColor = Color.grey;
        public Color disabledColor = Color.grey;
        public float tweenDuration = 0.2f;

        private Color _defaultColor;
        private TweenerCore<Color,Color,ColorOptions> _currentTween;

        void Awake()
        {
            _defaultColor = graphic.color;
        }
        public void ToSelectedColor()
        {
          ToColor(selectedColor);
        }
        public void ToHighlightedColor()
        {
            ToColor(highlightedColor);
        }
        public void ToPressedColor()
        {
            ToColor(pressedColor);
        }
        public void ToDisabledColor()
        {
            ToColor(disabledColor);
        }
        public void ToNormalColor()
        {
            ToColor(_defaultColor);
        }
        public void ToColor(Color c)
        {
            if (_currentTween != null)
                _currentTween.Kill();
            _currentTween = graphic.DOColor(c, tweenDuration);
        }
    }
}
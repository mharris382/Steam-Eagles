using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using Tools;
using UnityEngine;

namespace UI.Tool
{
    public class ModeNameDisplay : MonoBehaviour, IModeNameListener
    {
        public TextMeshPro modeNameText;
        public Color startColor = Color.black;
        public float fadeTime = 0.5f;
        public float fadeDelay = 0.5f;
        public Ease fadeEase = Ease.InOutSine;
        private TweenerCore<Color,Color,ColorOptions> _currentTween;


        public void DisplayModeName(string modeName)
        {
            if(_currentTween != null)
                _currentTween.Kill();
            
            modeNameText.text = modeName;
            modeNameText.color = startColor;
            
            this._currentTween =  modeNameText.DOFade(0, fadeTime).SetDelay(fadeDelay).SetEase(fadeEase);
            _currentTween.Play();
        }
    }
}
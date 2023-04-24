using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using Tools;
using UnityEngine;

namespace UI.Tool
{
    /// <summary>
    /// NOTE: this GUI element class is unique in that it does not exist on a Canvas and therefore does not need to
    /// use the HUDToolControllerBase to synchronize with the player.  Because it exist on the character prefab
    /// it can be referenced as an GameObject reference by the tool system.  This is a special case and should not
    /// be the default for UI elements.
    /// </summary>
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
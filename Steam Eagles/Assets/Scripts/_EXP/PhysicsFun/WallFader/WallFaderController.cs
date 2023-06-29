using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    /// <summary>
    /// handles fading FG walls in and out when the player enters the area.
    /// </summary>
    [RequireComponent(typeof(WallFaderBase))]
    
    public class WallFaderController : MonoBehaviour
    {
        private WallFaderBase _wfb;
        public WallFaderBase wfb => _wfb ? _wfb : (_wfb = GetComponent<WallFaderBase>());

        [SerializeField] float fadeTime = 1f;
        [SerializeField] private Ease fadeEase = Ease.InOutSine;
        [SerializeField] float timeToTriggerUnFade = 5;
        
        

        private Tween _fadeTween;
        private float Alpha
        {
            get => wfb.GetWallAlpha();
            set => wfb.SetWallAlpha(value);
        }


        public void FadeOut()
        {
            wfb.SetWallAlpha(0.0f);
        }

        public void FadeIn()
        {
            wfb.SetWallAlpha(1.0f);
        }
    }
    
    
}
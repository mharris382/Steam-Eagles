using System;
using DG.Tweening;
using UnityEngine;

namespace Characters.Cameras
{
    public class SplitScreenEdgeFader : MonoBehaviour, ISplitScreenTween
    {
        public GameObject canvasGroup;
        
        CanvasGroup _canvasGroup;
        void Awake()
        {
            if(!canvasGroup.TryGetComponent<CanvasGroup>(out _canvasGroup))
            {
                _canvasGroup = canvasGroup.AddComponent<CanvasGroup>();
            }
        
        }
        
        public Tween ToSplitScreenTween(float duration, ref float atPosition)
        {
            _canvasGroup.alpha = 0;
            return _canvasGroup.DOFade(1, duration)
                .SetAutoKill(false)
                .SetEase(Ease.InOutCubic);
        }
    }
}
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace UI.Wheel
{
    public class UIWheelAnimated : UIWheel
    {

        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public float openCloseDuration = 0.5f;
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public Vector2 openCloseScale = new Vector2(0.1f, 1);
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public Ease openCloseEase = Ease.OutBack;
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public float openCloseDelay = 0.1f;

         
        [System.Serializable] private class AngleOffset
        {
            public int number;
            public float angle;
        }

        private AngleOffset[] _offsets;
        private Sequence _openSequence;

        private bool _dirty;
        private Vector3 CloseScale => new Vector3(openCloseScale.x, openCloseScale.x, openCloseScale.x);
        private Vector3 OpenScale => new Vector3(openCloseScale.y, openCloseScale.y, openCloseScale.y);


        
       
        void CreateOpenCloseWheelAnimation()
        {
            _openSequence = DOTween.Sequence();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.localScale = CloseScale;
                _openSequence.Insert(openCloseDelay* (i+1), child.DOScale(OpenScale, openCloseDuration));
            }
            transform.localScale = CloseScale;
            _openSequence.Insert(0, transform.DOScale(OpenScale, openCloseDuration));
            _openSequence.SetEase(openCloseEase);
            _openSequence.SetAutoKill(false);
            _openSequence.SetRecyclable(true);
        }

        protected override   void OpenWheel()
        {
            UpdateSequence();
            _openSequence.PlayForward();
        }

        protected override  void CloseWheel()
        {
            UpdateSequence();
            _openSequence.PlayBackwards();
        }

        protected override  void UpdateSequence()
        {
            if (_openSequence == null || _dirty)
            {
                if(_openSequence != null) _openSequence.Kill();
                _dirty = false;
                CreateOpenCloseWheelAnimation();
            }
        }

 
        
        
        
    }
}
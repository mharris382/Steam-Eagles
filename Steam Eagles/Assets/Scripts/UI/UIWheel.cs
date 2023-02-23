using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.EventSystems;

namespace UI
{
    public class UIWheel : MonoBehaviour
    {
        [OnValueChanged(nameof(UpdateChildImages))]
        public float centerOffset = 4;
        [OnValueChanged(nameof(UpdateChildImages))]
        public float infoRadius = 1.5f;

        public float imageSize = 400;
        public int maxCount = 7;
        
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public float openCloseDuration = 0.5f;
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public Vector2 openCloseScale = new Vector2(0.1f, 1);
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public Ease openCloseEase = Ease.OutBack;
        [FoldoutGroup("Animation"), OnValueChanged(nameof(MarkDirty))]public float openCloseDelay = 0.1f;
        [TableList,SerializeField]
         private AngleOffset[] offsets;

         private bool _isOpen;
         private List<UIWheelSegment> _segments = new List<UIWheelSegment>();
         
         
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
        private void Awake()
        {
            _offsets = new AngleOffset[maxCount];
            foreach (var angleOffset in offsets)
            {
                if (angleOffset.number < maxCount)
                {
                    _offsets[angleOffset.number] = angleOffset;
                }
            }
            _isOpen = false;
        }


        
        private void OnEnable()
        {
            OpenWheel();
        }

        private void OnDisable()
        {
            CloseWheel();
        }

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

        void OpenWheel()
        {
           UpdateSequence();
            _openSequence.PlayForward();
        }

        void CloseWheel()
        {
            UpdateSequence();
            _openSequence.PlayBackwards();
        }

        void UpdateSequence()
        {
            if (_openSequence == null || _dirty)
            {
                if(_openSequence != null) _openSequence.Kill();
                _dirty = false;
                CreateOpenCloseWheelAnimation();
            }
        }

        void MarkDirty()
        {
            _dirty = true;
        }
        [Button]
        public void UpdateChildImages()
        {
            
            int segments = transform.childCount;
            if (segments < 2) return;
            float anglePerSegment = 360f / segments;
            float startAngle = anglePerSegment / 2f;
            if (Application.isPlaying)
            {
                try
                {
                    anglePerSegment += _offsets[segments-1].angle;
                }
                catch (IndexOutOfRangeException e)
                {
                    Debug.LogError("AngleOffset not found for " + segments + " segments!",this);
                }
            }
            else
            {
                foreach (var angleOffset in offsets)
                {
                    if (angleOffset.number == segments)
                    {
                        startAngle += angleOffset.angle;
                        break;
                    }
                }
            }
            
            Vector3 direction = Vector3.up;
            Quaternion rotation = Quaternion.Euler(0, 0, startAngle);
            Quaternion rotationStep = Quaternion.Euler(0, 0, anglePerSegment);
            _segments.Clear();
            for (int i = 0; i < segments; i++)
            {
                var child = transform.GetChild(i);
                var wheelSegment = child.GetComponent<UIWheelSegment>();
                wheelSegment.RectTransform.sizeDelta = new Vector2(imageSize, imageSize);
                wheelSegment.fillAmount = 1f / transform.childCount;
                var dir2 = Quaternion.Euler(0, 0, startAngle +((anglePerSegment * i) - anglePerSegment / 2f)) * Vector3.right;
                wheelSegment.Direction = dir2;
                child.localPosition = dir2 * centerOffset;
                child.localRotation = Quaternion.Euler(0, 0, startAngle + (anglePerSegment * i));
                //rotate the direction halfway between the last arc which is the center of the arc segment

                var infoContainer = wheelSegment.uiSlotContainer;
                infoContainer.position = transform.position + (dir2 * infoRadius);
                //info container should not be rotated with the arc
                infoContainer.rotation = Quaternion.identity;
                _segments.Add(wheelSegment);
            }
        }

        
        
        
    }
}
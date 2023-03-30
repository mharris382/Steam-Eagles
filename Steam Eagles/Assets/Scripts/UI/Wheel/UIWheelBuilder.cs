using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace UI.Wheel
{
    public class UIWheelBuilder : MonoBehaviour
    {
        public UIWheel wheel;
        
        [Required]
        public UIWheelController controller;
        
        [Required]
        [AssetsOnly] public UIWheelSegment segmentPrefab;


        
        private Transform _inactiveSegmentsParent;
        private IDisposable _lastWheelSubscription;

        private Queue<UIWheelSegment> _segments = new Queue<UIWheelSegment>();
        private List<UIWheelSegment> _activeSegments = new List<UIWheelSegment>();
        private void Awake()
        {
            _inactiveSegmentsParent = new GameObject("Inactive Segments").transform;
            _inactiveSegmentsParent.SetParent(transform);
            _inactiveSegmentsParent.gameObject.SetActive(false);
            
            _segments = new Queue<UIWheelSegment>();
            _activeSegments = new List<UIWheelSegment>();
            for (int i = 0; i < 7; i++)
            {
                _segments.Enqueue(Instantiate(segmentPrefab, _inactiveSegmentsParent));
            }
        }

        public IDisposable CreateWheel(List<UIWheelSelectable> selectables)
        {
            if(_lastWheelSubscription != null)
                _lastWheelSubscription.Dispose();
            Debug.Assert(_lastWheelSubscription==null);
            
            foreach (var selectable in selectables)
            {
                var segment = _segments.Dequeue();
                segment.transform.SetParent(wheel.transform);
                segment.Display(selectable);
                _activeSegments.Add(segment);
            }
            
            wheel.UpdateChildImages();
            var wheelData =   wheel.GetComponent<UIWheelData>();
            wheelData.SetWheelData(_activeSegments);
            wheelData.IsWheelOpen = true;
            _lastWheelSubscription = Disposable.Create(() =>
            {
                if(_lastWheelSubscription == null)return;
                ClearActiveSegments();
                wheelData.IsWheelOpen = false;
                _lastWheelSubscription = null;
            });
            return _lastWheelSubscription;
        }

        private void ClearActiveSegments()
        {
            foreach (var uiWheelSegment in _activeSegments)
            {
                uiWheelSegment.transform.SetParent(_inactiveSegmentsParent);
                _segments.Enqueue(uiWheelSegment);
            }

            _activeSegments.Clear();
        }
    }
}
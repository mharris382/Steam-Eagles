using System;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using Zenject;

namespace UI.Wheel
{
    public interface IWheelable<T>
    {
        IWheelConverter<T> Converter { get; }
        IEnumerable<T> GetItems();
    }
    
    
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
                var inst = Instantiate(segmentPrefab, _inactiveSegmentsParent);
                _segments.Enqueue(inst);
            }
        }

        public IDisposable CreateWheel<T>(IEnumerable<T> items, IWheelConverter<T> converter)
        {
            return CreateWheel(items.Select(converter.GetSelectableFor).ToList());
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
            _lastWheelSubscription = Disposable.Create(() => {
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
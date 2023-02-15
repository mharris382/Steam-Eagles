using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIWheelData : MonoBehaviour
    {
        public UnityEvent<bool> onWheelOpenStateChanged;
        
        private List<UIWheelSegment> _segments;
        private bool _isWheelOpen;

        public List<UIWheelSegment> Segments => _segments;

        public bool IsWheelOpen
        {
            get => _isWheelOpen;
            set
            {
                _isWheelOpen = value;
                onWheelOpenStateChanged?.Invoke(_isWheelOpen);
            }
        }
        public void SetWheelData(List<UIWheelSegment> segments)
        {
            _segments = segments;
        }


        public UIWheelSegment GetSegmentAtPosition(Vector2 selectedPosition)
        {
            float minAngleDif = float.MaxValue;
            UIWheelSegment selectedSegment = null;
            foreach (var uiWheelSegment in _segments)
            {
                var angle = Vector2.Angle(uiWheelSegment.Direction.normalized, selectedPosition.normalized);
                if(angle < minAngleDif)
                {
                    minAngleDif = angle;
                    selectedSegment = uiWheelSegment;
                }
            }
            return selectedSegment;
        }
    }
}
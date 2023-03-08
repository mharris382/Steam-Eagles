using System;
using Players;
using UniRx;
using UnityEngine;

namespace UI
{
    public class UIWheelInputHandler : MonoBehaviour
    {
        public UIWheelData wheel;
        public Player player;
        private Vector2 _selectedPosition;
        private bool _selectionMade;
        UIWheelSegment _lastSelectedSegment;
        private ReactiveProperty<UIWheelSegment> _selectedSegment = new ReactiveProperty<UIWheelSegment>();
        public Vector2 wheelCenter = Vector2.one/2f;
        public float minMouseRadius = 50;


        private void Awake()
        {
            _selectedSegment.Subscribe(OnWheelSegementSelectionChanged).AddTo(this);
        }
        
        void OnWheelSegementSelectionChanged(UIWheelSegment segment)
        {
            if (_lastSelectedSegment != null)
            {
                _lastSelectedSegment.SetSelected(false);
            }
            _lastSelectedSegment = segment;
            if (segment != null)
            {
                segment.SetSelected(true);
            }
        }

        public void OnWheelOpenChanged(bool isOpen)
        {
            if(isOpen)OnWheelOpened();
            else OnWheelClosed();
        }
        void OnWheelOpened()
        {
            _selectionMade = false;
            _selectedPosition = Vector2.zero;
            _selectedSegment.Value = null;
        }

        void OnWheelClosed()
        {
            if (_selectionMade)
            {
                _selectedSegment.Value.Selectable.onSelected.Invoke();
            }
        }

        private void Update()
        {
            if (wheel.IsWheelOpen)
            {
                UpdateWheel();
            }
        }

        void UpdateWheel()
        {
            Debug.Assert(player.InputWrapper != null, "Missing character input");
            Debug.Assert(player.InputWrapper.PlayerInput != null, "Missing player input");
            var playerInput = player.InputWrapper.PlayerInput;
            Vector2 aimDirection;
            if (playerInput.currentControlScheme.Contains("Keyboard"))
            {
                var mp = (Vector2)Input.mousePosition;
                var screenCenter = new Vector2(Screen.width, Screen.height) * wheelCenter;
                var dir = mp - screenCenter ;
                if(dir.magnitude < minMouseRadius)
                    aimDirection = Vector2.zero;
                else
                    aimDirection = dir.normalized;
            }
            else
            {
                aimDirection = playerInput.actions["Aim"].ReadValue<Vector2>();    
            }
            if (aimDirection != Vector2.zero)
            {
                _selectedPosition = aimDirection;
                _selectedSegment.Value = wheel.GetSegmentAtPosition(_selectedPosition);
                _selectionMade = true;
            }
        }
    }
}
using CoreLib;
using DG.Tweening;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;

namespace UI.Wheel
{
    [System.Obsolete("Use UIWheel instead")]
    public class UISelectorWheel : MonoBehaviour
    {
        public SharedInt SelectedIndex;
        
        public UISelectedAbilitySlot slotPrefab;
        

        [PropertyRange(0, nameof(numberOfItems))]
        [OnValueChanged(nameof(TestValue)),MaxValue(nameof(numberOfItems))] public int startItemIndex;
        [Min(2)] public int numberOfItems = 2;

        void TestValue()
        {
            selectedIndex = startItemIndex;
            SetRotation();
        }

        [OnValueChanged(nameof(SetRotation))]  [Wrap(-360, 360)] public float offsetRotation;
        [OnValueChanged(nameof(SetRotation))] public float min = 0;
        [OnValueChanged(nameof(SetRotation))] public float max = 360;
        
        
        [Header("Tween properties"), Range(0, 1)]
        public float movementDuration = 0.50f;
        public RotateMode rotateMode;
        public Ease movementEase = Ease.Linear;
        private int _selectedIndex;
        public int selectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                StartWheelMovement();
            }
        }

        private float _previousTargetRotation;
        private float _currentTargetRotation;

        public bool testUp;
        public bool testDown;
        public int testUpNumber = 1;
        
        private void Start()
        {
            if (SelectedIndex != null)
            {
                SelectedIndex.Value = 0;
                SelectedIndex.onValueChanged.AsObservable().TakeUntilDestroy(this).Subscribe(index => selectedIndex = index);
            }
        }
        
        
        
        public void Update()
        {
            if (testDown)
            {
                if (_selectedIndex == 0)
                {
                    selectedIndex = numberOfItems;
                }
                else
                {
                    selectedIndex--;
                }

                testDown = false;
                StartWheelMovement();
            }

            if (testUp)
            {
                _selectedIndex+= testUpNumber % numberOfItems;
                _selectedIndex %= numberOfItems;
                testUp = false;
                StartWheelMovement();
            }
        }

        
        
        public void StartWheelMovement()
        {
            _currentTargetRotation = GetDesiredRotation(_selectedIndex, numberOfItems, offsetRotation, min, max);
            var rt = this.GetComponent<RectTransform>();
            rt.DOKill();
            
            rt.DORotateQuaternion(Quaternion.Euler(0, 0, _currentTargetRotation), movementDuration).SetEase(movementEase)
                .Play().SetAutoKill(true);
        }

        void SetRotation()
        {
            GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, GetDesiredRotation(_selectedIndex, numberOfItems, offsetRotation, min, max));
        }
        
        private static float GetDesiredRotation(int targetIndex, int count, float rotationOffset, float min = 0, float max = 360)
        {
            float t =  (max / (float) count);

            float anglePerItemDeg = t;
            
            //float anglePerItemRad = anglePerItemDeg * Mathf.Deg2Rad;
            return targetIndex * anglePerItemDeg + rotationOffset;
        }
        
    }
}
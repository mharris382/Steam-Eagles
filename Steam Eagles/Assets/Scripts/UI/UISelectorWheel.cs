using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class UISelectorWheel : MonoBehaviour
    {
        [Min(2)]
        public int numberOfItems = 2;
        public UISelectedAbilitySlot slotPrefab;

        [Range(-360, 360)]
        public float offsetRotation;
        [Header("Tween properties"), Range(0, 1)]
        public float movementDuration = 0.50f;

        public float min = 0;
        public float max = 360;
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
        
        private static float GetDesiredRotation(int targetIndex, int count, float rotationOffset, float min = 0, float max = 360)
        {
            float t =  (max / (float) count);

            float anglePerItemDeg = t;
            
            //float anglePerItemRad = anglePerItemDeg * Mathf.Deg2Rad;
            return targetIndex * anglePerItemDeg + rotationOffset;
        }
        //
        //
        // IEnumerable<Vector3> GetWaypoints(float radius, int count, int resolution = 1)
        // {
        //     var tr = transform;
        //     Vector3 center = tr.position;
        //     Vector3 r0 =  Quaternion.identity * tr.right;
        //     
        //     float anglePerItemDeg = 360f / count;
        //     
        //     for (int i = 0; i < count; i++)
        //     {
        //         var angle = anglePerItemDeg * i;
        //         r0 = Quaternion.Euler(0, 0, angle) * tr.right;
        //         yield return center + r0 * radius;
        //         if (resolution > 0)
        //         {
        //             float anglePerSubstep = anglePerItemDeg / resolution;
        //             for (int j = 0; j < resolution; j++)
        //             {
        //                 var r1 = Quaternion.Euler(0, 0, angle + anglePerSubstep * j) * Vector3.right;
        //                 yield return center + r1 * radius;
        //             }
        //         }
        //     }
        // }
        //
        // IEnumerable<Vector3[]> GetWaypointsSegmented(float radius, int count, int resolution = 1)
        // {
        //     var tr = transform;
        //     Vector3 center = tr.position;
        //     Vector3 r0 =  Quaternion.identity * tr.right;
        //     
        //     float anglePerItemDeg = 360f / count;
        //     
        //     for (int i = 0; i < count; i++)
        //     {
        //         var angle = anglePerItemDeg * i;
        //         r0 = Quaternion.Euler(0, 0, angle) * tr.right;
        //         //yield return center + r0 * radius;
        //         if (resolution > 0)
        //         {
        //             yield return GetWaypointDirectionsForSection(angle, i, resolution, anglePerItemDeg).Select(t => center + t).ToArray();
        //         }
        //     }
        // }
        //
        // IEnumerable<Vector3> GetWaypointDirectionsForSection(float startAngle, int i, int resolution, float anglePerItemDeg)
        // {
        //     var angle = anglePerItemDeg * i;
        //     
        //     var right = transform.right;
        //     Vector3 startDirection = Quaternion.Euler(0, 0, startAngle) * right;
        //     Vector3 endDirection = Quaternion.Euler(0, 0, startAngle + anglePerItemDeg) * right;
        //     resolution = Mathf.Min(resolution, 1);
        //     for (float t = 0; t < 1; t+= 1/(float)resolution)
        //     {
        //         yield return Vector3.Slerp(startDirection, endDirection, t);
        //     }
        // }
    }
}
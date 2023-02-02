using Sirenix.OdinInspector;
using UnityEngine;

namespace UI
{
    public class UISelectorWheel2 : MonoBehaviour
    {
        [Required, ChildGameObjectsOnly] public RectTransform itemParent;
        public int numberOfItems => itemParent.childCount;


        
       [OnValueChanged(nameof(AutoPositionChildren))]public float radius = 5;
       [OnValueChanged(nameof(AutoPositionChildren))][Wrap(-180, 180)] public float targetAngleOffset = 45;
       [OnValueChanged(nameof(AutoPositionChildren))][MinMaxSlider(-360, 360, true)] public Vector2 angleRange = new Vector2(0, 360);
        
        float minAngle => angleRange.x;
        float maxAngle => angleRange.y;
        
        
        void AutoPositionChildren()
        {
            if (itemParent == null) return;
            var angleStep = (maxAngle - minAngle) / (numberOfItems - 1);
            Vector3 targetAngle = Quaternion.Euler(0, 0, targetAngleOffset) * Vector3.right;
            var startAngle =  Quaternion.Euler(0, 0, minAngle) * targetAngle;
            var endAngle = Quaternion.Euler(0, 0, maxAngle) * targetAngle;
            for (int i = 0; i < itemParent.childCount; i++)
            {
                var child = itemParent.GetChild(i);
                var item = child.GetComponent<UISelectorWheelItem>();
                var angle = angleStep * i;
                var pos = Quaternion.Euler(0, 0, angle) * targetAngle;
                child.position = pos * radius;
            }
        }
    }
}
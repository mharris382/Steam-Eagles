using System;
using UniRx;
using UnityEngine;

namespace PhysicsFun
{
    [Serializable]
    public class WaterCheck
    {
        [SerializeField] private float checkRadius = 0.5f;
        [SerializeField] private BoolReactiveProperty isUnderWater = new BoolReactiveProperty(false);
    
        public IReadOnlyReactiveProperty<bool> IsUnderWater => isUnderWater;
        public float Radius => checkRadius;
        public bool IsSubmerged(Transform transform)
        {
            LayerMask mask = LayerMask.GetMask("Water");
            var pos = transform.position;
            var coll = Physics2D.OverlapCircle(pos, checkRadius, mask);
            return coll != null;
        }
    }

    
}
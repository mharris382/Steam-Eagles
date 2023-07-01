using System;
using UnityEngine;
using UnityEngine.Events;

namespace PhysicsFun.Airships
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CounterWeight : MonoBehaviour
    {
        public UnityEvent<Rigidbody2D> onWeightAttached;
        private DistanceJoint2D _distanceJoint;

        public DistanceJoint2D DistanceJoint2D => _distanceJoint;

        private void Awake()
        {
            _distanceJoint = GetComponent<DistanceJoint2D>();
        }
    }
}
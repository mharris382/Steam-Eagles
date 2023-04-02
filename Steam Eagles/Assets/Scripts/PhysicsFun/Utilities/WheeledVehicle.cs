using System;
using UnityEngine;

namespace Puzzles
{
    public class WheeledVehicle : MonoBehaviour
    {
        public Rigidbody2D chassis;
        public Vector2[] wheelOffsets;
        public WheelJoint2D[] wheels;

        public float maxWheelDistance = 1;
        
        public enum SetMode
        {
            IGNORE,
            TRUE,
            FALSE
        }

        [Range(0,1)]
        public float blendToStartOffset = 1;
        
        public SetMode useMotor;
        public float speed = 10;
        public float smoothing = 10; 
        Vector2[] _startingPositions;
        float _maxDistanceSqr;
        private Vector2 _smoothVelocity;
        public float suspension = 1;

        private void Awake()
        {
            _maxDistanceSqr = (maxWheelDistance * maxWheelDistance);
            _startingPositions = new Vector2[wheels.Length];
            for (int i = 0; i < wheels.Length; i++)
            {
                var wheel = wheels[i];
                var offset = wheelOffsets[i];
                _startingPositions[i] = (Vector2)chassis.transform.InverseTransformPoint(wheel.transform.position);

            }
        }

        private void Update()
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                var wheelJoint2D = wheels[i];
                var motor = wheelJoint2D.motor;
                motor.motorSpeed = speed;
                wheelJoint2D.motor = motor;
                bool useMotor = wheelJoint2D.useMotor;
                SetValue(setMode: this.useMotor, ref useMotor);
                wheelJoint2D.useMotor = useMotor;
                var diff = wheelJoint2D.attachedRigidbody.position - chassis.position;
                var distance = diff.sqrMagnitude;
                if (distance > _maxDistanceSqr)
                {
                    var newPosition =Vector2.Lerp(diff.normalized * Mathf.Sqrt(distance), chassis.transform.TransformPoint(_startingPositions[i]), blendToStartOffset);
                    if (smoothing > 0)
                    {
                        newPosition = Vector2.SmoothDamp(wheelJoint2D.attachedRigidbody.position, newPosition, ref _smoothVelocity, smoothing*Time.deltaTime);
                    }
                    wheelJoint2D.attachedRigidbody.position = newPosition;
                }
            }
        }

        void UpdateWheel(int i)
        {
            var wheel = wheels[i];
            var wheelPosition = wheel.attachedRigidbody.position;
            var wheelLocalPosition = (Vector2)chassis.transform.InverseTransformPoint(wheelPosition);
            var wheelStartPosition = wheelLocalPosition - wheel.connectedAnchor;
            var wheelDistance = Vector2.Distance(wheelStartPosition, _startingPositions[i]);
        }

        void SetValue(SetMode setMode, ref bool value)
        {
            switch (setMode)
            {
                case SetMode.IGNORE:
                    break;
                case SetMode.TRUE:
                    value = true;
                    break;
                case SetMode.FALSE:
                    value = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(setMode), setMode, null);
            }
        }

        public float Speed
        {
            get => speed;
            set => speed = value;
        }

        public bool UseMotor
        {
            get => useMotor == SetMode.TRUE;
            set => useMotor = value ? SetMode.TRUE : SetMode.FALSE;
        }
    }
}
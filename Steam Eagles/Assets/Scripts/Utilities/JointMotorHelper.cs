using System;
using UnityEngine;

namespace Utilities
{
    public class JointMotorHelper : MonoBehaviour
    {
        private float _motorSpeed;
        private bool _invertMotorSpeed;
        public float MotorSpeed
        {
            set
            {
                _motorSpeed = value;
                _onMotorSpeedSet?.Invoke(_invertMotorSpeed ? -_motorSpeed : _motorSpeed);
            }
        }
        
        public bool InvertMotorSpeed
        {
            set
            {
                _invertMotorSpeed = value;
                MotorSpeed = _motorSpeed;
            }
        }

        

        private SliderJoint2D _sliderJoint2D;
        private HingeJoint2D _hingeJoint;
        private event Action<float> _onMotorSpeedSet;
        private JointMotor2D _motor;
        private void Awake()
        {
            _sliderJoint2D = GetComponent<SliderJoint2D>();
            _hingeJoint = GetComponent<HingeJoint2D>();
            if (_sliderJoint2D != null)
            {
                _onMotorSpeedSet += f =>
                {
                    var motor = _sliderJoint2D.motor;
                    motor.motorSpeed = f;
                    _sliderJoint2D.motor = motor;
                };
            }

            if (_hingeJoint != null)
            {
                _onMotorSpeedSet += f =>
                {
                    var motor = _hingeJoint.motor;
                    motor.motorSpeed = f;
                    _hingeJoint.motor = motor;
                };
            }
        }
    }
}
using System;
using CoreLib.Interfaces;
using SteamEagles.Characters;
using UnityEngine;

namespace Characters
{
    public class CharacterClimbHandle : IDisposable
    {
        /// <summary>
        /// if we are climbing on something which is attached to a dynamic rigidbody the height will be local to the rigidbody, otherwise it will be global position
        /// </summary>
        public Rigidbody2D attachRigidbody2D;

        [System.Obsolete("Use FixedJoint")]
        private SliderJoint2D _climbingSliderJoint;
        private FixedJoint2D _climbingFixedJoint;
        
        private readonly CharacterState _climber;
        private readonly IClimbable _climbable;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        private Vector2 _climbPosition;
        
        private bool _explicitlyDisposed;
        public bool  IsExplicitlyDisposed => _explicitlyDisposed;
        public bool IsDisposed => _climbingFixedJoint == null;
        
        public Vector2 MaxPosition
        {
            get => _maxPosition;
            set
            {
                if(IsDisposed) return;
                _maxPosition = value;
            }
        }
        public Vector2 MinPosition
        {
            get => _minPosition;
            set
            {
                if(IsDisposed) return;
                _minPosition = value;
            }
        }

        public Vector2 ClimbPosition
        {
            get => _climbPosition;
            set
            {
                _climbPosition = ClosestPointOnLine(MinPosition, MaxPosition, value);
                if(IsDisposed) return;
                ClimbingFixedJoint.connectedAnchor = _climbPosition;
            }
        }

        public SliderJoint2D ClimbingSliderJoint => _climbingSliderJoint== null ? _climbingSliderJoint = SetupNewJoint() : _climbingSliderJoint;

        public FixedJoint2D ClimbingFixedJoint => _climbingFixedJoint;

        SliderJoint2D SetupNewJoint()
        {
            Debug.Assert(_climber != null, "_climber == null");
            Debug.Assert(_climber.gameObject != null, "_climber.gameObject == null");
            
            _climbingSliderJoint = _climber.gameObject.AddComponent<SliderJoint2D>();
            _climbingSliderJoint.breakAction = JointBreakAction2D.Ignore;
            _climbingSliderJoint.connectedBody = attachRigidbody2D;
            _climbingSliderJoint.anchor = Vector2.zero;
            _climbingSliderJoint.connectedAnchor = (_maxPosition + _minPosition) / 2f;
            _climbingSliderJoint.autoConfigureConnectedAnchor = false;
            _climbingSliderJoint.angle = 90;
            _climbingSliderJoint.useLimits = true;
            _climbingSliderJoint.limits = new JointTranslationLimits2D
            {
                min = _minPosition.y,
                max = _maxPosition.y
            };
            return _climbingSliderJoint;
        }
        public CharacterClimbHandle(
            CharacterState climber,
            IClimbable climbable)
        {
            _climber = climber;
            Debug.Assert(_climber != null, "_climber == null");
            Debug.Assert(_climber.gameObject != null, "_climber.gameObject == null");
            _climbable = climbable;
            this._minPosition = climbable.MinClimbLocalPosition;
            this._maxPosition = climbable.MaxClimbLocalPosition;
            var position = climber.Rigidbody.transform.localPosition;
            ClimbPosition = ClosestPointOnLine(_minPosition, _maxPosition , position);
            this.attachRigidbody2D = climbable.Rigidbody;
            
            _climbingSliderJoint = null;
            
            _climbingFixedJoint = _climber.gameObject.AddComponent<FixedJoint2D>();
            _climbingFixedJoint.connectedBody = climbable.Rigidbody;
            _climbingFixedJoint.autoConfigureConnectedAnchor = false;
            _climbingFixedJoint.anchor = Vector2.zero;
            _climbingFixedJoint.connectedAnchor = ClimbPosition;
            
            _explicitlyDisposed = false;
        }

        public void Dispose()
        {
            if (!_explicitlyDisposed && _climbingFixedJoint == null)
            {
                Debug.LogWarning("Joint was destroyed without being explicitly disposed. this means unity destroyed it. this breaks the logic of the climbing system.");
            }
            if (!IsDisposed)
            {
                GameObject.Destroy(_climbingFixedJoint);
            }

            _explicitlyDisposed = true;
        }

        static Vector2 ClosestPointOnLine(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
        {
            Vector2 lineDirection = lineEnd - lineStart;
            float length = lineDirection.magnitude;
            lineDirection.Normalize();
            Vector2 pointDirection = point - lineStart;
            float dot = Vector2.Dot(pointDirection, lineDirection);
            float distance = Mathf.Clamp(dot, 0f, length);
            return lineStart + lineDirection * distance;
        }


        public void MoveClimber(float distance)
        {
            var upDirection = _maxPosition - _minPosition;
            upDirection.Normalize();
            ClimbPosition += upDirection * distance;
        }
    }
}
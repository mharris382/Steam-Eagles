using System;
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

        private SliderJoint2D _climbingJoint;
        private readonly CharacterState _climber;
        private readonly IClimbable _climbable;
        private Vector2 _minPosition;
        private Vector2 _maxPosition;
        
        private bool _explicitlyDisposed;
        public bool  IsExplicitlyDisposed => _explicitlyDisposed;
        public bool IsDisposed => _climbingJoint == null;
        
        public Vector2 MaxPosition
        {
            get => _maxPosition;
            set
            {
                if(IsDisposed) return;
                _maxPosition = value;
                _climbingJoint.limits = new JointTranslationLimits2D
                {
                    min = _minPosition.y,
                    max = _maxPosition.y
                };
            }
        }
        public Vector2 MinPosition
        {
            get => _minPosition;
            set
            {
                if(IsDisposed) return;
                _minPosition = value;
                _climbingJoint.limits = new JointTranslationLimits2D
                {
                    min = _minPosition.y,
                    max = _maxPosition.y
                };
            }
        }

        public SliderJoint2D ClimbingJoint => _climbingJoint== null ? _climbingJoint = SetupNewJoint() : _climbingJoint;


        SliderJoint2D SetupNewJoint()
        {
            Debug.Assert(_climber != null, "_climber == null");
            Debug.Assert(_climber.gameObject != null, "_climber.gameObject == null");
            
            _climbingJoint = _climber.gameObject.AddComponent<SliderJoint2D>();
            _climbingJoint.breakAction = JointBreakAction2D.Ignore;
            _climbingJoint.connectedBody = attachRigidbody2D;
            _climbingJoint.anchor = Vector2.zero;
            _climbingJoint.connectedAnchor = (_maxPosition + _minPosition) / 2f;
            _climbingJoint.autoConfigureConnectedAnchor = false;
            _climbingJoint.angle = 90;
            _climbingJoint.useLimits = true;
            _climbingJoint.limits = new JointTranslationLimits2D
            {
                min = _minPosition.y,
                max = _maxPosition.y
            };
            return _climbingJoint;
        }
        public CharacterClimbHandle(CharacterState climber, IClimbable climbable)
        {
            _climber = climber;
            Debug.Assert(_climber != null, "_climber == null");
            Debug.Assert(_climber.gameObject != null, "_climber.gameObject == null");
            _climbable = climbable;
            this._minPosition = climbable.MinClimbLocalPosition;
            this._maxPosition = climbable.MaxClimbLocalPosition;
            this.attachRigidbody2D = climbable.Rigidbody;
            _climbingJoint = _climber.gameObject.AddComponent<SliderJoint2D>();
            _climbingJoint.breakAction = JointBreakAction2D.Ignore;
            _climbingJoint.connectedBody = attachRigidbody2D;
            _climbingJoint.anchor = Vector2.zero;
            _climbingJoint.connectedAnchor = (_maxPosition + _minPosition) / 2f;
            _climbingJoint.autoConfigureConnectedAnchor = false;
            _climbingJoint.angle = 90;
            _climbingJoint.useLimits = true;
            _climbingJoint.limits = new JointTranslationLimits2D
            {
                min = _minPosition.y,
                max = _maxPosition.y
            };
            _explicitlyDisposed = false;
        }

        public void Dispose()
        {
            if (!_explicitlyDisposed && _climbingJoint == null)
            {
                Debug.LogWarning("Joint was destroyed without being explicitly disposed. this means unity destroyed it. this breaks the logic of the climbing system.");
            }
            if (!IsDisposed)
            {
                GameObject.Destroy(_climbingJoint);
            }

            _explicitlyDisposed = true;
        }
    }
}
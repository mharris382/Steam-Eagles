using System;
using System.Collections;
using UnityEngine;

namespace PhysicsFun
{
    /// <summary>
    /// used for rendering a sprite sheet representing a rotating object, where the axis
    /// of rotation is orthogonal to the camera, therefore the rotation is represented
    /// as a single frame in the sprite sheet
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatedSpriteRotor : MonoBehaviour
    {
        private Animator _anim;
        private Animator anim => _anim ? _anim : _anim = GetComponent<Animator>();
        
         
         private float _targetRotationSpeed = 1f;
         private float _actualRotationSpeed = 0;
         
         private static readonly int Powered = Animator.StringToHash("Powered");

         [SerializeField] private float accel = 2f;
         private static readonly int Speed = Animator.StringToHash("Speed");

         private Coroutine _coroutine;

         public bool testing = true;
         
         
         private void Awake()
        {
            _anim = GetComponent<Animator>();
        }
        
        public void SetRotationSpeed(float speed)
        {
            _targetRotationSpeed = speed;
            anim.SetFloat(Speed, speed);
            UpdateRotationSpeed(_targetRotationSpeed);
        }

       
        public void SetRotorPowered(bool powered)
        {
            //_anim.SetBool(Powered, powered);
            UpdateRotationSpeed(powered ? _targetRotationSpeed : 0);
        }

        public void SetRotationDirection(int direction)
        {
            direction = Mathf.Clamp(direction, -1, 1);
            SetRotationSpeed(direction);
        }
        
        private void UpdateRotationSpeed(float speed)
        {
            if(_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(AnimateSpeed(speed));
        }

        
        IEnumerator AnimateSpeed(float targetSpeed)
        {
            while (Mathf.Abs(_actualRotationSpeed - targetSpeed) > 0.01f)
            {
                _actualRotationSpeed = Mathf.MoveTowards(_actualRotationSpeed, targetSpeed, accel * Time.deltaTime);
                _anim.SetFloat(Speed, _actualRotationSpeed);
                yield return null;
            }
            anim.SetFloat(Speed, _actualRotationSpeed);
        }
    }
}
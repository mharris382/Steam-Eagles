using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.Events;

public class GroundCheck : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<bool> OnGroundedStateChanged;

    
    BoolReactiveProperty _isGroundedProperty = new BoolReactiveProperty(true);
    public bool IsGrounded
    {
        get => _isGroundedProperty.Value;
        private set => _isGroundedProperty.Value = value;
    }

    public IObservable<bool> IsGroundedEventStream => _isGroundedProperty ?? (_isGroundedProperty = new BoolReactiveProperty(false));

    public Transform groundCheckParent;

    public float GroundPercent
    {
        get;
        private set;
    }

    public float TimeSinceGrounded => Time.time - _lastGroundedTime;
    

    [SerializeField]
    protected Raycast2D[] _points ;//=> groundCheckParent.GetComponentsInChildren<Raycast2D>();


    public RaycastHit2D Hit { get; private set; }

    protected RaycastHit2D?[] hits;

    public bool MovingRight { get; set; }
    public float verticalVelocityThreshold = 1;
    private float _lastGroundedTime;
    private CharacterState _state;
    private float TimeInAir => Time.time - _lastGroundedTime;
    
    private void Awake()
    {
        _state = GetComponent<CharacterState>();
        _isGroundedProperty.Subscribe(gnded => OnGroundedStateChanged?.Invoke(gnded));
        var pnts = _points;
        hits = new RaycastHit2D?[_points.Length];
    }

 
    protected virtual void Update()
    {
            if (_state == null) return;

            if ((_state.IsJumping && (_state.VelocityY > verticalVelocityThreshold)) || _state.IsDropping)
            {
                IsGrounded = false;
                GroundPercent = 0;
                return;
            }
            int hitCount = 0;
            for (int i = 0; i < _points.Length; i++)
            {
                var pnt = _points[i];
                if (pnt.CheckForHit())
                {
                    IsGrounded = true;
                    Hit = pnt.Hit2D;
                    hitCount++;
                    hits[i] = Hit;
                }
                else
                {
                    hits[i] = null;
                }
            }

            GroundPercent = hitCount / (float)_points.Length;
            IsGrounded = hitCount > 0;
            if (IsGrounded)
            {
                _lastGroundedTime = Time.time;
            }
            
            
   }
    
}
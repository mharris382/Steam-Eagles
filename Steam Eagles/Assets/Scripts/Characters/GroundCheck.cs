using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UnityEngine.Events;

public interface IGroundCheck
{
    bool IsGrounded { get; }
    
    Vector2 GroundNormal { get; }
}
public class GroundCheck : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<bool> OnGroundedStateChanged;

    private bool _isGrounded;
    public bool IsGrounded
    {
        get => _isGrounded;
        private set
        {
            if (value != _isGrounded)
            {
                _isGrounded = value;
                OnGroundedStateChanged?.Invoke(value);
            }
        }
    }


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
        _points = groundCheckParent.GetComponentsInChildren<Raycast2D>();
        hits = new RaycastHit2D?[_points.Length];
    }

    private IEnumerator Start()
    {
        yield return null;
        OnGroundedStateChanged?.Invoke(IsGrounded);
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
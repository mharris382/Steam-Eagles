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

    [SerializeField]
    protected Raycast2D[] _points ;//=> groundCheckParent.GetComponentsInChildren<Raycast2D>();


    public RaycastHit2D Hit { get; private set; }


    protected Dictionary<Transform, Raycast2D> _checkPoints = new Dictionary<Transform, Raycast2D>();

    private CharacterState _state;
    private void Awake()
    {
        _state = GetComponent<CharacterState>();
        _isGroundedProperty.Subscribe(gnded => OnGroundedStateChanged?.Invoke(gnded));
        var pnts = _points;
        foreach (var groundCheckPoint in pnts)
        {
            _checkPoints.Add(groundCheckPoint.transform, groundCheckPoint);
        }
    }

    public bool MovingRight { get; set; }
    public float verticalVelocityThreshold = 1;
    protected virtual void Update()
    {
            Raycast2D mid = null; 
            if (_state == null) return;
            if (_state.VelocityY > verticalVelocityThreshold && _state.JumpHeld)
            {
                IsGrounded = false;
                Hit = new RaycastHit2D();
                return;
            }
            string order = "";
            foreach (var pnt in _points)
            {
                if (pnt.CheckForHit())
                {
                    IsGrounded = true;
                    Hit = pnt.Hit2D;
                    return;
                }
            }
            IsGrounded = false;
            
        return;
   }

    public void FixedUpdate()
    {
        if (_state == null) return;
        if (_state.VelocityY > 0)
        {
            
        }
    }

    private bool GetPoint(int i, out Raycast2D point)
    {
        point = null;
        if (_checkPoints.ContainsKey(groundCheckParent.GetChild(i)) == false) return false;
        point = _checkPoints[groundCheckParent.GetChild(i)];
        return true;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using UnityEngine;
using UniRx;

using UnityEngine.Events;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

public interface IGroundCheck
{
    bool IsGrounded { get; }
    
    Vector2 GroundNormal { get; }

    event Action<bool> OnGroundedStateChanged;
}

public class GroundCheck : MonoBehaviour, IGroundCheck
{
    [SerializeField, PreviouslySerializedAs("OnGroundedStateChanged")]
    private UnityEvent<bool> onGroundedStateChanged;
    public Transform groundCheckParent;
    public float verticalVelocityThreshold = 1;
    
    [SerializeField] protected Raycast2D[] _points ;//=> groundCheckParent.GetComponentsInChildren<Raycast2D>();


    public event Action<bool> OnGroundedStateChanged
    {
        add
        {
            if (value == null) return;
            if (_listeners.ContainsKey(value)) return;
            _listeners.Add(value, onGroundedStateChanged.AsObservable().Subscribe(value));
        }
        remove
        {
            if (value == null) return;
            if (!_listeners.ContainsKey(value)) return;
            _listeners[value].Dispose();
            _listeners.Remove(value);
        }
    }

    public bool IsGrounded
    {
        get => _isGrounded;
        private set
        {
            if (value != _isGrounded)
            {
                _isGrounded = value;
                onGroundedStateChanged?.Invoke(value);
            }
        }
    }

    public float GroundPercent
    {
        get;
        private set;
    }

    public float TimeSinceGrounded => Time.time - _lastGroundedTime;
    
    public bool MovingRight { set; get; }
    public RaycastHit2D Hit { get; private set; }
    public Vector2 GroundNormal => IsGrounded ? Hit.normal : Vector2.up;


    private bool _isGrounded;
    private Dictionary<Action<bool>, IDisposable> _listeners = new Dictionary<Action<bool>, IDisposable>();
    private RaycastHit2D?[] hits;
    private float _lastGroundedTime;
    private CharacterState _state;

    private void Awake()
    {
        _state = GetComponent<CharacterState>();
        _points = groundCheckParent.GetComponentsInChildren<Raycast2D>();
        hits = new RaycastHit2D?[_points.Length];
    }

    private IEnumerator Start()
    {
        yield return null;
        onGroundedStateChanged?.Invoke(IsGrounded);
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


#if UNITY_EDITOR
[CustomEditor(typeof(GroundCheck))]
public class GroundCheckEditor : OdinEditor
{
    private void OnSceneGUI()
    {
        var groundCheck = target as GroundCheck;
        if(groundCheck.groundCheckParent == null) return;
        var parent = groundCheck.groundCheckParent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var gndCheck = parent.GetChild(i);
            var pos = gndCheck.position;
            Handles.DrawWireDisc(pos, Vector3.forward, 0.2f);
            gndCheck.position = Handles.FreeMoveHandle(pos, Quaternion.identity, 0.03f, Vector3.zero, Handles.DotHandleCap);
        }
    }
}
#endif

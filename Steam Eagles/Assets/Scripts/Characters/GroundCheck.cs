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

    List<Raycast2D> _leftOrderedPoints = new List<Raycast2D>();
    List<Raycast2D> _rightOrderedPoints = new List<Raycast2D>();
    List<Raycast2D> _middleOrderedPoints = new List<Raycast2D>();
    Raycast2D _rightMostPoint;
    Raycast2D _leftMostPoint;
    private void Awake()
    {
        _state = GetComponent<CharacterState>();
        _points = groundCheckParent.GetComponentsInChildren<Raycast2D>();
        _leftOrderedPoints = new List<Raycast2D>(_points.OrderBy(t => t.transform.position.x));
        _rightOrderedPoints = new List<Raycast2D>(_points.OrderByDescending(t => t.transform.position.x));
        _middleOrderedPoints = new List<Raycast2D>(_points.OrderByDescending(t => Mathf.Abs(t.transform.position.x)));
        _rightMostPoint = _rightOrderedPoints[0];
        _leftMostPoint = _leftOrderedPoints[0];

        DebugOrder();
        hits = new RaycastHit2D?[_points.Length];

        void DebugOrder()
        {
            string leftDebug = $"{name} Left Ordered Points: ";
            foreach (var leftOrderedPoint in _leftOrderedPoints)
            {
                leftDebug += leftOrderedPoint.name + ", ";
            }

            string rightDebug = "Right Ordered Points: ";
            foreach (var rightOrderedPoint in _rightOrderedPoints)
            {
                rightDebug += rightOrderedPoint.name + ", ";
            }

            string middleDebug = "Middle Ordered Points: ";
            foreach (var midOrderedPoint in _middleOrderedPoints)
            {
                middleDebug += midOrderedPoint.name + ", ";
            }

            Debug.Log($"{leftDebug}\n{rightDebug}\n{middleDebug}");
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        onGroundedStateChanged?.Invoke(IsGrounded);
    }

    protected virtual void Update()
    {
        return;
        if (_state == null) return;
        int hitCount = 0;
        List<Raycast2D> orderedPoints = null;
        if (_state.IsDropping)
        {
            _state.IsGrounded = false;
            return;
        }

        if (Mathf.Abs(_state.MoveX) <= 0.1f)
        {
            orderedPoints = _middleOrderedPoints;
        }
        else if (_state.MoveX > 0)
        {
            orderedPoints = _rightOrderedPoints;
        }
        else
        {
            orderedPoints = _leftOrderedPoints;
        }
        for (int i = 0; i < orderedPoints.Count; i++)
        {
            var pnt = orderedPoints[i];
            if (pnt.CheckForHit())
            {
                IsGrounded = true;
                Hit = pnt.Hit2D;
                hitCount++;
                hits[i] = Hit;
                break;
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
            var fmh_199_61_638107055393666229 = Quaternion.identity; gndCheck.position = Handles.FreeMoveHandle(pos, 0.03f, Vector3.zero, Handles.DotHandleCap);
        }
    }
}
#endif

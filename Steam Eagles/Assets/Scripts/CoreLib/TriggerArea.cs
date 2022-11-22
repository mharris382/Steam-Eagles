using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TriggerArea : TriggerAreaBase<Rigidbody2D>
{
    public UnityEvent<Rigidbody2D> onTargetAdded;
    public UnityEvent<Rigidbody2D> onTargetRemoved;
    public UnityEvent<int> onTargetCountChanged;
    
    protected override bool HasTarget(Rigidbody2D rbTarget, out Rigidbody2D value)
    {
        value = rbTarget;
        return true;
    }

    protected override bool HasTarget(Collider2D target, out Rigidbody2D value)
    {
        value = null;
        if (target.attachedRigidbody == null) return false;
        value = target.attachedRigidbody;
        return true;
    }

    protected override void OnTargetAdded(Rigidbody2D target, int totalNumberOfTargets)
    {
        onTargetAdded?.Invoke(target);
        onTargetCountChanged?.Invoke(totalNumberOfTargets);
        base.OnTargetAdded(target, totalNumberOfTargets);
    }

    protected override void OnTargetRemoved(Rigidbody2D target, int totalNumberOfTargets)
    {
        onTargetRemoved?.Invoke(target);
        onTargetCountChanged?.Invoke(totalNumberOfTargets);
        base.OnTargetRemoved(target, totalNumberOfTargets);
    }
}

//TODO: feature request - allow TriggerAreaBase to buffer the messages so that if for example a player were to jump back and forth between a trigger, it would not constantly trigger the event until a min time passes.  it should also bias the most recent state
public abstract class TriggerAreaBase<T> : MonoBehaviour
{
    private Dictionary<Collider2D, T> _targetsInArea = new Dictionary<Collider2D, T>();
    private Dictionary<Rigidbody2D, T> _rbTargetsInArea = new Dictionary<Rigidbody2D, T>();
    private List<T> _targets = new List<T>();

    public bool searchColliderForTargets = true;
    public bool searchRigidbodyForTargets = true;
    public bool debug = true;
    public T GetEarliestTarget() => _targets.Count == 0 ? default : _targets[^1];

    public T GetLatestTarget() => _targets.Count == 0 ? default : _targets[0];

    public T GetNearestTarget()
    {
        Vector2 pos = transform.position;
        float minDist = float.MaxValue;
        T nearest = default;
        foreach (var rb in _rbTargetsInArea)
        {
            var distSqr = (rb.Key.position - pos).sqrMagnitude;
            if (distSqr < minDist)
            {
                minDist = distSqr;
                nearest = rb.Value;
            }
        }

        return nearest;
    }
    
    public T GetNearestTarget(float angle, float tolerance = 5)
    {
        Vector2 pos = transform.position;
        
        float minAngleDiff = float.MaxValue;
        float minDist = float.MaxValue;
        //convert angle to vector
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        
        T nearest = default;
        foreach (var rb in _rbTargetsInArea)
        {
            var diff = (rb.Key.position - pos);
            var angleDiff = Vector2.Angle(dir, diff.normalized);
            if (angleDiff - minAngleDiff < tolerance)
            {
                if(diff.sqrMagnitude < minDist)
                {
                    minDist = diff.sqrMagnitude;
                    minAngleDiff = angleDiff;
                    nearest = rb.Value;
                }
            }   
        }
        return nearest;
    }

    public T GetNearestTarget(float angle)
    {
        Vector2 pos = transform.position;
        
        float minAngleDiff = float.MaxValue;
        
        //convert angle to vector
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        
        T nearest = default;
        foreach (var rb in _rbTargetsInArea)
        {
            var angleDiff = Vector2.Angle(dir, (rb.Key.position - pos).normalized);
            if (angleDiff < minAngleDiff)
            {
                minAngleDiff = angleDiff;
                nearest = rb.Value;
            }      
        }
        return nearest;
    }
    
    public int GetTargetCount() => _targets.Count;
    
    public T GetTarget(int index) => _targets[index];

    private void Awake()
    {
        Debug.Assert(searchColliderForTargets || searchRigidbodyForTargets);
        enabled = searchColliderForTargets || searchRigidbodyForTargets;
        if (!enabled)
        {
            Debug.LogWarning("This Trigger Area will never detect any targets", this);
        }
    }

    protected abstract bool HasTarget(Rigidbody2D rbTarget, out T value);
    protected abstract bool HasTarget(Collider2D target, out T value);

    protected virtual void OnTargetAdded(T target, int totalNumberOfTargets)
    {
        if (debug)
        {
            Debug.Log($"Added: <b>{target}</b>\t Total Count = {totalNumberOfTargets}");
        }
    }

    protected virtual void OnTargetRemoved(T target, int totalNumberOfTargets)
    {
        if (debug)
        {
            Debug.Log($"Removed: <b>{target}</b>\t Total Count = {totalNumberOfTargets}");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (searchColliderForTargets && HasTarget(col, out var value))
        {
            if (_targetsInArea.ContainsKey(col))
            {
                return;
            }
            _targetsInArea.Add(col, value);
            AddTarget(value);
        }

        if (searchRigidbodyForTargets && col.attachedRigidbody != null &&
            !_rbTargetsInArea.ContainsKey(col.attachedRigidbody) && HasTarget(col.attachedRigidbody, out var valu2))
        {
            if (_rbTargetsInArea.ContainsKey(col.attachedRigidbody))
            {
                return;
            }
            _rbTargetsInArea.Add(col.attachedRigidbody, valu2);
            AddTarget(valu2);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_targets.Count == 0) return;
        if (_targetsInArea.ContainsKey(other))
        {
            var target = _targetsInArea[other];
            _targetsInArea.Remove(other);
            RemoveTarget(target);
        }

        if (other.attachedRigidbody != null && _rbTargetsInArea.ContainsKey(other.attachedRigidbody))
        {
            var target2 = _rbTargetsInArea[other.attachedRigidbody];
            _rbTargetsInArea.Remove(other.attachedRigidbody);
            RemoveTarget(target2);
        }
    }

    private void AddTarget(T target)
    {
        if (_targets.Contains(target))
            return;
        _targets.Add(target);
        OnTargetAdded(target, _targets.Count);
    }

    private void RemoveTarget(T target)
    {
        if (!_targets.Contains(target))
        {
            return;
        }

        _targets.Remove(target);
        OnTargetRemoved(target, _targets.Count);
    }
}
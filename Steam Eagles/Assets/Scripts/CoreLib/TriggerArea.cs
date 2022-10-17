using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TriggerAreaBase<T> : MonoBehaviour
{

    private Dictionary<Collider2D, T> _targetsInArea = new Dictionary<Collider2D, T>();
    private Dictionary<Rigidbody2D, T> _rbTargetsInArea = new Dictionary<Rigidbody2D, T>();
    private List<T> _targets = new List<T>();

    public bool searchColliderForTargets = true;
    public bool searchRigidbodyForTargets = true;
    public bool debug = true;
    public T GetEarliestTarget() => _targets.Count == 0 ? default  : _targets[^1];

    public T GetLatestTarget() => _targets.Count == 0 ? default : _targets[0];

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
            _targetsInArea.Add(col, value);
            AddTarget(value);
        }
        if (searchRigidbodyForTargets && col.attachedRigidbody != null && HasTarget(col.attachedRigidbody, out var valu2))
        {
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
        if(_targets.Contains(target))
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
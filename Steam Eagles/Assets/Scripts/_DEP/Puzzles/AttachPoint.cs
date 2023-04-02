using System;
using Puzzles;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
[System.Obsolete("This prototype is probably worth saving as a reference for future features, but will not be used directly as it's replacement will be tied to the item/tool/inventory system")]
public class AttachPoint : MonoBehaviour
{
    public bool automaticAttach;
    public TriggerArea triggerArea;
    public Joint2D attachJoint;
    private Rigidbody2D _itemRB;
    private HoldableItem _holdableItem;
    private Joint2D[] _joints;
    private IDisposable _disposable;
    public Events events;
    
    [Serializable]
    public class Events
    {
        public UnityEvent<GameObject> onObjAttached;
        public UnityEvent<GameObject> onObjDetached;
    }
    
    public bool HasTankAttached() => AttachedHoldableItem != null;
    public HoldableItem AttachedHoldableItem
    {
        get => _holdableItem;
        set
        {
            if (value == null)
            {
                _holdableItem = null;
                _itemRB = null;
            }
            else
            {
                _holdableItem = value;
                _itemRB = value.GetComponent<Rigidbody2D>();
            }
        }
    }

    private void Awake()
    {
        this._joints = attachJoint.GetComponents<Joint2D>();
        if (automaticAttach)
        {
            triggerArea.onTargetAdded.AsObservable().Where(t => !HasTankAttached())
                .Select(t => t == null ? null : t.GetComponent<HoldableItem>()).Where(t => t != null)
                .TakeUntilDestroy(this)
                .Subscribe(AttachGasTank);
        }
      
    }

    public void Attach(HoldableItem heldItem, Rigidbody2D heldBody, Rigidbody2D holderBody)
    {
        AttachGasTank(heldItem);
    }

    public virtual bool AllowedToAttachItem(HoldableItem item)
    {
        return true;
    }
    public void AttachGasTank(HoldableItem gasTank)
    {
        if (!AllowedToAttachItem(gasTank))
        {
            return;
        }
        AttachedHoldableItem = gasTank;
        if (_holdableItem.IsHeld)
        {
            AttachedHoldableItem = null;
            return;
        }
        
        foreach (var joint2D in _joints) joint2D.connectedBody = _itemRB;
        _disposable = _holdableItem.IsHeldStream.Where(t => t).Take(1).Subscribe(t => DisconnectGasTank());
        events.onObjAttached.Invoke(_holdableItem.gameObject);
    }

    private void DisconnectGasTank()
    {
        if (AttachedHoldableItem == null) return;

        _disposable?.Dispose();
        
        foreach (var joint2D in _joints) joint2D.connectedBody = null;

        events.onObjDetached?.Invoke(AttachedHoldableItem.gameObject);
        AttachedHoldableItem = null;
    }
}
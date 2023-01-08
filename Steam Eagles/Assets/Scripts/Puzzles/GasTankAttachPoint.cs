using System;
using System.Collections;
using System.Collections.Generic;
using GasSim;
using Puzzles;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.Events;


public class GasTankAttachPoint : MonoBehaviour
{
    public TriggerArea triggerArea;
    public Joint2D attachJoint;
    public IGasTank attachedGasTank;
    public Events events;

    [Tooltip(
        "If enabled the attach point will grab objects that are held by players when object enters trigger area.  Otherwise held objects will only be grabbed when the player releases them.")]
    public bool allowAttachHeldItems;

    public bool lockTankUntilEmpty = true;

    [Serializable]
    public class Events
    {
        public UnityEvent<IGasTank> onTankAttached;
        public UnityEvent<IGasTank> onTankDetached;
        public UnityEvent<GameObject> onTankObjAttached;
        public UnityEvent<GameObject> onTankObjDetached;


    }

    private Rigidbody2D _tankRB;
    private SpriteRenderer _tankSpriteRenderer;
    private GasTankState _gasTankState;

    private HoldableItem _holdableItem;

    //private Valve _valve;
    private Joint2D _joints;
    private IDisposable _disposable;

    public IGasTank AttachedTank
    {
        set
        {
            if (value == null)
            {
                _holdableItem = null;
                _tankRB = null;
                _tankSpriteRenderer = null;
                _gasTankState = null;
                attachedGasTank = null;
            }
            else
            {
                _holdableItem = value.gameObject.GetComponent<HoldableItem>();
                //if(_holdableItem.IsHeld) _holdableItem.Dropped(_holdableItem.HeldBy);
                _tankRB = value.gameObject.GetComponent<Rigidbody2D>();
                _tankSpriteRenderer = value.gameObject.GetComponent<SpriteRenderer>();
                _gasTankState = value.gameObject.GetComponent<GasTankState>();
                attachedGasTank = value;
            }

        }
    }



    private void DisconnectGasTank()
    {
        if (attachedGasTank == null) return;

        _disposable?.Dispose();
        _gasTankState.IsConnected = false;
        //foreach (var joint2D in _joints) joint2D.connectedBody = null;

        events.onTankDetached?.Invoke(attachedGasTank);
        AttachedTank = null;
        attachJoint.enabled = false;
    }

    public bool HasTankAttached() => attachedGasTank != null;


    public void AttachGasTank(IGasTank gasTank)
    {
        AttachedTank = gasTank;
        if (_holdableItem.IsHeld)
        {
            AttachedTank = null;
            return;
        }

        if(lockTankUntilEmpty)StartCoroutine(LockUntilEmpty(gasTank));
        _gasTankState.IsConnected = true;
        _joints.connectedBody = _tankRB;
        _joints.enabled = true;
        _disposable = _holdableItem.IsHeldStream.Where(t => t).Take(1).Subscribe(t => DisconnectGasTank());
        events.onTankAttached.Invoke(attachedGasTank);
    }

    
    IEnumerator LockUntilEmpty(IGasTank gasTank)
    {
        _colliders = gasTank.gameObject.GetComponents<Collider2D>();
        foreach (var collider2D1 in _colliders) collider2D1.enabled = false;
        while (HasTankAttached() && gasTank.StoredAmountNormalized > 0.25f)
        {
            yield return null;
        }
        
        foreach (var collider2D1 in _colliders) collider2D1.enabled = true;
    }

    Collider2D[] _colliders = new Collider2D[10];
    
    private void Awake()
    {
        this._joints = attachJoint;
        
        triggerArea.onTargetAdded.AsObservable().Where(t => !HasTankAttached())
            .Select(t => t == null ? null : t.GetComponent<IGasTank>()).Where(t => t != null)
            .TakeUntilDestroy(this)
            .Subscribe(AttachGasTank);
        
        events.onTankAttached.AsObservable().TakeUntilDestroy(this).Select(t => t.gameObject)
            .Subscribe(t => events.onTankObjAttached?.Invoke(t));
        
        events.onTankDetached.AsObservable().TakeUntilDestroy(this).Select(t => t.gameObject)
            .Subscribe(t => events.onTankObjDetached?.Invoke(t));

        attachJoint.enabled = false;
        _joints.enabled = false;
    }
    
    IEnumerator Start()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.1f);
            if (!HasTankAttached()) yield return null;
            else
            {
                if (lockTankUntilEmpty && attachedGasTank.StoredAmountNormalized < 0.25f)
                {
                    foreach (var collider2D1 in _colliders) collider2D1.enabled = true;
                }
                else if(_holdableItem.IsHeld)
                    DisconnectGasTank();
            }
        }
    }
}
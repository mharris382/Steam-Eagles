using System;
using System.Collections;
using System.Collections.Generic;
using GasSim;
using Puzzles;
using UniRx;
using UnityEngine;
using UnityEngine.Events;


public class GasTankAttachPoint : MonoBehaviour
{
    public TriggerArea triggerArea;
    public Joint2D attachJoint;
    public GasTank attachedGasTank;
    public Events events;
    [Tooltip("If enabled the attach point will grab objects that are held by players when object enters trigger area.  Otherwise held objects will only be grabbed when the player releases them.")]
    public bool allowAttachHeldItems;
    
    [Serializable]
    public class Events
    {
        public UnityEvent<GasTank> onTankAttached;
        public UnityEvent<GasTank> onTankDetached;
        public UnityEvent<GameObject> onTankObjAttached;
        public UnityEvent<GameObject> onTankObjDetached;

      
    }

    private Rigidbody2D _tankRB;
    private SpriteRenderer _tankSpriteRenderer;
    private GasTankState _gasTankState;
    private HoldableItem _holdableItem;
    //private Valve _valve;
    private Joint2D[] _joints;
    private IDisposable _disposable;
    public GasTank AttachedTank
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
                _holdableItem = value.GetComponent<HoldableItem>();
                //if(_holdableItem.IsHeld) _holdableItem.Dropped(_holdableItem.HeldBy);
                _tankRB = value.GetComponent<Rigidbody2D>();
                _tankSpriteRenderer = value.GetComponent<SpriteRenderer>();
                _gasTankState = value.GetComponent<GasTankState>();
                attachedGasTank = value;
            }
         
        }
    }
    
    

    private void DisconnectGasTank()
    {
        if (attachedGasTank == null) return;
        
        _disposable?.Dispose();
        _gasTankState.IsConnected = false;
        foreach (var joint2D in _joints) joint2D.connectedBody = null;
        
        events.onTankDetached?.Invoke(attachedGasTank);
        AttachedTank = null;
    }

    public bool HasTankAttached() => attachedGasTank != null;
    

    public void AttachGasTank(GasTank gasTank)
    {
        AttachedTank = gasTank;
        if (_holdableItem.IsHeld)
        {
            AttachedTank = null;
            return;
        }
        
        _gasTankState.IsConnected = true;
        foreach (var joint2D in _joints) joint2D.connectedBody = _tankRB;
        _disposable = _holdableItem.IsHeldStream.Where(t => t).Take(1).Subscribe(t => DisconnectGasTank());
        events.onTankAttached.Invoke(attachedGasTank);
    }

    private void Awake()
    {
        this._joints = attachJoint.GetComponents<Joint2D>();
        triggerArea.onTargetAdded.AsObservable().Where(t => !HasTankAttached())
            .Select(t => t == null ? null : t.GetComponent<GasTank>()).Where(t => t != null)
            .TakeUntilDestroy(this)
            .Subscribe(AttachGasTank);
        events.onTankAttached.AsObservable().TakeUntilDestroy(this).Select(t => t.gameObject)
            .Subscribe(t => events.onTankObjAttached?.Invoke(t));
        
        events.onTankDetached.AsObservable().TakeUntilDestroy(this).Select(t => t.gameObject)
            .Subscribe(t => events.onTankObjDetached?.Invoke(t));
    }
    IEnumerator Start()
    {
        while (enabled)
        {
            yield return new WaitForSeconds(0.1f);
            if (!HasTankAttached()) yield return null;
            else
            {
                if(_holdableItem.IsHeld)
                    DisconnectGasTank();
            }
        }
    }
}
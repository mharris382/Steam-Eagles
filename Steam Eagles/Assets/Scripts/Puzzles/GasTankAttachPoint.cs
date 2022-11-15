using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;


public class GasTankAttachPoint : MonoBehaviour
{
    public TriggerArea triggerArea;
    public Joint2D attachJoint;
    public GasTank attachedGasTank;
    
    
    public bool HasTankAttached() => attachedGasTank != null;
    

    public void AttachGasTank(GasTank gasTank)
    {
        attachedGasTank = gasTank;
        var rb = gasTank.GetComponent<Rigidbody2D>();
        if (rb == null) return;
        var joints = attachJoint.GetComponents<Joint2D>();
        foreach (var joint2D in joints)
        {
            
        }
    }

    private void Awake()
    {
        triggerArea.onTargetAdded.AsObservable().Where(t => !HasTankAttached())
            .Select(t => t == null ? null : t.GetComponent<GasTank>()).Where(t => t != null)
            .TakeUntilDestroy(this)
            .Subscribe(AttachGasTank);
    }
}



using System;
using System.Collections;
using System.Collections.Generic;
using StateMachine;
using UnityEngine;

/// <summary>
/// used as a workaround for 
/// </summary>
public class PositionTracker : MonoBehaviour
{
    public SharedTransform tpTransform;
    public SharedTransform bdTransform;

    private bool HasTargets => tpTransform.HasValue && bdTransform.HasValue;


    private void Update()
    {
        var tpPos = tpTransform.Position;
        var bdPos = bdTransform.Position;
        transform.position = (bdPos - tpPos) / 2f;
    }
}

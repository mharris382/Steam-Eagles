using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimIOSink : MonoBehaviour, IGasSink
{
    public Vector3 Position
    {
        get { return transform.position; }
    }
    public bool isActive { get; }
    public int TryRemoveGas(int amount)
    {
        throw new System.NotImplementedException();
    }
}

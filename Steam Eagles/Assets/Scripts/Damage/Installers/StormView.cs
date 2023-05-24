using System;
using UnityEngine;
using Zenject;

public class StormView : MonoBehaviour
{
    private IStormViewLayer[] _viewLayers;

    private void Awake()
    {
        _viewLayers = GetComponentsInChildren<IStormViewLayer>();
        Debug.Assert(_viewLayers.Length >= 1, "No storm view layers found!", this);
    }
    
    public class Factory : PlaceholderFactory<StormView>{ }
}
using System;
using GasSim;
using UniRx;
using UnityEngine;


public class ConnectablePowerSource : MonoBehaviour, IGasPowerSource
{
    private SpriteRenderer _sr;
    private IDisposable _disposable;

    public string sortingLayer = "Hypergas Generator";
    public int sortingOrder = -10;

    private int sortingLayerId;
    public float storedPower = 1;


    private float _storedPower;
    

    private IGasPowerSource _powerSource;
    public IGasPowerSource PowerSource
    {
        get => _powerSource;
        set
        {
            _powerSource = value;
        }
    }
    
    private void Awake()
    {
        sortingLayerId= SortingLayer.NameToID(sortingLayer);
        
    }

    public void OnObjectTankAttached(GameObject gasTank)
    {
        PowerSource = gasTank.GetComponent<IGasPowerSource>();
        if (PowerSource == null) return;
        _storedPower = storedPower;
        this._sr = gasTank.GetComponentInChildren<SpriteRenderer>();
        
        
        var prevSortingOrder = this._sr.sortingOrder;
        var prevSortingLayer = this._sr.sortingLayerID;
        
        _disposable = Disposable.Create(() =>
        {
            if(_sr.sortingOrder == sortingOrder) _sr.sortingOrder = prevSortingOrder;
            if(_sr.sortingLayerID == sortingLayerId) _sr.sortingLayerID = prevSortingLayer;
        });
    }

    public void OnGasTankDetached(GameObject gasTank)
    {
        PowerSource = null;
        _disposable?.Dispose();
    }

    public float PowerCapacity => PowerSource == null ? storedPower : PowerSource.PowerCapacity;
    public float AvailablePower => PowerSource == null ? 0 : PowerSource.AvailablePower;

    public void ConsumePower(float amount)
    {
        if(PowerSource != null) PowerSource.ConsumePower(amount);
        else
        {
            _storedPower -= amount;
            _storedPower = Mathf.Max(0, _storedPower);
        }
    }
}
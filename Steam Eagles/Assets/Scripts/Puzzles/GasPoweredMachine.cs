using System;
using UniRx;
using UnityEngine;

public class GasPoweredMachine : MonoBehaviour
{
    private SpriteRenderer _sr;
    private IDisposable _disposable;

    public string sortingLayer = "Hypergas Generator";
    public int sortingOrder = -10;

    private int sortingLayerId;
    private void Awake()
    {
        sortingLayerId= SortingLayer.NameToID(sortingLayer);
        
    }

    public void OnGasTankAttached(GasTank gasTank)
    {
        this._sr = gasTank.GetComponent<SpriteRenderer>();
        gasTank.transform.SetAsLastSibling();
        
        var prevSortingOrder = this._sr.sortingOrder;
        var prevSortingLayer = this._sr.sortingLayerID;
        
        _disposable = Disposable.Create(() =>
        {
            if(_sr.sortingOrder == sortingOrder) _sr.sortingOrder = prevSortingOrder;
            if(_sr.sortingLayerID == sortingLayerId) _sr.sortingLayerID = prevSortingLayer;
        });
    }

    public void OnGasTankDetached(GasTank gasTank)
    {
        _disposable?.Dispose();
    }
    
}